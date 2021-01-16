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

        /// <summary>True if adding an event to the trigger, false if otherwise.</summary>
        static bool HandleTriggerEntryAndExit(AABB bounds, float3 activatorPosition, ComponentDataFromEntity<SpatialEvent> eventFromEntity, Entity triggerEntity, Entity activatorEntity, EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex)
        {
            if (
                !bounds.Contains(activatorPosition) &&
                eventFromEntity.HasComponent(triggerEntity) &&
                eventFromEntity[triggerEntity].Activator == activatorEntity
            )
            {
                commandBuffer.RemoveComponent<SpatialEvent>(entityInQueryIndex, triggerEntity);
            }
            else if (
                bounds.Contains(activatorPosition) &&
                !eventFromEntity.HasComponent(triggerEntity)
            )
            {
                commandBuffer.AddComponent(entityInQueryIndex, triggerEntity, new SpatialEvent
                {
                    Activator = activatorEntity
                });

                return true;
            }

            return false;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var triggerBufferFromEntity = GetBufferFromEntity<SpatialTriggerBufferElement>(false);

            var triggersWithChangedActivatorBuffers = new NativeList<Entity>(Allocator.TempJob);

            Entities
                .WithAll<SpatialTrigger>()
                .WithChangeFilter<SpatialActivatorBufferElement>()
                .WithNativeDisableParallelForRestriction(triggerBufferFromEntity)
                .WithNativeDisableParallelForRestriction(triggersWithChangedActivatorBuffers)
                .ForEach((Entity entity, int nativeThreadIndex, in DynamicBuffer<SpatialActivatorBufferElement> activatorBuffer) =>
                {
                    for (var i = 0; i < activatorBuffer.Length; ++i)
                    {
                        var activatorEntity = activatorBuffer[i].Value;

                        if (activatorEntity == Entity.Null) continue;

                        var triggerBuffer = triggerBufferFromEntity.HasComponent(activatorEntity) ?
                            triggerBufferFromEntity[activatorEntity] :
                            commandBuffer.AddBuffer<SpatialTriggerBufferElement>(nativeThreadIndex, activatorEntity);

                        var activatorHasTrigger = false;

                        for (var j = 0; j < triggerBuffer.Length; ++j)
                        {
                            if (triggerBuffer[j] == entity)
                            {
                                activatorHasTrigger = true;
                                break;
                            }
                        }

                        if (!activatorHasTrigger) triggerBuffer.Add(entity);
                    }

                    triggersWithChangedActivatorBuffers.Add(entity);
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
                        for (var i = 0; i < triggerBuffer.Length; ++i)
                        {
                            var triggerEntity = triggerBuffer[i];

                            if (
                                triggerEntity == Entity.Null ||
                                triggersWithChangedActivatorBuffers.BinarySearch(triggerEntity) == -1
                            ) continue;

                            var activatorBuffer = activatorBufferFromEntity[triggerEntity];

                            var triggerHasActivator = false;

                            for (var j = 0; j < activatorBuffer.Length; ++j)
                            {
                                var activatorEntity = activatorBuffer[j];

                                if (entity == activatorEntity)
                                {
                                    triggerHasActivator = true;
                                    break;
                                }
                            }

                            if (!triggerHasActivator) triggerBuffer.RemoveAt(i--);
                        }
                    })
                    .WithName("RemoveTriggersFromActivatorBuffersJob")
                    .ScheduleParallel();

                Dependency.Complete();
            }

            triggersWithChangedActivatorBuffers.Dispose();

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var eventFromEntity = GetComponentDataFromEntity<SpatialEvent>(true);

            var changedTriggers = new NativeHashSet<Entity>(10, Allocator.TempJob);
            var parallelChangedTriggers = changedTriggers.AsParallelWriter();

            Entities
                .WithChangeFilter<Translation>() // If a trigger moves, we know that it could have moved into an activator's bounds.
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(eventFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in SpatialTrigger trigger, in DynamicBuffer<SpatialActivatorBufferElement> activatorBuffer) =>
                {
                    parallelChangedTriggers.Add(entity);

                    if (activatorBuffer.Length == 0 || !localToWorldFromEntity.HasComponent(entity)) return;

                    var triggerPosition = localToWorldFromEntity[entity].Position;

                    for (var i = 0; i < activatorBuffer.Length; ++i)
                    {
                        var activatorEntity = activatorBuffer[i];

                        if (activatorEntity == Entity.Null || !localToWorldFromEntity.HasComponent(activatorEntity)) continue;

                        var activatorPosition = localToWorldFromEntity[activatorEntity].Position;

                        var bounds = trigger.Bounds;
                        bounds.Center += triggerPosition;

                        if (HandleTriggerEntryAndExit(bounds, activatorPosition, eventFromEntity, entity, activatorEntity, commandBuffer, entityInQueryIndex)) return;
                    }
                })
                .WithName("SpatialTriggerJob")
                .ScheduleParallel();

            var triggerFromEntity = GetComponentDataFromEntity<SpatialTrigger>(true);

            var job = Entities
                .WithAll<SpatialActivator>()
                .WithChangeFilter<Translation>() // If an activator moves, we know that it could have potentially moved into a trigger's bounds.
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(triggerFromEntity)
                .WithReadOnly(eventFromEntity)
                .WithReadOnly(changedTriggers)
                .WithDisposeOnCompletion(changedTriggers)
                .ForEach((Entity entity, int entityInQueryIndex, in DynamicBuffer<SpatialTriggerBufferElement> triggerBuffer) =>
                {
                    if (triggerBuffer.Length == 0 || !localToWorldFromEntity.HasComponent(entity)) return;

                    var activatorPosition = localToWorldFromEntity[entity].Position;

                    for (var i = 0; i < triggerBuffer.Length; ++i)
                    {
                        var triggerEntity = triggerBuffer[i];

                        if (triggerEntity == Entity.Null || changedTriggers.Contains(triggerEntity) || !localToWorldFromEntity.HasComponent(triggerEntity)) continue;

                        var trigger = triggerFromEntity[triggerEntity];
                        var triggerPosition = localToWorldFromEntity[triggerEntity].Position;

                        var bounds = trigger.Bounds;
                        bounds.Center += triggerPosition;

                        HandleTriggerEntryAndExit(bounds, activatorPosition, eventFromEntity, triggerEntity, entity, commandBuffer, entityInQueryIndex);
                    }
                })
                .WithName("SpatialActivatorJob")
                .ScheduleParallel(Dependency);

            Dependency = JobHandle.CombineDependencies(Dependency, job);

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
