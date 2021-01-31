using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Spatial
{
    /// <summary>Detects the entry and exit of activators to and from the bounds of triggers.</summary>
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class SpatialStartSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetOrCreateSystem<BuildPhysicsWorld>();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithNone<SpatialEntryBufferElement>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    commandBuffer.AddBuffer<SpatialEntryBufferElement>(entityInQueryIndex, entity);
                })
                .WithName("SpatialAddEntryBufferJob")
                .ScheduleParallel();

            Entities
                .WithNone<SpatialExitBufferElement>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    commandBuffer.AddBuffer<SpatialExitBufferElement>(entityInQueryIndex, entity);
                })
                .WithName("SpatialAddExitBufferJob")
                .ScheduleParallel();

            Entities
                .WithNone<SpatialOverlapBufferElement>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    commandBuffer.AddBuffer<SpatialOverlapBufferElement>(entityInQueryIndex, entity);
                })
                .WithName("SpatialAddOverlapBufferJob")
                .ScheduleParallel();

            Entities
                .WithNone<SpatialTagBufferElement>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    commandBuffer.AddBuffer<SpatialTagBufferElement>(entityInQueryIndex, entity);
                })
                .WithName("SpatialAddTagBufferJob")
                .ScheduleParallel();

            var activatorFromEntity = GetComponentDataFromEntity<SpatialActivator>(true);

            var tagsFromEntity = GetBufferFromEntity<SpatialTagBufferElement>(true);

            var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithAll<SpatialTagBufferElement>()
                .WithReadOnly(activatorFromEntity)
                .WithReadOnly(collisionWorld)
                .WithReadOnly(tagsFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<SpatialEntryBufferElement> entries, ref DynamicBuffer<SpatialExitBufferElement> exits, ref DynamicBuffer<SpatialOverlapBufferElement> overlaps, in SpatialTrigger trigger, in PhysicsCollider collider, in LocalToWorld localToWorld) =>
                {
                    var tags = tagsFromEntity[entity];

                    if (tags.Length <= 0) return;

                    var possibleOverlaps = new NativeList<int>(Allocator.Temp);

                    var aabb = collider.Value.Value.CalculateAabb();
                    aabb.Min += localToWorld.Position;
                    aabb.Max += localToWorld.Position;

                    if (!collisionWorld.OverlapAabb(
                        new OverlapAabbInput
                        {
                            Aabb = aabb,
                            Filter = trigger.Filter
                        },
                        ref possibleOverlaps
                    ))
                    {
                        possibleOverlaps.Dispose();
                        return;
                    }

                    var overlappingActivatorEntitySet = new NativeHashSet<Entity>(1, Allocator.Temp);

                    for (var i = 0; i < possibleOverlaps.Length; ++i)
                    {
                        var overlappingEntity = collisionWorld.Bodies[possibleOverlaps[i]].Entity;

                        if (!activatorFromEntity.HasComponent(overlappingEntity)) continue;

                        overlappingActivatorEntitySet.Add(overlappingEntity);
                    }

                    possibleOverlaps.Dispose();

                    for (var i = 0; i < overlaps.Length; ++i)
                    {
                        var overlapEntity = overlaps[i].Value;

                        if (overlappingActivatorEntitySet.Contains(overlapEntity)) continue;

                        exits.Add(overlapEntity);

                        for (var j = 0; j < overlaps.Length; ++j)
                        {
                            if (overlaps[j].Value != overlapEntity) continue;
                            overlaps.RemoveAt(j);
                            break;
                        }
                    }

                    var overlappingActivators = overlappingActivatorEntitySet.ToNativeArray(Allocator.Temp);

                    overlappingActivatorEntitySet.Dispose();

                    var tagSet = new NativeHashSet<FixedString128>(1, Allocator.Temp);
                    for (var i = 0; i < tags.Length; ++i) tagSet.Add(tags[i]);

                    var overlapSet = new NativeHashSet<Entity>(1, Allocator.Temp);
                    for (var i = 0; i < overlaps.Length; ++i) overlapSet.Add(overlaps[i]);

                    for (var i = 0; i < overlappingActivators.Length; ++i)
                    {
                        var overlappingEntity = overlappingActivators[i];

                        if (
                            overlappingEntity == Entity.Null ||
                            overlappingEntity == entity || // Cannot self-activate!
                            !activatorFromEntity.HasComponent(overlappingEntity) ||
                            !tagsFromEntity.HasComponent(overlappingEntity) ||
                            overlapSet.Contains(overlappingEntity)
                        ) continue;

                        var overlappingTags = tagsFromEntity[overlappingEntity];

                        var sharesTag = false;

                        for (var j = 0; j < overlappingTags.Length; ++j)
                        {
                            if (!tagSet.Contains(overlappingTags[j])) continue;

                            sharesTag = true;
                            break;
                        }

                        if (!sharesTag) continue;

                        commandBuffer.AppendToBuffer<SpatialEntryBufferElement>(entityInQueryIndex, entity, overlappingEntity);

                        entries.Add(overlappingEntity);
                        overlaps.Add(overlappingEntity);
                    }

                    overlapSet.Dispose();
                    tagSet.Dispose();
                    overlappingActivators.Dispose();
                })
                .WithName("SpatialStartJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
