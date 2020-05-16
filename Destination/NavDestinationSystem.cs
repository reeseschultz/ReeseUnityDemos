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
            var parentFromEntity = GetComponentDataFromEntity<Parent>();
            var translationFromEntity = GetComponentDataFromEntity<Translation>();

            var createJob = Entities
                .WithChangeFilter<NavNeedsDestination>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(parentFromEntity)
                .WithNativeDisableParallelForRestriction(translationFromEntity)
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

                        if (!physicsWorld.CastCollider(castInput, out ColliderCastHit hit) || hit.RigidBodyIndex == -1) {
                            commandBuffer.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity); // Ignore invalid destinations.
                            return;
                        }

                        var surfaceEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

                        if (agent.Destination.Equals(Entity.Null)) {
                            var destinationEntity = commandBuffer.CreateEntity(entityInQueryIndex);

                            commandBuffer.AddComponent(entityInQueryIndex, destinationEntity, new NavDestination
                            {
                                Agent = entity
                            });

                            commandBuffer.AddComponent(entityInQueryIndex, destinationEntity, new Parent
                            {
                                Value = surfaceEntity
                            });

                            commandBuffer.AddComponent<LocalToParent>(entityInQueryIndex, destinationEntity);
                            commandBuffer.AddComponent<LocalToWorld>(entityInQueryIndex, destinationEntity);
                            commandBuffer.AddComponent<Translation>(entityInQueryIndex, destinationEntity);

                            commandBuffer.AddComponent(entityInQueryIndex, destinationEntity, new Translation
                            {
                                Value = NavUtil.MultiplyPoint3x4(
                                    math.inverse(localToWorldFromEntity[surfaceEntity].Value),
                                    destination.Value
                                ) + agent.Offset
                            });
                        } else {
                            var destinationParent = parentFromEntity[agent.Destination];
                            destinationParent.Value = surfaceEntity;
                            parentFromEntity[agent.Destination] = destinationParent;

                            var destinationTranslation = translationFromEntity[agent.Destination];
                            destinationTranslation.Value = NavUtil.MultiplyPoint3x4(
                                math.inverse(localToWorldFromEntity[surfaceEntity].Value),
                                destination.Value
                            ) + agent.Offset;
                            translationFromEntity[agent.Destination] = destinationTranslation;
                        }

                        commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                    }
                })
                .WithName("CreateDestinationJob")
                .Schedule(JobHandle.CombineDependencies(
                    inputDeps,
                    buildPhysicsWorld.FinalJobHandle
                ));

            barrier.AddJobHandleForProducer(createJob);

            var agentFromEntity = GetComponentDataFromEntity<NavAgent>();

            var mapJob = Entities
                .WithNativeDisableParallelForRestriction(agentFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavDestination destination) =>
                {
                    if (
                        destination.Agent.Equals(Entity.Null) ||
                        !agentFromEntity.Exists(destination.Agent)
                    ) return;

                    var agent = agentFromEntity[destination.Agent];

                    if (!agent.Destination.Equals(Entity.Null)) return;

                    agent.Destination = entity;
                    agentFromEntity[destination.Agent] = agent;
                })
                .WithName("DestinationMappingJob")
                .Schedule(createJob);

            return mapJob;
        }
    }
}
