using Reese.Nav;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reese.Demo
{
    class NavFallSystem : JobComponentSystem
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var elapsedSeconds = (float)Time.ElapsedTime;
            var fallSecondsMax = 5;
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            var job = Entities
                .ForEach((Entity entity, int entityInQueryIndex, in NavAgent agent) => {
                    if (!agent.IsFalling) return;

                    if (elapsedSeconds - agent.FallSeconds >= fallSecondsMax)
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                        Debug.Log("Agent with entity ID = " + entity.Index + " has fallen to their death.");
                    }                   
                })
                .WithoutBurst()
                .WithName("NavFallJob")
                .Schedule(inputDeps);

            barrier.AddJobHandleForProducer(job);

            return job;
        }
    }
}
