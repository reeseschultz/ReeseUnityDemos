using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using Unity.Collections;
using UnityEngine;

namespace Reese.Nav
{
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    class NavDestinationSystem : JobComponentSystem
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;

            var job = Entities
                .WithChangeFilter<NavNeedsDestination>()
                .ForEach((ref NavAgent agent, in NavNeedsDestination destination) =>
                {
                    var collider = SphereCollider.Create(
                        new SphereGeometry() {
                            Center = destination.Value,
                            Radius = 1
                        },
                        new CollisionFilter()
                        {
                            BelongsTo = ~0u,
                            CollidesWith = ~0u,
                            GroupIndex = 0
                        }
                    );

                    unsafe {
                        var castInput = new ColliderCastInput()
                        {
                            Collider = (Collider*)collider.GetUnsafePtr(),
                            Orientation = quaternion.identity
                        };

                        if (!physicsWorld.CastCollider(castInput, out ColliderCastHit hit) || hit.RigidBodyIndex == -1) return;

                        Debug.Log(physicsWorld.Bodies[hit.RigidBodyIndex].Entity);
                    }
                })
                .WithoutBurst()
                .WithName("NavDestinationJob")
                .Schedule(
                    JobHandle.CombineDependencies(
                        inputDeps,
                        buildPhysicsWorld.FinalJobHandle
                    )
                );

            job.Complete();

            return job;
        }
    }
}
