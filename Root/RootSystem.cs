using Unity.Entities;
using Unity.Jobs;

namespace Reese.Utility
{
    public class RootSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<Prefab>()
                .WithChangeFilter<LinkedEntityGroup>()
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<LinkedEntityGroup> linkedEntities) =>
                {
                    for (var i = 0; i < linkedEntities.Length; ++i)
                    {
                        commandBuffer.AddComponent(nativeThreadIndex, linkedEntities[i].Value, new Root
                        {
                            Value = entity
                        });
                    }
                })
                .WithName("RootJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
