using Reese.Math;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Reese.Nav
{
    /// <summary>Manages destinations for agents, rate-limiting their path searches.</summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavSurfaceSystem))]
    public partial class NavDestinationSystem : SystemBase
    {
        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
        BuildPhysicsWorld buildPhysicsWorld => World.GetOrCreateSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnStartRunning()
            => this.RegisterPhysicsRuntimeSystemReadWrite();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var elapsedSeconds = (float)Time.ElapsedTime;
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var settings = navSystem.Settings;

            Entities
                .WithNone<NavProblem>()
                .WithChangeFilter<NavDestination>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, in NavDestination destination) =>
                {
                    if (elapsedSeconds - agent.DestinationSeconds < settings.DestinationRateLimitSeconds)
                    {
                        commandBuffer.AddComponent<NavDestination>(entityInQueryIndex, entity, destination); // So that the change filter applies next frame.
                        return;
                    }

                    var collider = SphereCollider.Create(
                        new SphereGeometry()
                        {
                            Center = destination.WorldPoint,
                            Radius = settings.DestinationSurfaceColliderRadius
                        },
                        new CollisionFilter()
                        {
                            BelongsTo = NavUtil.ToBitMask(settings.ColliderLayer),
                            CollidesWith = NavUtil.ToBitMask(settings.SurfaceLayer),
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
                            commandBuffer.RemoveComponent<NavDestination>(entityInQueryIndex, entity); // Ignore invalid destinations.
                            return;
                        }

                        var localDestination = destination.WorldPoint.ToLocal(localToWorldFromEntity[hit.Entity]) + agent.Offset;

                        if (NavUtil.ApproxEquals(localDestination, agent.LocalDestination, destination.Tolerance)) return;

                        if (destination.Teleport)
                        {
                            commandBuffer.SetComponent<Parent>(entityInQueryIndex, entity, new Parent
                            {
                                Value = hit.Entity
                            });

                            commandBuffer.SetComponent<Translation>(entityInQueryIndex, entity, new Translation
                            {
                                Value = localDestination
                            });

                            commandBuffer.RemoveComponent<NavDestination>(entityInQueryIndex, entity);

                            return;
                        }

                        agent.DestinationSurface = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                        agent.LocalDestination = localDestination;
                        agent.DestinationSeconds = elapsedSeconds;

                        commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                    }
                })
                .WithName("CreateDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
            buildPhysicsWorld.AddInputDependencyToComplete(Dependency);
        }
    }
}
