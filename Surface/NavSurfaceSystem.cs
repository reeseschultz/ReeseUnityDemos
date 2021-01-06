using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>The primary responsibility of this system is to track the
    /// surface (or lack thereof) underneath a given NavAgent. It also maintains
    /// parent-child relationships.</summary>
    [UpdateAfter(typeof(NavBasisSystem))]
    public class NavSurfaceSystem : SystemBase
    {
        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
        Dictionary<int, GameObject> gameObjectMap = new Dictionary<int, GameObject>();
        NativeHashMap<int, bool> needsSurfaceMap = default;
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

        protected override void OnCreate()
            => needsSurfaceMap = new NativeHashMap<int, bool>(
                navSystem.Settings.NeedsSurfaceMapSize,
                Allocator.Persistent
            );

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var defaultBasis = World.GetExistingSystem<NavBasisSystem>().DefaultBasis;

            // Below job is needed because Unity.Physics removes the Parent
            // component for dynamic bodies.
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
                .WithoutBurst()
                .WithName("NavAddParentToSurfaceJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            // Below job is needed so users don't have to manually add the
            // Parent and LocalToParent components when spawning agents.
            Entities
                .WithNone<NavHasProblem, Parent>()
                .ForEach((Entity entity, int entityInQueryIndex, in NavAgent agent) =>
                {
                    commandBuffer.AddComponent<Parent>(entityInQueryIndex, entity);
                    commandBuffer.AddComponent<LocalToParent>(entityInQueryIndex, entity);
                })
                .WithoutBurst()
                .WithName("NavAddParentToAgentJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            // Below job is needed because Unity.Transforms assumes that
            // children should be scaled by their surface by automatically
            // providing them with a CompositeScale.
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
            var settings = navSystem.Settings;
            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var map = needsSurfaceMap;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithNone<NavHasProblem, NavFalling, NavJumping>()
                .WithAll<NavNeedsSurface, LocalToParent>()
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                .WithNativeDisableContainerSafetyRestriction(map)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Parent surface, ref Translation translation, in LocalToWorld localToWorld) =>
                {
                    if (
                        !surface.Value.Equals(Entity.Null) &&
                        map.TryGetValue(entity.Index, out bool needsSurface) &&
                        !map.TryAdd(entity.Index, false)
                    ) return;

                    var rayInput = new RaycastInput
                    {
                        Start = localToWorld.Position + agent.Offset,
                        End = -localToWorld.Up * settings.SurfaceRaycastDistanceMax,
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = NavUtil.ToBitMask(settings.ColliderLayer),
                            CollidesWith = NavUtil.ToBitMask(settings.SurfaceLayer),
                        }
                    };

                    if (!physicsWorld.CastRay(rayInput, out RaycastHit hit))
                    {
                        if (++agent.SurfaceRaycastCount >= settings.SurfaceRaycastMax)
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

                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                })
                .WithName("NavSurfaceTrackingJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }

        protected override void OnDestroy()
            => needsSurfaceMap.Dispose();
    }
}
