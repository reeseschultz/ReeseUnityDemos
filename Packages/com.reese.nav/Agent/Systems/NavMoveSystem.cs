using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>
    /// Takes the current heading from the NavSteeringSystem and moves the entity based on that.
    /// Also calculates and applies an appropriate rotation for the entity, so it always faces the direction of movement,
    /// even when other steering behaviors affect it.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavSteeringSystem))]
    public class NavMoveSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities
                .WithNone<NavProblem, NavPlanning>()
                .WithAll<NavWalking, LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .ForEach(
                    (Entity entity, ref Translation translation, ref Rotation rotation, in NavAgent agent,
                        in NavSteering navSteering, in Parent surface) =>
                    {
                        if (agent.DestinationSurface.Equals(Entity.Null)) return;

                        translation.Value += navSteering.CurrentHeading * agent.TranslationSpeed * deltaTime;

                        // Add rotation with flocking behavior steering included
                        var lookAt = NavUtil.MultiplyPoint3x4( // To world (from local in terms of destination surface).
                            localToWorldFromEntity[agent.DestinationSurface].Value,
                            translation.Value + navSteering.CurrentHeading
                        );

                        lookAt = NavUtil.MultiplyPoint3x4( // To local (in terms of agent's current surface).
                            math.inverse(localToWorldFromEntity[surface.Value].Value),
                            lookAt
                        );

                        lookAt.y = translation.Value.y;

                        var lookRotation = quaternion.LookRotationSafe(lookAt - translation.Value, math.up());

                        if (math.length(agent.SurfacePointNormal) > 0.01f)
                            lookRotation = Quaternion.FromToRotation(math.up(), agent.SurfacePointNormal) *
                                           lookRotation;

                        rotation.Value = math.slerp(rotation.Value, lookRotation, deltaTime / agent.RotationSpeed);
                    }
                )
                .WithName("NavMoveJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}