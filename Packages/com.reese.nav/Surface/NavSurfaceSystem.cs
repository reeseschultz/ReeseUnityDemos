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
    /// surface (or lack thereof) underneath a given NavAgent. It also maintains
    /// parent-child relationships.</summary>
    [UpdateAfter(typeof(NavBasisSystem))]
    public class NavSurfaceSystem : JobComponentSystem
    {
        static ConcurrentDictionary<int, bool> needsSurfaceDictionary = new ConcurrentDictionary<int, bool>();

        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var defaultBasis = World.GetExistingSystem<NavBasisSystem>().DefaultBasis;

            // Below job is needed because Unity.Physics removes the Parent
            // component for dynamic bodies.
            var addParentToSurfaceJob = Entities
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

                    commandBuffer.AddComponent<LocalToParent>(entityInQueryIndex, entity);
                })
                .WithoutBurst()
                .WithName("NavAddParentToSurfaceJob")
                .Schedule(inputDeps);

            barrier.AddJobHandleForProducer(addParentToSurfaceJob);

            // Below job is needed so users don't have to manually add the
            // Parent and LocalToParent components when spawning agents.
            var addParentToAgentJob = Entities
                .WithNone<Parent>()
                .ForEach((Entity entity, int entityInQueryIndex, in NavAgent agent) =>
                {
                    commandBuffer.AddComponent<Parent>(entityInQueryIndex, entity);
                    commandBuffer.AddComponent<LocalToParent>(entityInQueryIndex, entity);
                })
                .WithoutBurst()
                .WithName("NavAddParentToAgentJob")
                .Schedule(addParentToSurfaceJob);

            barrier.AddJobHandleForProducer(addParentToAgentJob);

            // Below job is needed because Unity.Transforms assumes that
            // children should be scaled by their surface by automatically
            // providing them with a CompositeScale.
            var removeCompositeScaleJob = Entities
                .WithAll<CompositeScale>()
                .WithAny<NavSurface, NavBasis>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    commandBuffer.RemoveComponent<CompositeScale>(entityInQueryIndex, entity);
                })
                .WithName("NavRemoveCompositeScaleJob")
                .Schedule(addParentToAgentJob);

            barrier.AddJobHandleForProducer(removeCompositeScaleJob);

            var elapsedSeconds = (float)Time.ElapsedTime;
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            var navSurfaceTrackingJob = Entities
                .WithNone<NavFalling, NavJumping>()
                .WithAll<NavNeedsSurface, LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Parent surface, ref Translation translation, in LocalToWorld localToWorld) =>
                {
                    if (
                        !surface.Value.Equals(Entity.Null) &&
                        needsSurfaceDictionary.GetOrAdd(entity.Index, true)
                    ) return;

                    needsSurfaceDictionary[entity.Index] = false;

                    var rayInput = new RaycastInput
                    {
                        Start = localToWorldFromEntity[entity].Position + agent.Offset,
                        End = -localToWorld.Up *NavConstants.SURFACE_RAYCAST_DISTANCE_MAX,
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = NavUtil.ToBitMask(NavConstants.COLLIDER_LAYER),
                            CollidesWith = NavUtil.ToBitMask(NavConstants.SURFACE_LAYER),
                        }
                    };

                    if (!physicsWorld.CastRay(rayInput, out RaycastHit hit))
                    {
                        if (++agent.SurfaceRaycastCount >= NavConstants.SURFACE_RAYCAST_MAX)
                        {
                            agent.FallSeconds = elapsedSeconds;

                            commandBuffer.RemoveComponent<NavNeedsSurface>(entityInQueryIndex, entity);
                            commandBuffer.AddComponent<NavFalling>(entityInQueryIndex, entity);
                        }

                        return;
                    }

                    agent.SurfaceRaycastCount = 0;
                    surface.Value = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                    commandBuffer.RemoveComponent<NavNeedsSurface>(entityInQueryIndex, entity);

                    if (!jumpBufferFromEntity.Exists(entity)) return;
                    var jumpBuffer = jumpBufferFromEntity[entity];
                    if (jumpBuffer.Length < 1) return;

                    translation.Value = jumpBuffer[0].Value + agent.Offset;

                    jumpBuffer.Clear();

                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                })
                .WithoutBurst()
                .WithName("NavSurfaceTrackingJob")
                .Schedule(
                    JobHandle.CombineDependencies(
                        removeCompositeScaleJob,
                        buildPhysicsWorld.FinalJobHandle
                    )
                );

            barrier.AddJobHandleForProducer(navSurfaceTrackingJob);

            return navSurfaceTrackingJob;
        }
    }
}
