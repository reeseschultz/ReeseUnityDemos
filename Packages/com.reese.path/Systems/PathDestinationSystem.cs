using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Path
{
    /// <summary>Manages destinations for agents.</summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class PathDestinationSystem : SystemBase
    {
        PathSystem pathSystem => World.GetOrCreateSystem<PathSystem>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var elapsedSeconds = (float)Time.ElapsedTime;
            var settings = pathSystem.Settings;

            Entities
                .WithNone<PathProblem>()
                .WithChangeFilter<PathDestination>()
                .ForEach((Entity entity, int entityInQueryIndex, ref PathAgent agent, in PathDestination destination) =>
                {
                    if (elapsedSeconds - agent.DestinationSeconds < settings.DestinationRateLimitSeconds)
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, destination); // So that the change filter applies next frame.
                        return;
                    }

                    agent.WorldDestination = destination.WorldPoint + agent.Offset;
                    agent.DestinationSeconds = elapsedSeconds;

                    commandBuffer.AddComponent<PathPlanning>(entityInQueryIndex, entity);
                })
                .WithName("PathDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
