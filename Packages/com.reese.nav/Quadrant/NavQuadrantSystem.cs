using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
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
        public static NativeMultiHashMap<int, QuadrantData> QuadrantHashMap;

        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();

        protected override void OnCreate()
            => QuadrantHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);

        protected override void OnDestroy()
            => QuadrantHashMap.Dispose();

        protected override void OnUpdate()
        {
            QuadrantHashMap.Clear();

            var navFlockingSettings = navSystem.FlockingSettings;
            var entityCount = GetEntityQuery(typeof(NavAgent)).CalculateEntityCount();

            if (entityCount > QuadrantHashMap.Capacity) QuadrantHashMap.Capacity = entityCount;

            var parallelHashMap = QuadrantHashMap.AsParallelWriter();

            Entities
                .WithAll<NavAgent>()
                .ForEach((Entity entity, in LocalToWorld localToWorld) =>
                    {
                        parallelHashMap.Add(
                            HashPosition(localToWorld.Position, navFlockingSettings),
                            new QuadrantData
                            {
                                LocalToWorld = localToWorld
                            }
                        );
                    }
                )
                .WithName("NavHashPositionJob")
                .ScheduleParallel();
        }

        public static int HashPosition(float3 position, NavFlockingSettings flockingSettings)
            => (int)(math.floor(position.x / flockingSettings.QuadrantCellSize) + flockingSettings.QuadrantZMultiplier * math.floor(position.z / flockingSettings.QuadrantCellSize));

        static void SearchQuadrantNeighbor(in NativeMultiHashMap<int, QuadrantData> quadrantHashMap, in int key,
            in Entity entity, in NavAgent agent, in float3 pos, ref int separationNeighbors, ref int alignmentNeighbors,
            ref int cohesionNeighbors, ref float3 cohesionPos, ref float3 alignmentVec, ref float3 separationVec,
            ref QuadrantData closestQuadrantData)
        {
            if (!quadrantHashMap.TryGetFirstValue(key, out var quadrantData, out var iterator)) return;

            closestQuadrantData = quadrantData;

            var closestDistance = math.distance(pos, quadrantData.LocalToWorld.Position);

            do
            {
                if (entity == quadrantData.Entity || quadrantData.LocalToWorld.Position.Equals(pos)) continue;

                var distance = math.distance(pos, quadrantData.LocalToWorld.Position);
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