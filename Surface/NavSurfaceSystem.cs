using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Reese.Nav
{
    /// <summary>The primary responsibility of this system is to track the
    /// surface (or lack thereof) underneath a given NavAgent. It also maintains
    /// parent-child relationships.</summary>
    [UpdateAfter(typeof(NavBasisSystem))]
    public class NavSurfaceSystem : SystemBase
    {
        Dictionary<int, GameObject> gameObjectMap = new Dictionary<int, GameObject>();
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        public bool GameObjectMapContainsKey(int key)
            => gameObjectMap.ContainsKey(key);

        public bool GameObjectMapContainsValue(GameObject go)
            => gameObjectMap.ContainsValue(go);

        public int GameObjectMapCount()
            => gameObjectMap.Count;

        public Dictionary<int, GameObject>.KeyCollection GameObjectMapKeys()
            => gameObjectMap.Keys;

        public Dictionary<int, GameObject>.ValueCollection GameObjectMapValues()
            => gameObjectMap.Values;

        public void GameObjectMapAdd(int key, GameObject value)
            => gameObjectMap.Add(key, value);

        public bool GameObjectMapRemove(int key)
            => gameObjectMap.Remove(key);

        public bool GameObjectMapTryGetValue(int key, out GameObject value)
            => gameObjectMap.TryGetValue(key, out value);

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var defaultBasis = World.GetExistingSystem<NavBasisSystem>().DefaultBasis;

            // Prevents Unity.Physics from removing the Parent component from dynamic bodies:
            Entities
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
                .WithName("NavAddParentToSurfaceJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            // Adds Parent and LocalToParent components when to agents:
            Entities
                .WithNone<NavHasProblem, Parent>()
                .ForEach((Entity entity, int entityInQueryIndex, in NavAgent agent) =>
                {
                    commandBuffer.AddComponent<Parent>(entityInQueryIndex, entity);
                    commandBuffer.AddComponent<LocalToParent>(entityInQueryIndex, entity);
                })
                .WithName("NavAddParentToAgentJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            // Prevents Unity.Transforms from assuming that children should be scaled by their parent:
            Entities
                .WithAll<CompositeScale>()
                .WithAny<NavSurface, NavBasis>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    commandBuffer.RemoveComponent<CompositeScale>(entityInQueryIndex, entity);
                })
                .WithName("NavRemoveCompositeScaleJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            var elapsedSeconds = (float)Time.ElapsedTime;
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>();

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithNone<NavHasProblem, NavFalling, NavJumping>()
                .WithAll<NavNeedsSurface, LocalToParent>()
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                .WithNativeDisableParallelForRestriction(pathBufferFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Parent surface, ref Translation translation, in LocalToWorld localToWorld) =>
                {
                    if (!surface.Value.Equals(Entity.Null) && false) return;

                    var rayInput = new RaycastInput
                    {
                        Start = localToWorld.Position + agent.Offset,
                        End = -localToWorld.Up * NavConstants.SURFACE_RAYCAST_DISTANCE_MAX,
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

                    translation.Value.y = hit.Position.y + agent.Offset.y;

                    if (!jumpBufferFromEntity.HasComponent(entity)) return;
                    var jumpBuffer = jumpBufferFromEntity[entity];
                    if (jumpBuffer.Length < 1) return;

                    translation.Value = jumpBuffer[0].Value + agent.Offset;

                    jumpBuffer.Clear();

                    if (pathBufferFromEntity.HasComponent(entity)) pathBufferFromEntity[entity].Clear();

                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                })
                .WithName("NavSurfaceTrackingJob")
                .ScheduleParallel();

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            ////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////
            // TODO : Move following jobs and related code into transform extensions package. //
            ////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////

            // Corrects the translation of children with a parent not at the origin:
            Entities
                .WithChangeFilter<PreviousParent>()
                .WithAny<NavFixTranslation>()
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in PreviousParent previousParent, in Parent parent) =>
                {
                    if (previousParent.Value.Equals(Entity.Null) || !localToWorldFromEntity.HasComponent(parent.Value)) return;

                    var parentTransform = localToWorldFromEntity[parent.Value];

                    if (parentTransform.Position.Equals(float3.zero))
                    {
                        commandBuffer.RemoveComponent<NavFixTranslation>(entityInQueryIndex, entity);
                        return;
                    }

                    translation.Value = NavUtil.MultiplyPoint3x4(
                        math.inverse(parentTransform.Value),
                        translation.Value
                    );

                    commandBuffer.RemoveComponent<NavFixTranslation>(entityInQueryIndex, entity);
                })
                .WithName("NavFixTranslationJob")
                .ScheduleParallel();

            // Re-parents entities to ensure correct transform:
            Entities
                .WithNone<NavAgent, LocalToParent>()
                .ForEach((Entity entity, int entityInQueryIndex, in Parent parent) =>
                {
                    commandBuffer.RemoveComponent<Parent>(entityInQueryIndex, entity);

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new Parent
                    {
                        Value = parent.Value
                    });

                    commandBuffer.AddComponent<LocalToParent>(entityInQueryIndex, entity);
                })
                .WithName("NavReparentingJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
