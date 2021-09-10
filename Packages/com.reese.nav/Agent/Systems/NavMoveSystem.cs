using Reese.Math;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

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

        static void Rotate(float deltaSeconds, LocalToWorld destinationSurfaceLocalToWorld, LocalToWorld surfaceLocalToWorld, NavSteering steering, NavAgent agent, Translation translation, ref Rotation rotation)
        {
            var lookAt = (translation.Value + steering.CurrentHeading)
                .ToWorld(destinationSurfaceLocalToWorld)
                .ToLocal(surfaceLocalToWorld);

            lookAt.y = translation.Value.y;

            var lookRotation = quaternion.LookRotationSafe(lookAt - translation.Value, math.up());

            if (math.length(agent.SurfacePointNormal) > 0.01f) lookRotation = math.mul(lookRotation, math.up().FromToRotation(agent.SurfacePointNormal));

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

                        var destinationSurfaceLocalToWorld = localToWorldFromEntity[agent.DestinationSurface];
                        var surfaceLocalToWorld = localToWorldFromEntity[surface.Value];

                        Rotate(deltaSeconds, destinationSurfaceLocalToWorld, surfaceLocalToWorld, steering, agent, translation, ref rotation);
                    }
                )
                .WithName("NavMoveJob")
                .ScheduleParallel();
        }
    }
}
