using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Reese.Demo
{
    class SpatialTriggerSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            var spatialTriggerEventFromEntity = GetComponentDataFromEntity<SpatialTriggerEvent>(true);

            Entities
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(spatialTriggerEventFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in SpatialTrigger trigger, in DynamicBuffer<SpatialTriggerActivatorBufferElement> activatorBuffer) =>
                {
                    if (activatorBuffer.Length == 0 || !localToWorldFromEntity.HasComponent(entity)) return;

                    var triggerPosition = localToWorldFromEntity[entity].Position;

                    for (var i = 0; i < activatorBuffer.Length; ++i)
                    {
                        var activatorEntity = activatorBuffer[i];

                        if (activatorEntity == Entity.Null || !localToWorldFromEntity.HasComponent(activatorEntity)) continue;

                        var activatorPosition = localToWorldFromEntity[activatorEntity].Position;

                        var bounds = trigger.Bounds;
                        bounds.Center += triggerPosition;

                        if (
                            !bounds.Contains(activatorPosition) &&
                            spatialTriggerEventFromEntity.HasComponent(entity) &&
                            spatialTriggerEventFromEntity[entity].Activator == activatorEntity
                        )
                        {
                            commandBuffer.RemoveComponent<SpatialTriggerEvent>(entityInQueryIndex, entity);
                        }
                        else if (
                            bounds.Contains(activatorPosition) &&
                            !spatialTriggerEventFromEntity.HasComponent(entity)
                        )
                        {
                            commandBuffer.AddComponent(entityInQueryIndex, entity, new SpatialTriggerEvent
                            {
                                Activator = activatorEntity
                            });

                            return;
                        }
                    }
                })
                .WithName("SpatialTriggerJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
