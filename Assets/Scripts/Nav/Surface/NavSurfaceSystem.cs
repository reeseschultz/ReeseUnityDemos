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
    class NavSurfaceSystem : JobComponentSystem
    {
        /// <summary>For knowing whether or not the NavAgent has attempted
        /// jumping since the last time this system ran. Helps filter out
        /// agents to prevent unnecessary raycasts checking for a surface
        /// below. In other words, if the agent didn't jump and the system
        /// isn't starting up for the first time, then the surface is known.
        /// </summary>
        static ConcurrentDictionary<int, bool> hasJumpedDictionary = new ConcurrentDictionary<int, bool>();

        /// <summary>For knowing how many times raycasting has been conducted
        /// from the negative y-component of the agent (while jumping) to detect
        /// a surface below. If no surface is detected and
        /// NavConstants.RAYCAST_MAX is exceeded for a given NavAgent, then
        /// NavAgent.IsFalling is set to true. Raycasting then stops. An example
        /// of how to handle falling is in NavFallSystem, but feel free to use
        /// whatever implementation you want, hence why NavFallSystem is
        /// namespaced in Reese.Demo and not Reese.Nav.</summary>
        static ConcurrentDictionary<int, int> raycastCountDictionary = new ConcurrentDictionary<int, int>();

        /// <summary>Used for raycasting in order to detect a surface below a
        /// given NavAgent.</summary>
        BuildPhysicsWorld buildPhysicsWorldSystem => World.GetExistingSystem<BuildPhysicsWorld>();

        /// <summary>For adding Parent and LocalToParent components when they
        /// or the Parent.Value are nonexistent on a given NavSurface.</summary>
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            var parentFromEntity = GetComponentDataFromEntity<Parent>(true);
            var defaultBasis = World.GetExistingSystem<NavBasisSystem>().DefaultBasis;

            // Below job is needed because Unity.Physics can remove the Parent
            // component, at least in 2019.3, thus it has to be added later at
            // runtime and not in authoring. Please submit an issue or PR if
            // you've a cleaner solution.
            var addParentJob = Entities
                .WithReadOnly(parentFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in NavSurface surface) =>
                {
                    if (parentFromEntity.Exists(entity)) return;

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

            parentFromEntity = GetComponentDataFromEntity<Parent>();

            var elapsedSeconds = (float)Time.ElapsedTime;
            var physicsWorld = buildPhysicsWorldSystem.PhysicsWorld;

            return Entities
                .WithChangeFilter<NavAgent>()
                .WithReadOnly(elapsedSeconds)
                .WithReadOnly(defaultBasis)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(parentFromEntity)
                .ForEach((Entity entity, ref NavAgent agent, in Translation translation) =>
                {
                    if (agent.IsFalling) return;

                    if (!hasJumpedDictionary.ContainsKey(entity.Index)) hasJumpedDictionary.TryAdd(entity.Index, agent.HasJumped);

                    hasJumpedDictionary.TryGetValue(entity.Index, out bool hasJumped);

                    if (!parentFromEntity.HasComponent(entity)) return;

                    var parent = parentFromEntity[entity];
                    if (!parent.Value.Equals(Entity.Null) && hasJumped == agent.HasJumped) return;

                    hasJumpedDictionary[entity.Index] = agent.HasJumped = false;

                    if (!raycastCountDictionary.ContainsKey(entity.Index)) raycastCountDictionary.TryAdd(entity.Index, 0);

                    var rayInput = new RaycastInput
                    {
                        Start = translation.Value,
                        End = -math.up() * NavConstants.SURFACE_RAYCAST_DISTANCE_MAX,
                        Filter = CollisionFilter.Default
                    };

                    if (!physicsWorld.CastRay(rayInput, out RaycastHit hit) || hit.RigidBodyIndex == -1)
                    {
                        agent.HasJumped = true;
                        raycastCountDictionary.TryGetValue(entity.Index, out int raycastCount);
                        raycastCountDictionary[entity.Index] = ++raycastCount;

                        if (raycastCount >= NavConstants.RAYCAST_MAX)
                        {
                            agent.Surface = Entity.Null;
                            agent.IsFalling = true;
                            agent.FallSeconds = elapsedSeconds;
                        }

                        return;
                    }

                    raycastCountDictionary[entity.Index] = 0;

                    agent.Surface = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

                    if (!parentFromEntity.HasComponent(agent.Surface)) return;

                    var parentBasis = parentFromEntity[agent.Surface].Value;

                    parent.Value = parentBasis.Equals(Entity.Null) ? defaultBasis : parentBasis;

                    parentFromEntity[entity] = parent;
                })
                .WithoutBurst()
                .WithName("NavSurfaceTrackingJob")
                .Schedule(
                    JobHandle.CombineDependencies(
                        addParentJob,
                        buildPhysicsWorldSystem.FinalJobHandle
                    )
                );
        }
    }
}
