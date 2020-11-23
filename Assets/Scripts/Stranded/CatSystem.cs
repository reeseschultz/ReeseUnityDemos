using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reese.Demo
{
    class CatSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithChangeFilter<SpatialTriggerEvent>()
                .ForEach((Entity entity, int entityInQueryIndex, in Cat cat) =>
                {
                    Debug.Log("Meow.");
                })
                .WithoutBurst()
                .WithName("MeowJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
