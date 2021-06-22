using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;

namespace Reese.Nav
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavMoveSystem))]
    public class NavStopSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var walkingFromEntity = GetComponentDataFromEntity<NavWalking>(true);
            var destinationFromEntity = GetComponentDataFromEntity<NavDestination>(true);
            var planningFromEntity = GetComponentDataFromEntity<NavPlanning>(true);

            Entities
                .WithNone<NavFalling, NavJumping>()
                .WithReadOnly(walkingFromEntity)
                .WithReadOnly(destinationFromEntity)
                .WithReadOnly(planningFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<NavPathBufferElement> pathBuffer, in NavStop stop) =>
                {
                    commandBuffer.RemoveComponent<NavStop>(entityInQueryIndex, entity);

                    if (walkingFromEntity.HasComponent(entity)) commandBuffer.RemoveComponent<NavWalking>(entityInQueryIndex, entity);
                    if (destinationFromEntity.HasComponent(entity)) commandBuffer.RemoveComponent<NavDestination>(entityInQueryIndex, entity);
                    if (planningFromEntity.HasComponent(entity)) commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);

                    pathBuffer.Clear();
                })
                .WithName("NavStopJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
