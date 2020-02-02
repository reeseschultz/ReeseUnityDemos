using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using Unity.Collections;
using System.Collections.Concurrent;

namespace Reese.Nav
{
    /// <summary>The primary responsibility of this system is to track the
    /// surface (or lack thereof) underneath a given NavAgent. It also ensures
    /// parent-child relationships are maintained in lieu of Unity.Physics'
    /// efforts to destroy them.</summary>
    [UpdateAfter(typeof(NavBasisSystem))]
    class NavSurfaceSystem : JobComponentSystem
    {
        /// <summary>For knowing whether or not the NavAgent has attempted
        /// jumping since the last time this system ran. Helps filter out
        /// agents to prevent unnecessary raycasts checking for a surface
        /// below. In other words, if the agent didn't jump and the system
        /// isn't starting up for the first time, then the surface is known.
        /// </summary>
        static ConcurrentDictionary<int, bool> needsSurfaceDictionary = new ConcurrentDictionary<int, bool>();

        /// <summary>For raycasting in order to detect a surface below a
        /// given NavAgent.</summary>
        BuildPhysicsWorld buildPhysicsWorldSystem => World.GetExistingSystem<BuildPhysicsWorld>();

        /// <summary>For adding Parent and LocalToParent components when they
        /// or the Parent.Value are nonexistent on a given NavSurface.</summary>
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var defaultBasis = World.GetExistingSystem<NavBasisSystem>().DefaultBasis;

            // Below job is needed because Unity.Physics has been observed to remove
            // the Parent component, thus it can only reliably be added later at
            // runtime and not in authoring :(. Please submit an issue or PR if
            // you've a cleaner solution.
            var addParentJob = Entities
                .WithNone<Parent>()
                .ForEach((Entity entity, int entityInQueryIndex, in NavSurface surface) =>
                {
                    if (surface.Basis.Equals(Entity.Null))
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new Parent
                        {
                            Value = defaultBasis
                        });
                    }
                    else
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new Parent
                        {
                            Value = surface.Basis
                        });
                    }

                    commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(LocalToParent));
                })
                .WithoutBurst()
                .WithName("NavAddParentToSurfaceJob")
                .Schedule(inputDeps);

            barrier.AddJobHandleForProducer(addParentJob);

            // Below job is needed because Unity.Transforms assumes that
            // children should be scaled by their parent by automatically
            // providing them with a CompositeScale. This is a default that
            // probably doesn't reflect 99% of use cases.
            var removeCompositeScaleJob = Entities
                .WithAll<CompositeScale>()
                .WithAny<NavSurface, NavBasis>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    commandBuffer.RemoveComponent<CompositeScale>(entityInQueryIndex, entity);
                })
                .WithName("NavRemoveCompositeScaleJob")
                .Schedule(addParentJob);

            var parentFromEntity = GetComponentDataFromEntity<Parent>();
            var elapsedSeconds = (float)Time.ElapsedTime;
            var physicsWorld = buildPhysicsWorldSystem.PhysicsWorld;

            return Entities
                .WithNone<NavFalling, NavJumping>()
                .WithAll<NavNeedsSurface, Parent, LocalToParent>()
                .WithNativeDisableParallelForRestriction(parentFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, in Translation translation) =>
                {
                    var parent = parentFromEntity[entity];

                    if (
                        !parent.Value.Equals(Entity.Null) &&
                        needsSurfaceDictionary.GetOrAdd(entity.Index, true)
                    ) return;

                    needsSurfaceDictionary[entity.Index] = false;

                    var rayInput = new RaycastInput
                    {
                        Start = translation.Value,
                        End = -math.up() * NavConstants.SURFACE_RAYCAST_DISTANCE_MAX,
                        Filter = CollisionFilter.Default
                    };

                    if (!physicsWorld.CastRay(rayInput, out RaycastHit hit) || hit.RigidBodyIndex == -1)
                    {
                        if (++agent.SurfaceRaycastCount >= NavConstants.SURFACE_RAYCAST_MAX)
                        {
                            agent.Surface = Entity.Null;
                            agent.FallSeconds = elapsedSeconds;

                            commandBuffer.RemoveComponent<NavNeedsSurface>(entityInQueryIndex, entity);
                            commandBuffer.AddComponent<NavFalling>(entityInQueryIndex, entity);
                        }

                        return;
                    }

                    agent.SurfaceRaycastCount = 0;
                    agent.Surface = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

                    if (!parentFromEntity.HasComponent(agent.Surface)) return;

                    var parentBasis = parentFromEntity[agent.Surface].Value;
                    parent.Value = parentBasis.Equals(Entity.Null) ? defaultBasis : parentBasis;

                    parentFromEntity[entity] = parent;

                    commandBuffer.RemoveComponent<NavNeedsSurface>(entityInQueryIndex, entity);
                })
                .WithoutBurst()
                .WithName("NavSurfaceTrackingJob")
                .Schedule(
                    JobHandle.CombineDependencies(
                        removeCompositeScaleJob,
                        buildPhysicsWorldSystem.FinalJobHandle
                    )
                );
        }
    }
}
