﻿using Unity.Collections;
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

        /// <summary>True if adding an event to the trigger, false if otherwise.</summary>
        static bool HandleTriggerEntryAndExit(SpatialTrigger trigger, BufferFromEntity<SpatialEntryBufferElement> entriesFromEntity, BufferFromEntity<SpatialExitBufferElement> exitsFromEntity, AABB triggerBounds, AABB activatorBounds, ComponentDataFromEntity<SpatialEvent> eventFromEntity, Entity triggerEntity, Entity activatorEntity, EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex)
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

                return true;
            }

            return false;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var triggerBufferFromEntity = GetBufferFromEntity<SpatialTriggerBufferElement>();

            var triggersWithChangedActivatorBuffers = new NativeList<Entity>(Allocator.TempJob);

            Entities
                .WithAll<SpatialTrigger>()
                .WithChangeFilter<SpatialActivatorBufferElement>()
                .WithNativeDisableParallelForRestriction(triggerBufferFromEntity)
                .WithNativeDisableParallelForRestriction(triggersWithChangedActivatorBuffers)
                .ForEach((Entity entity, int nativeThreadIndex, in DynamicBuffer<SpatialActivatorBufferElement> activatorBuffer) =>
                {
                    var triggerList = new NativeList<Entity>(Allocator.Temp);

                    for (var i = 0; i < activatorBuffer.Length; ++i)
                    {
                        var activatorEntity = activatorBuffer[i].Value;

                        if (activatorEntity == Entity.Null) continue;

                        var triggerBuffer = triggerBufferFromEntity.HasComponent(activatorEntity) ?
                            triggerBufferFromEntity[activatorEntity] :
                            commandBuffer.AddBuffer<SpatialTriggerBufferElement>(nativeThreadIndex, activatorEntity);

                        triggerList.Clear();
                        triggerList.AddRange(triggerBuffer.AsNativeArray().Reinterpret<Entity>());

                        if (triggerList.BinarySearch(entity) == -1) triggerBuffer.Add(entity);
                    }

                    triggersWithChangedActivatorBuffers.Add(entity);

                    triggerList.Dispose();
                })
                .WithName("AddTriggersToActivatorBuffersJob")
                .ScheduleParallel();

            Dependency.Complete();

            if (triggersWithChangedActivatorBuffers.Length > 0)
            {
                var activatorBufferFromEntity = GetBufferFromEntity<SpatialActivatorBufferElement>(true);

                Entities
                    .WithAll<SpatialActivator>()
                    .WithReadOnly(activatorBufferFromEntity)
                    .WithNativeDisableParallelForRestriction(triggersWithChangedActivatorBuffers)
                    .ForEach((Entity entity, in DynamicBuffer<SpatialTriggerBufferElement> triggerBuffer) =>
                    {
                        var activatorList = new NativeList<Entity>(Allocator.Temp);

                        for (var i = triggerBuffer.Length - 1; i >= 0; --i)
                        {
                            var triggerEntity = triggerBuffer[i];

                            if (
                                triggerEntity == Entity.Null ||
                                triggersWithChangedActivatorBuffers.BinarySearch(triggerEntity) == -1
                            ) continue;

                            var activatorBuffer = activatorBufferFromEntity[triggerEntity];

                            activatorList.Clear();
                            activatorList.AddRange(activatorBuffer.AsNativeArray().Reinterpret<Entity>());

                            if (activatorList.BinarySearch(entity) == -1) triggerBuffer.RemoveAt(i);
                        }

                        activatorList.Dispose();
                    })
                    .WithName("RemoveTriggersFromActivatorBuffersJob")
                    .ScheduleParallel();

                Dependency.Complete();
            }

            triggersWithChangedActivatorBuffers.Dispose();

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var eventFromEntity = GetComponentDataFromEntity<SpatialEvent>(true);
            var activatorFromEntity = GetComponentDataFromEntity<SpatialActivator>(true);

            var entriesFromEntity = GetBufferFromEntity<SpatialEntryBufferElement>();
            var exitsFromEntity = GetBufferFromEntity<SpatialExitBufferElement>();

            var changedTriggers = new NativeHashSet<Entity>(10, Allocator.TempJob);
            var parallelChangedTriggers = changedTriggers.AsParallelWriter();

            Entities
                .WithChangeFilter<Translation>() // If a trigger moves, we know that it could have moved into an activator's bounds.
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(eventFromEntity)
                .WithReadOnly(activatorFromEntity)
                .WithNativeDisableParallelForRestriction(entriesFromEntity)
                .WithNativeDisableParallelForRestriction(exitsFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in SpatialTrigger trigger, in DynamicBuffer<SpatialActivatorBufferElement> activatorBuffer) =>
                {
                    parallelChangedTriggers.Add(entity);

                    if (activatorBuffer.Length == 0 || !localToWorldFromEntity.HasComponent(entity)) return;

                    var triggerPosition = localToWorldFromEntity[entity].Position;

                    var triggerBounds = trigger.Bounds;
                    triggerBounds.Center += triggerPosition;

                    for (var i = 0; i < activatorBuffer.Length; ++i)
                    {
                        var activatorEntity = activatorBuffer[i];

                        if (
                            activatorEntity == Entity.Null ||
                            !localToWorldFromEntity.HasComponent(activatorEntity) ||
                            !activatorFromEntity.HasComponent(activatorEntity)
                        ) continue;

                        var activatorPosition = localToWorldFromEntity[activatorEntity].Position;

                        var activator = activatorFromEntity[activatorEntity];

                        var activatorBounds = activator.Bounds;
                        activatorBounds.Center += activatorPosition;

                        if (HandleTriggerEntryAndExit(trigger, entriesFromEntity, exitsFromEntity, triggerBounds, activatorBounds, eventFromEntity, entity, activatorEntity, commandBuffer, entityInQueryIndex)) return;
                    }
                })
                .WithName("SpatialTriggerJob")
                .ScheduleParallel();

            var triggerFromEntity = GetComponentDataFromEntity<SpatialTrigger>(true);

            var job = Entities
                .WithChangeFilter<Translation>() // If an activator moves, we know that it could have potentially moved into a trigger's bounds.
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(triggerFromEntity)
                .WithReadOnly(eventFromEntity)
                .WithReadOnly(changedTriggers)
                .WithNativeDisableParallelForRestriction(entriesFromEntity)
                .WithNativeDisableParallelForRestriction(exitsFromEntity)
                .WithDisposeOnCompletion(changedTriggers)
                .ForEach((Entity entity, int entityInQueryIndex, in SpatialActivator activator, in DynamicBuffer<SpatialTriggerBufferElement> triggerBuffer) =>
                {
                    if (triggerBuffer.Length == 0 || !localToWorldFromEntity.HasComponent(entity)) return;

                    var activatorPosition = localToWorldFromEntity[entity].Position;

                    var activatorBounds = activator.Bounds;
                    activatorBounds.Center += activatorPosition;

                    for (var i = 0; i < triggerBuffer.Length; ++i)
                    {
                        var triggerEntity = triggerBuffer[i];

                        if (
                            triggerEntity == Entity.Null ||
                            changedTriggers.Contains(triggerEntity) ||
                            !localToWorldFromEntity.HasComponent(triggerEntity)
                        ) continue;

                        var trigger = triggerFromEntity[triggerEntity];
                        var triggerPosition = localToWorldFromEntity[triggerEntity].Position;

                        var triggerBounds = trigger.Bounds;
                        triggerBounds.Center += triggerPosition;

                        HandleTriggerEntryAndExit(trigger, entriesFromEntity, exitsFromEntity, triggerBounds, activatorBounds, eventFromEntity, triggerEntity, entity, commandBuffer, entityInQueryIndex);
                    }
                })
                .WithName("SpatialActivatorJob")
                .ScheduleParallel(Dependency);

            Dependency = JobHandle.CombineDependencies(Dependency, job);

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
