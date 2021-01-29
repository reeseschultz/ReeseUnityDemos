using Reese.Nav;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reese.Demo
{
    class NavFallSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var elapsedSeconds = (float)Time.ElapsedTime;
            var fallSecondsMax = 5;

            Entities
                .WithNone<NavProblem>()
                .WithAll<NavFalling>()
                .ForEach((Entity entity, int entityInQueryIndex, in NavAgent agent) =>
                {
                    if (elapsedSeconds - agent.FallSeconds >= fallSecondsMax)
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                        Debug.Log("Agent with entity ID = " + entity.Index + " has fallen to their death.");
                    }
                })
                .WithoutBurst()
                .WithName("NavFallJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
