using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;

namespace Reese.Nav
{
    /// <summary>Creates and updates destinations as persistent entities that
    /// retain location information pertinent to nav agents.</summary>
    [UpdateAfter(typeof(NavSurfaceSystem))]
    public class NavDestinationSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithChangeFilter<NavNeedsDestination>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, in NavNeedsDestination needsDestination) =>
                {
                    var collider = SphereCollider.Create(
                        new SphereGeometry()
                        {
                            Center = needsDestination.Destination,
                            Radius = NavConstants.DESTINATION_SURFACE_COLLIDER_RADIUS
                        },
                        new CollisionFilter()
                        {
                            BelongsTo = NavUtil.ToBitMask(NavConstants.COLLIDER_LAYER),
                            CollidesWith = NavUtil.ToBitMask(NavConstants.SURFACE_LAYER),
                        }
                    );

                    unsafe
                    {
                        var castInput = new ColliderCastInput()
                        {
                            Collider = (Collider*)collider.GetUnsafePtr(),
                            Orientation = quaternion.identity
                        };

                        if (!physicsWorld.CastCollider(castInput, out ColliderCastHit hit))
                        {
                            commandBuffer.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity); // Ignore invalid destinations.
                            return;
                        }

                        var destination = NavUtil.MultiplyPoint3x4(
                            math.inverse(localToWorldFromEntity[hit.Entity].Value),
                            needsDestination.Destination
                        ) + agent.Offset;

                        if (needsDestination.Teleport)
                        {
                            commandBuffer.SetComponent<Parent>(entityInQueryIndex, entity, new Parent
                            {
                                Value = hit.Entity
                            });

                            commandBuffer.SetComponent<Translation>(entityInQueryIndex, entity, new Translation
                            {
                                Value = destination
                            });

                            commandBuffer.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);

                            return;
                        }

                        agent.DestinationSurface = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                        agent.LocalDestination = destination;

                        commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                    }
                })
                .WithName("CreateDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
