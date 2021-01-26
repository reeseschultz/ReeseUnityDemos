using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Reese.Demo
{
    /// <summary>Detects the entry and exit of activators to and from the bounds of triggers.</summary>
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class SpatialEventSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetOrCreateSystem<BuildPhysicsWorld>();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var activatorFromEntity = GetComponentDataFromEntity<SpatialActivator>(true);

            var entriesFromEntity = GetBufferFromEntity<SpatialEntryBufferElement>();
            var exitsFromEntity = GetBufferFromEntity<SpatialExitBufferElement>();

            var groupBufferFromEntity = GetBufferFromEntity<SpatialGroupBufferElement>(true);

            var previousEntryBufferFromEntity = GetBufferFromEntity<SpatialPreviousEntryBufferElement>();

            var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithAll<SpatialGroupBufferElement>()
                .WithReadOnly(activatorFromEntity)
                .WithReadOnly(collisionWorld)
                .WithReadOnly(groupBufferFromEntity)
                .WithNativeDisableParallelForRestriction(entriesFromEntity)
                .WithNativeDisableParallelForRestriction(exitsFromEntity)
                .WithNativeDisableParallelForRestriction(previousEntryBufferFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in SpatialTrigger trigger, in PhysicsCollider collider, in LocalToWorld localToWorld) =>
                {
                    var groupBuffer = groupBufferFromEntity[entity];

                    if (groupBuffer.Length <= 0) return;

                    var overlaps = new NativeList<int>(Allocator.Temp);

                    var aabb = collider.Value.Value.CalculateAabb();
                    aabb.Min += localToWorld.Position;
                    aabb.Max += localToWorld.Position;

                    if (!collisionWorld.OverlapAabb(
                        new OverlapAabbInput
                        {
                            Aabb = aabb,
                            Filter = trigger.Filter
                        },
                        ref overlaps
                    ))
                    {
                        overlaps.Dispose();
                        return;
                    }

                    var overlappingActivatorEntitySet = new NativeHashSet<Entity>(1, Allocator.Temp);

                    for (var i = 0; i < overlaps.Length; ++i)
                    {
                        var overlappingEntity = collisionWorld.Bodies[overlaps[i]].Entity;

                        if (!activatorFromEntity.HasComponent(overlappingEntity)) continue;

                        overlappingActivatorEntitySet.Add(overlappingEntity);
                    }

                    overlaps.Dispose();

                    DynamicBuffer<SpatialPreviousEntryBufferElement> previousEntryBuffer = default;
                    if (!previousEntryBufferFromEntity.HasComponent(entity)) previousEntryBuffer = commandBuffer.AddBuffer<SpatialPreviousEntryBufferElement>(entityInQueryIndex, entity);
                    else previousEntryBuffer = previousEntryBufferFromEntity[entity];

                    for (var i = 0; i < previousEntryBuffer.Length; ++i)
                    {
                        var previousEntryEntity = previousEntryBuffer[i].Value;

                        if (overlappingActivatorEntitySet.Contains(previousEntryEntity)) continue;

                        if (!exitsFromEntity.HasComponent(entity)) commandBuffer.AddBuffer<SpatialExitBufferElement>(entityInQueryIndex, entity);
                        commandBuffer.AppendToBuffer<SpatialExitBufferElement>(entityInQueryIndex, entity, previousEntryEntity);

                        for (var j = 0; j < previousEntryBuffer.Length; ++j)
                        {
                            if (previousEntryBuffer[j].Value != previousEntryEntity) continue;

                            previousEntryBuffer.RemoveAt(j);
                            break;
                        }
                    }

                    var overlappingActivators = overlappingActivatorEntitySet.ToNativeArray(Allocator.Temp);

                    overlappingActivatorEntitySet.Dispose();

                    var groupSet = new NativeHashSet<FixedString128>(1, Allocator.Temp);
                    for (var i = 0; i < groupBuffer.Length; ++i) groupSet.Add(groupBuffer[i]);

                    var previousEntrySet = new NativeHashSet<Entity>(1, Allocator.Temp);
                    for (var i = 0; i < previousEntryBuffer.Length; ++i) previousEntrySet.Add(previousEntryBuffer[i]);

                    for (var i = 0; i < overlappingActivators.Length; ++i)
                    {
                        var overlappingEntity = overlappingActivators[i];

                        if (
                            overlappingEntity == Entity.Null ||
                            !activatorFromEntity.HasComponent(overlappingEntity) ||
                            !groupBufferFromEntity.HasComponent(overlappingEntity) ||
                            previousEntrySet.Contains(overlappingEntity)
                        ) continue;

                        var overlappingGroups = groupBufferFromEntity[overlappingEntity];

                        var sharesGroup = false;

                        for (var j = 0; j < overlappingGroups.Length; ++j)
                        {
                            if (!groupSet.Contains(overlappingGroups[j])) continue;

                            sharesGroup = true;
                            break;
                        }

                        if (!sharesGroup) continue;

                        if (!entriesFromEntity.HasComponent(entity)) commandBuffer.AddBuffer<SpatialEntryBufferElement>(entityInQueryIndex, entity);
                        commandBuffer.AppendToBuffer<SpatialEntryBufferElement>(entityInQueryIndex, entity, overlappingEntity);

                        previousEntryBuffer.Add(overlappingEntity);
                    }

                    previousEntrySet.Dispose();
                    groupSet.Dispose();
                    overlappingActivators.Dispose();
                })
                .WithName("SpatialEventJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
