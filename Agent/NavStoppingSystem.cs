using Unity.Entities;
using Unity.Jobs;

namespace Reese.Nav
{
    public class NavStoppingSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var lerpingFromEntity = GetComponentDataFromEntity<NavLerping>(true);
            var destinationFromEntity = GetComponentDataFromEntity<NavNeedsDestination>(true);
            var planningFromEntity = GetComponentDataFromEntity<NavPlanning>(true);

            var job = Entities
                .WithNone<NavFalling, NavJumping>()
                .WithReadOnly(lerpingFromEntity)
                .WithReadOnly(destinationFromEntity)
                .WithReadOnly(planningFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in NavStop stop) =>
                {
                    commandBuffer.RemoveComponent<NavStop>(entityInQueryIndex, entity);

                    if (lerpingFromEntity.HasComponent(entity)) commandBuffer.RemoveComponent<NavLerping>(entityInQueryIndex, entity);

                    if (destinationFromEntity.HasComponent(entity)) commandBuffer.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);

                    if (planningFromEntity.HasComponent(entity)) commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
                })
                .WithName("NavStoppingJob")
                .ScheduleParallel(Dependency);

            job.Complete();
        }
    }
}
