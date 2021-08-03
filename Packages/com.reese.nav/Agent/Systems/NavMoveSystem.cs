using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Translates and rotates agents based on their current heading.</summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavSteeringSystem))]
    public class NavMoveSystem : SystemBase
    {
        static void Translate(float deltaSeconds, NavSteering steering, NavAgent agent, ref Translation translation)
            => translation.Value += steering.CurrentHeading * agent.TranslationSpeed * deltaSeconds;

        static void Rotate(float deltaSeconds, float4x4 destinationSurfaceLocalToWorld, float4x4 surfaceLocalToWorld, NavSteering steering, NavAgent agent, Translation translation, ref Rotation rotation)
        {
            var lookAt = NavUtil.MultiplyPoint3x4( // To world (from local in terms of destination surface).
                destinationSurfaceLocalToWorld,
                translation.Value + steering.CurrentHeading
            );

            lookAt = NavUtil.MultiplyPoint3x4( // To local (in terms of agent's current surface).
                math.inverse(surfaceLocalToWorld),
                lookAt
            );

            lookAt.y = translation.Value.y;

            var lookRotation = quaternion.LookRotationSafe(lookAt - translation.Value, math.up());

            if (math.length(agent.SurfacePointNormal) > 0.01f) lookRotation *= Quaternion.FromToRotation(math.up(), agent.SurfacePointNormal);

            rotation.Value = math.slerp(rotation.Value, lookRotation, deltaSeconds / agent.RotationSpeed);
        }

        protected override void OnUpdate()
        {
            var deltaSeconds = Time.DeltaTime;
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities
                .WithNone<NavProblem, NavPlanning>()
                .WithAll<NavWalking, LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .ForEach(
                    (Entity entity, ref Translation translation, ref Rotation rotation, in NavAgent agent, in NavSteering steering, in Parent surface) =>
                    {
                        if (agent.DestinationSurface.Equals(Entity.Null)) return;

                        Translate(deltaSeconds, steering, agent, ref translation);

                        var destinationSurfaceLocalToWorld = localToWorldFromEntity[agent.DestinationSurface].Value;
                        var surfaceLocalToWorld = localToWorldFromEntity[surface.Value].Value;

                        Rotate(deltaSeconds, destinationSurfaceLocalToWorld, surfaceLocalToWorld, steering, agent, translation, ref rotation);
                    }
                )
                .WithName("NavMoveJob")
                .ScheduleParallel();
        }
    }
}
