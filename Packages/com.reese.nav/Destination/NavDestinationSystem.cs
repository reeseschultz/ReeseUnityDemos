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
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    class NavDestinationSystem : JobComponentSystem
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var needsDestinationFromEntity = GetComponentDataFromEntity<NavNeedsDestination>(true);

            var destroyDestinationJob = Entities
                .WithReadOnly(needsDestinationFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavDestination destination) => {
                    if (needsDestinationFromEntity.Exists(destination.Agent)) return;
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                })
                .Schedule(inputDeps);

            destroyDestinationJob.Complete();

            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var agentPrefab = EntityManager.CreateEntityQuery(typeof(NavAgentPrefab)).GetSingleton<NavAgentPrefab>().Value;

            Entities
                .WithChangeFilter<NavNeedsDestination>()
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, in NavNeedsDestination destination) =>
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

                        var surfaceEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                        var destinationEntity = EntityManager.Instantiate(agentPrefab); // TODO : Need a conditional destination marker prefab.

                        EntityManager.AddComponentData(destinationEntity, new NavDestination{
                            Agent = entity
                        });

                        EntityManager.AddComponentData(destinationEntity, new Parent{
                            Value = surfaceEntity
                        });

                        EntityManager.AddComponent<LocalToParent>(destinationEntity);
                        EntityManager.AddComponent<Translation>(destinationEntity);

                        EntityManager.AddComponentData(destinationEntity, new Translation{
                            Value = NavUtil.MultiplyPoint3x4(
                                math.inverse(GetComponentDataFromEntity<LocalToWorld>(true)[surfaceEntity].Value),
                                destination.Value
                            ) + agent.Offset
                        });

                        agent.Destination = destinationEntity;
                    }
                })
                .WithStructuralChanges()
                .Run();

            return destroyDestinationJob;
        }
    }
}
