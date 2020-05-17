using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Collections;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;

namespace Reese.Nav
{
    /// <summary>Creates and updates destinations as persistent entities that
    /// retain location information pertinent to nav agents.</summary>
    [UpdateAfter(typeof(NavSurfaceSystem))]
    class NavDestinationSystem : JobComponentSystem
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            var createJob = Entities
                .WithChangeFilter<NavNeedsDestination>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, in NavNeedsDestination destination) =>
                {
                    var collider = SphereCollider.Create(
                        new SphereGeometry()
                        {
                            Center = destination.Value,
                            Radius = 1
                        },
                        new CollisionFilter() // TODO : Resolve via Issue #3.
                        {
                            BelongsTo = ~0u,
                            CollidesWith = ~0u,
                            GroupIndex = 0
                        }
                    );

                    unsafe
                    {
                        var castInput = new ColliderCastInput()
                        {
                            Collider = (Collider*)collider.GetUnsafePtr(),
                            Orientation = quaternion.identity
                        };

                        if (!physicsWorld.CastCollider(castInput, out ColliderCastHit hit) || hit.RigidBodyIndex == -1)
                        {
                            commandBuffer.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity); // Ignore invalid destinations.
                            return;
                        }

                        agent.DestinationSurface = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

                        agent.LocalDestination = NavUtil.MultiplyPoint3x4(
                            math.inverse(localToWorldFromEntity[agent.DestinationSurface].Value),
                            destination.Value
                        ) + agent.Offset;

                        commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                    }
                })
                .WithName("CreateDestinationJob")
                .Schedule(JobHandle.CombineDependencies(
                    inputDeps,
                    buildPhysicsWorld.FinalJobHandle
                ));

            barrier.AddJobHandleForProducer(createJob);

            return createJob;
        }
    }
}
