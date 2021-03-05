using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Reese.Spatial
{
    /// <summary>Detects the entry and exit of activators to and from the bounds of triggers.</summary>
    public class SpatialStartSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetOrCreateSystem<BuildPhysicsWorld>();

        protected override void OnUpdate()
        {
            Entities
                .WithAll<SpatialTrigger>()
                .WithNone<SpatialEntry>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddBuffer<SpatialEntry>(entity);
                })
                .WithName("SpatialAddEntryBufferJob")
                .WithStructuralChanges()
                .Run();

            Entities
                .WithAll<SpatialTrigger>()
                .WithNone<SpatialExit>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddBuffer<SpatialExit>(entity);
                })
                .WithName("SpatialAddExitBufferJob")
                .WithStructuralChanges()
                .Run();

            Entities
                .WithAll<SpatialTrigger>()
                .WithNone<SpatialOverlap>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddBuffer<SpatialOverlap>(entity);
                })
                .WithName("SpatialAddOverlapBufferJob")
                .WithStructuralChanges()
                .Run();

            Entities
                .WithAny<SpatialTrigger, SpatialActivator>()
                .WithNone<SpatialTag>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddBuffer<SpatialTag>(entity);
                })
                .WithName("SpatialAddTagBufferJob")
                .WithStructuralChanges()
                .Run();

            var activatorFromEntity = GetComponentDataFromEntity<SpatialActivator>(true);

            var tagsFromEntity = GetBufferFromEntity<SpatialTag>(true);

            var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithAll<SpatialTag>()
                .WithReadOnly(activatorFromEntity)
                .WithReadOnly(collisionWorld)
                .WithReadOnly(tagsFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<SpatialEntry> entries, ref DynamicBuffer<SpatialExit> exits, ref DynamicBuffer<SpatialOverlap> overlaps, in SpatialTrigger trigger, in PhysicsCollider collider, in LocalToWorld localToWorld) =>
                {
                    if (collider.Value == BlobAssetReference<Unity.Physics.Collider>.Null) return;

                    for (var i = 0; i < entries.Length; ++i)
                        if (entries[i].Value.Activator == Entity.Null)
                            entries.RemoveAt(i);

                    for (var i = 0; i < exits.Length; ++i)
                        if (exits[i].Value.Activator == Entity.Null)
                            exits.RemoveAt(i);

                    var tags = tagsFromEntity[entity];
                    var tagSet = new NativeHashSet<FixedString128>(1, Allocator.Temp);
                    for (var i = 0; i < tags.Length; ++i) tagSet.Add(tags[i]);

                    for (var i = 0; i < overlaps.Length; ++i)
                        if (
                            overlaps[i].Value.Activator == Entity.Null ||
                            !tagSet.Contains(overlaps[i].Value.Tag) // In case a tag is removed at runtime, must remove its associated overlap.
                        ) overlaps.RemoveAt(i);

                    if (tags.Length <= 0)
                    {
                        tagSet.Dispose();
                        return;
                    }

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
                        tagSet.Dispose();
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
                        var overlapEntity = overlaps[i].Value.Activator;

                        if (overlappingActivatorEntitySet.Contains(overlapEntity)) continue;

                        exits.Add(new SpatialEvent
                        {
                            Activator = overlapEntity,
                            Tag = overlaps[i].Value.Tag
                        });

                        for (var j = 0; j < overlaps.Length; ++j)
                        {
                            if (overlaps[j].Value.Activator != overlapEntity) continue;
                            overlaps.RemoveAt(j);
                            break;
                        }
                    }

                    var overlappingActivators = overlappingActivatorEntitySet.ToNativeArray(Allocator.Temp);

                    overlappingActivatorEntitySet.Dispose();

                    var overlapSet = new NativeHashSet<Entity>(1, Allocator.Temp);
                    for (var i = 0; i < overlaps.Length; ++i) overlapSet.Add(overlaps[i].Value.Activator);

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
                        FixedString128 tag = default;

                        for (var j = 0; j < overlappingTags.Length; ++j)
                        {
                            if (!tagSet.Contains(overlappingTags[j])) continue;

                            sharesTag = true;
                            tag = overlappingTags[j];
                            break;
                        }

                        if (!sharesTag) continue;

                        var evt = new SpatialEvent
                        {
                            Activator = overlappingEntity,
                            Tag = tag
                        };

                        entries.Add(evt);
                        overlaps.Add(evt);
                    }

                    overlapSet.Dispose();
                    tagSet.Dispose();
                    overlappingActivators.Dispose();
                })
                .WithName("SpatialStartJob")
                .ScheduleParallel();
        }
    }
}
