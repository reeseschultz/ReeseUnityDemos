using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Demo
{
    /// <summary>Detects the entry and exit of activators to and from the bounds of triggers.</summary>
    public class SpatialEventSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        NativeMultiHashMap<Entity, FixedString128> entityToGroupMap = default;
        NativeMultiHashMap<FixedString128, Entity> groupToActivatorMap = default;
        NativeMultiHashMap<FixedString128, Entity> groupToTriggerMap = default;

        protected override void OnCreate()
        {
            entityToGroupMap = new NativeMultiHashMap<Entity, FixedString128>(1000, Allocator.Persistent);
            groupToActivatorMap = new NativeMultiHashMap<FixedString128, Entity>(10, Allocator.Persistent);
            groupToTriggerMap = new NativeMultiHashMap<FixedString128, Entity>(10, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            entityToGroupMap.Dispose();
            groupToActivatorMap.Dispose();
            groupToTriggerMap.Dispose();
        }

        protected override void OnUpdate()
        {
            var localEntityToGroupMap = entityToGroupMap;
            var localGroupToActivatorMap = groupToActivatorMap;
            var localGroupToTriggerMap = groupToTriggerMap;

            var activatorFromEntity = GetComponentDataFromEntity<SpatialActivator>(true);
            var triggerFromEntity = GetComponentDataFromEntity<SpatialTrigger>(true);

            Entities
                .WithChangeFilter<SpatialGroupBufferElement>()
                .WithReadOnly(activatorFromEntity)
                .WithReadOnly(triggerFromEntity)
                .WithReadOnly(localEntityToGroupMap)
                .WithNativeDisableParallelForRestriction(localGroupToActivatorMap)
                .WithNativeDisableParallelForRestriction(localGroupToTriggerMap)
                .ForEach((Entity entity, in DynamicBuffer<SpatialGroupBufferElement> groupBuffer) =>
                {
                    var localGroupToActivatorMapParallel = localGroupToActivatorMap.AsParallelWriter();
                    var localGroupToTriggerMapParallel = localGroupToTriggerMap.AsParallelWriter();

                    var isActivator = activatorFromEntity.HasComponent(entity);
                    var isTrigger = triggerFromEntity.HasComponent(entity);

                    // Handle added groups:

                    var addedSet = new NativeHashSet<FixedString128>(1, Allocator.Temp);

                    for (var i = 0; i < groupBuffer.Length; ++i)
                    {
                        var currentGroup = groupBuffer[i].Value;

                        if (isActivator)
                        {
                            var activators = localGroupToActivatorMap.GetValuesForKey(currentGroup);
                            var hasActivator = false;

                            do
                            {
                                var otherEntity = activators.Current;

                                if (entity == otherEntity)
                                {
                                    hasActivator = true;
                                    break;
                                }
                            } while (activators.MoveNext());

                            if (!hasActivator) localGroupToActivatorMapParallel.Add(currentGroup, entity);
                        }

                        if (isTrigger)
                        {
                            var triggers = localGroupToTriggerMap.GetValuesForKey(currentGroup);
                            var hasTrigger = false;

                            do
                            {
                                var otherEntity = triggers.Current;

                                if (entity == otherEntity)
                                {
                                    hasTrigger = true;
                                    break;
                                }
                            } while (triggers.MoveNext());

                            if (!hasTrigger) localGroupToTriggerMapParallel.Add(currentGroup, entity);
                        }
                    }

                    // Handle removed groups:

                    var previousGroupEnumerator = localEntityToGroupMap.GetValuesForKey(entity);

                    do
                    {
                        var previousGroup = previousGroupEnumerator.Current;

                        if (addedSet.Contains(previousGroup)) continue;

                        var previousGroupInCurrentGroup = false;

                        for (var i = 0; i < groupBuffer.Length; ++i)
                        {
                            var currentGroup = groupBuffer[i].Value;

                            if (previousGroup == currentGroup)
                            {
                                previousGroupInCurrentGroup = true;
                                break;
                            }
                        }

                        if (!previousGroupInCurrentGroup)
                        {
                            if (isActivator) localGroupToActivatorMap.Remove(previousGroup, entity);
                            if (isTrigger) localGroupToTriggerMap.Remove(previousGroup, entity);
                        }
                    } while (previousGroupEnumerator.MoveNext());

                    addedSet.Dispose();
                })
                .WithName("SpatialGroupToEntityChangeJob")
                .ScheduleParallel();

            Entities
                .WithChangeFilter<SpatialGroupBufferElement>()
                .WithNativeDisableParallelForRestriction(localEntityToGroupMap)
                .ForEach((Entity entity, in DynamicBuffer<SpatialGroupBufferElement> groupBuffer) =>
                {
                    if (localEntityToGroupMap.ContainsKey(entity)) localEntityToGroupMap.Remove(entity);
                    for (var i = 0; i < groupBuffer.Length; ++i) localEntityToGroupMap.Add(entity, groupBuffer[i]);
                })
                .WithName("SpatialEntityToGroupChangeJob")
                .WithoutBurst()
                .Run();

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var eventFromEntity = GetComponentDataFromEntity<SpatialEvent>(true);

            var entriesFromEntity = GetBufferFromEntity<SpatialEntryBufferElement>();
            var exitsFromEntity = GetBufferFromEntity<SpatialExitBufferElement>();

            var changedTriggers = new NativeHashSet<Entity>(10, Allocator.TempJob);
            var parallelChangedTriggers = changedTriggers.AsParallelWriter();

            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithChangeFilter<Translation>() // If a trigger moves, we know that it could have moved into an activator's bounds.
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(eventFromEntity)
                .WithReadOnly(activatorFromEntity)
                .WithReadOnly(localGroupToActivatorMap)
                .WithNativeDisableParallelForRestriction(entriesFromEntity)
                .WithNativeDisableParallelForRestriction(exitsFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in SpatialTrigger trigger, in DynamicBuffer<SpatialGroupBufferElement> groupBuffer) =>
                {
                    parallelChangedTriggers.Add(entity);

                    if (groupBuffer.Length == 0 || !localToWorldFromEntity.HasComponent(entity)) return;

                    var triggerPosition = localToWorldFromEntity[entity].Position;

                    var triggerBounds = trigger.Bounds;
                    triggerBounds.Center += triggerPosition;

                    for (var i = 0; i < groupBuffer.Length; ++i)
                    {
                        var group = groupBuffer[i];

                        if (!localGroupToActivatorMap.ContainsKey(group)) continue;

                        var activatorEnumerator = localGroupToActivatorMap.GetValuesForKey(group);

                        do
                        {
                            var currentActivator = activatorEnumerator.Current;

                            if (
                                currentActivator == Entity.Null ||
                                !localToWorldFromEntity.HasComponent(currentActivator) ||
                                !activatorFromEntity.HasComponent(currentActivator)
                            ) continue;

                            var activatorPosition = localToWorldFromEntity[currentActivator].Position;

                            var activator = activatorFromEntity[currentActivator];

                            var activatorBounds = activator.Bounds;
                            activatorBounds.Center += activatorPosition;

                            HandleTriggerEntryAndExit(trigger, entriesFromEntity, exitsFromEntity, triggerBounds, activatorBounds, eventFromEntity, entity, currentActivator, commandBuffer, entityInQueryIndex);
                        } while (activatorEnumerator.MoveNext());
                    }
                })
                .WithName("SpatialTriggerJob")
                .ScheduleParallel();

            Entities
                .WithChangeFilter<Translation>() // If an activator moves, we know that it could have potentially moved into a trigger's bounds.
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(triggerFromEntity)
                .WithReadOnly(eventFromEntity)
                .WithReadOnly(changedTriggers)
                .WithReadOnly(localGroupToTriggerMap)
                .WithNativeDisableParallelForRestriction(entriesFromEntity)
                .WithNativeDisableParallelForRestriction(exitsFromEntity)
                .WithDisposeOnCompletion(changedTriggers)
                .ForEach((Entity entity, int entityInQueryIndex, in SpatialActivator activator, in DynamicBuffer<SpatialGroupBufferElement> groupBuffer) =>
                {
                    if (groupBuffer.Length == 0 || !localToWorldFromEntity.HasComponent(entity)) return;

                    var activatorPosition = localToWorldFromEntity[entity].Position;

                    var activatorBounds = activator.Bounds;
                    activatorBounds.Center += activatorPosition;

                    for (var i = 0; i < groupBuffer.Length; ++i)
                    {
                        var group = groupBuffer[i];

                        if (!localGroupToTriggerMap.ContainsKey(group)) continue;

                        var triggerEnumerator = localGroupToTriggerMap.GetValuesForKey(group);

                        do
                        {
                            var currentTrigger = triggerEnumerator.Current;

                            if (
                                currentTrigger == Entity.Null ||
                                changedTriggers.Contains(currentTrigger) ||
                                !localToWorldFromEntity.HasComponent(currentTrigger)
                            ) continue;

                            var trigger = triggerFromEntity[currentTrigger];
                            var triggerPosition = localToWorldFromEntity[currentTrigger].Position;

                            var triggerBounds = trigger.Bounds;
                            triggerBounds.Center += triggerPosition;

                            HandleTriggerEntryAndExit(trigger, entriesFromEntity, exitsFromEntity, triggerBounds, activatorBounds, eventFromEntity, currentTrigger, entity, commandBuffer, entityInQueryIndex);
                        } while (triggerEnumerator.MoveNext());
                    }
                })
                .WithName("SpatialActivatorJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }

        static void HandleTriggerEntryAndExit(SpatialTrigger trigger, BufferFromEntity<SpatialEntryBufferElement> entriesFromEntity, BufferFromEntity<SpatialExitBufferElement> exitsFromEntity, AABB triggerBounds, AABB activatorBounds, ComponentDataFromEntity<SpatialEvent> eventFromEntity, Entity triggerEntity, Entity activatorEntity, EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex)
        {
            if (
                !triggerBounds.Contains(activatorBounds) &&
                eventFromEntity.HasComponent(triggerEntity) &&
                eventFromEntity[triggerEntity].Activator == activatorEntity
            )
            {
                commandBuffer.RemoveComponent<SpatialEvent>(entityInQueryIndex, triggerEntity);

                if (trigger.TrackExits)
                {
                    if (!exitsFromEntity.HasComponent(triggerEntity)) commandBuffer.AddBuffer<SpatialExitBufferElement>(entityInQueryIndex, triggerEntity);

                    commandBuffer.AppendToBuffer<SpatialExitBufferElement>(entityInQueryIndex, triggerEntity, activatorEntity);
                }
            }
            else if (
                triggerBounds.Contains(activatorBounds) &&
                !eventFromEntity.HasComponent(triggerEntity)
            )
            {
                commandBuffer.AddComponent(entityInQueryIndex, triggerEntity, new SpatialEvent
                {
                    Activator = activatorEntity
                });

                if (trigger.TrackEntries)
                {
                    if (!exitsFromEntity.HasComponent(triggerEntity)) commandBuffer.AddBuffer<SpatialEntryBufferElement>(entityInQueryIndex, triggerEntity);

                    commandBuffer.AppendToBuffer<SpatialEntryBufferElement>(entityInQueryIndex, triggerEntity, activatorEntity);
                }
            }
        }
    }
}
