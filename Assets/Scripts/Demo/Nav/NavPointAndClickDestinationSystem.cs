using Reese.Nav;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class NavPointAndClickDestinationSystem : JobComponentSystem
    {
        public static float3 Destination;

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (
                !SceneManager.GetActiveScene().name.Equals("NavPointAndClickDemo")
            ) return inputDeps;

            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var destination = Destination;

            var job = Entities
                .WithNone<NavLerping>()
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent) =>
                {
                    var offsetDestination = destination + agent.Offset;
                    if (agent.LastDestination.Equals(offsetDestination)) return;
                    agent.WorldDestination = offsetDestination;
                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                })
                .WithName("NavPointAndClickDestinationJob")
                .Schedule(inputDeps);

            job.Complete();

            return job;
        }
    }
}
