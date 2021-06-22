using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using static Reese.Nav.NavSystem;

namespace Reese.Nav.Quadrant
{
    public struct QuadrantData
    {
        public LocalToWorld LocalToWorld;
        public Entity Entity;
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavDestinationSystem))]
    public class NavQuadrantSystem : SystemBase
    {
        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
        public static NativeMultiHashMap<int, QuadrantData> QuadrantHashMap;

        protected override void OnCreate()
            => QuadrantHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);

        protected override void OnDestroy()
            => QuadrantHashMap.Dispose();

        protected override void OnUpdate()
        {
            QuadrantHashMap.Clear();

            var navFlockingSettings = navSystem.FlockingSettings;

            EntityQuery entityQuery = GetEntityQuery(typeof(NavAgent));

            var entityCount = entityQuery.CalculateEntityCount();
            if (entityCount > QuadrantHashMap.Capacity) QuadrantHashMap.Capacity = entityCount;

            var parallelHashMap = QuadrantHashMap.AsParallelWriter();

            var hashPositionJobHandle = Entities
                .WithAll<NavAgent>()
                .ForEach(
                    (Entity entity, in LocalToWorld localToWorld) =>
                    {
                        var hashKey = HashkeyFromPosition(localToWorld.Position, navFlockingSettings);
                        // Debug.Log($"Position {localToWorld.Position} HashKey: {hashKey}");

                        parallelHashMap.Add(
                            hashKey,
                            new QuadrantData
                            {
                                LocalToWorld = localToWorld
                            }
                        );
                    }
                )
                .WithName("NavHashPositionJob")
                .ScheduleParallel(Dependency);

            hashPositionJobHandle.Complete();
        }

        public static int HashkeyFromPosition(float3 position, NavFlockingSettings flockingSettings)
            => (int)(math.floor(position.x / flockingSettings.QuadrantCellSize) + flockingSettings.QuadrantZMultiplier * math.floor(position.z / flockingSettings.QuadrantCellSize));

        static void SearchQuadrantNeighbor(in NativeMultiHashMap<int, QuadrantData> quadrantHashMap, in int key,
            in Entity entity, in NavAgent agent, in float3 pos, ref int separationNeighbors, ref int alignmentNeighbors,
            ref int cohesionNeighbors, ref float3 cohesionPos, ref float3 alignmentVec, ref float3 separationVec,
            ref QuadrantData closestQuadrantData)
        {
            if (!quadrantHashMap.TryGetFirstValue(key, out var quadrantData, out var iterator)) return; // Nothing in quadrant.

            closestQuadrantData = quadrantData;

            var closestDistance = Vector3.Distance(pos, quadrantData.LocalToWorld.Position);

            do // Loop through entities in quadrant.
            {
                if (entity == quadrantData.Entity || quadrantData.LocalToWorld.Position.Equals(pos)) continue;

                var distance = Vector3.Distance(pos, quadrantData.LocalToWorld.Position);
                var nearest = distance < closestDistance;

                closestDistance = math.select(closestDistance, distance, nearest);

                if (nearest) closestQuadrantData = quadrantData;

                if (distance < agent.SeparationPerceptionRadius)
                {
                    separationNeighbors++;
                    separationVec += (pos - quadrantData.LocalToWorld.Position) / distance;
                }

                if (distance < agent.AlignmentPerceptionRadius)
                {
                    alignmentNeighbors++;
                    alignmentVec += quadrantData.LocalToWorld.Up;
                }

                if (distance < agent.CohesionPerceptionRadius)
                {
                    cohesionNeighbors++;
                    cohesionPos += quadrantData.LocalToWorld.Position;
                }
            } while (quadrantHashMap.TryGetNextValue(out quadrantData, ref iterator));
        }

        public static void SearchQuadrantNeighbors(in NativeMultiHashMap<int, QuadrantData> quadrantHashMap,
            in int key, in Entity currentEntity, in NavAgent agent, in float3 pos, in NavFlockingSettings flockingSettings, ref int separationNeighbors,
            ref int alignmentNeighbors, ref int cohesionNeighbors, ref float3 cohesionPos, ref float3 alignmentVec,
            ref float3 separationVec, ref QuadrantData closestQuadrantData)
        {
            SearchQuadrantNeighbor(quadrantHashMap, key, currentEntity, agent, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key + 1, currentEntity, agent, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key - 1, currentEntity, agent, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key + flockingSettings.QuadrantZMultiplier, currentEntity, agent, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key - flockingSettings.QuadrantZMultiplier, currentEntity, agent, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
        }
    }
}