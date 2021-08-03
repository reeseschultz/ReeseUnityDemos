using Reese.Path;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Reese.Demo.PathFlockingSettingsSystem;

namespace Reese.Demo
{
    public struct QuadrantData
    {
        public LocalToWorld LocalToWorld;
        public Entity Entity;
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(Path.PathDestinationSystem))]
    public class PathQuadrantSystem : SystemBase
    {
        public static NativeMultiHashMap<int, QuadrantData> QuadrantHashMap;

        PathFlockingSettingsSystem flockingSettingsSystem => World.GetOrCreateSystem<PathFlockingSettingsSystem>();

        protected override void OnCreate()
            => QuadrantHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);

        protected override void OnDestroy()
            => QuadrantHashMap.Dispose();

        protected override void OnUpdate()
        {
            QuadrantHashMap.Clear();

            var flockingSettings = flockingSettingsSystem.FlockingSettings;
            var entityCount = GetEntityQuery(typeof(PathAgent)).CalculateEntityCount();

            if (entityCount > QuadrantHashMap.Capacity) QuadrantHashMap.Capacity = entityCount;

            var parallelHashMap = QuadrantHashMap.AsParallelWriter();

            Entities
                .WithAll<PathAgent>()
                .ForEach((Entity entity, in LocalToWorld localToWorld) =>
                    {
                        parallelHashMap.Add(
                            HashPosition(localToWorld.Position, flockingSettings),
                            new QuadrantData
                            {
                                LocalToWorld = localToWorld
                            }
                        );
                    }
                )
                .WithName("PathHashPositionJob")
                .ScheduleParallel();
        }

        public static int HashPosition(float3 position, PathFlockingSettings flockingSettings)
            => (int)(math.floor(position.x / flockingSettings.QuadrantCellSize) + flockingSettings.QuadrantZMultiplier * math.floor(position.z / flockingSettings.QuadrantCellSize));

        static void SearchQuadrantNeighbor(in NativeMultiHashMap<int, QuadrantData> quadrantHashMap, in int key,
            in Entity entity, in PathFlocking flocking, in float3 pos, ref int separationNeighbors, ref int alignmentNeighbors,
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

                if (distance < flocking.SeparationPerceptionRadius)
                {
                    separationNeighbors++;
                    separationVec += (pos - quadrantData.LocalToWorld.Position) / distance;
                }

                if (distance < flocking.AlignmentPerceptionRadius)
                {
                    alignmentNeighbors++;
                    alignmentVec += quadrantData.LocalToWorld.Up;
                }

                if (distance < flocking.CohesionPerceptionRadius)
                {
                    cohesionNeighbors++;
                    cohesionPos += quadrantData.LocalToWorld.Position;
                }
            } while (quadrantHashMap.TryGetNextValue(out quadrantData, ref iterator));
        }

        public static void SearchQuadrantNeighbors(in NativeMultiHashMap<int, QuadrantData> quadrantHashMap,
            in int key, in Entity currentEntity, in PathFlocking flocking, in float3 pos, in PathFlockingSettings flockingSettings, ref int separationNeighbors,
            ref int alignmentNeighbors, ref int cohesionNeighbors, ref float3 cohesionPos, ref float3 alignmentVec,
            ref float3 separationVec, ref QuadrantData closestQuadrantData)
        {
            SearchQuadrantNeighbor(quadrantHashMap, key, currentEntity, flocking, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key + 1, currentEntity, flocking, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key - 1, currentEntity, flocking, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key + flockingSettings.QuadrantZMultiplier, currentEntity, flocking, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
            SearchQuadrantNeighbor(quadrantHashMap, key - flockingSettings.QuadrantZMultiplier, currentEntity, flocking, pos, ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos, ref alignmentVec, ref separationVec, ref closestQuadrantData);
        }
    }
}
