using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Reese.Nav
{
    /// <summary>Adds NavPlanning components to NavAgents qualifying for, well,
    /// navigation planning.</summary>
    class NavQueueSystem : JobComponentSystem
    {
        /// <summary>For adding NavPlanning components.</summary>
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var jumpingFromEntity = GetComponentDataFromEntity<NavJumping>(true);

            return Entities
                .WithReadOnly(jumpingFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent) =>
                {
                    if (jumpingFromEntity.Exists(entity)) commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                    else if (!agent.HasQueuedPathPlanning && agent.HasDestination)
                    {
                        agent.HasQueuedPathPlanning = true;
                        commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                    }
                })
                .WithName("NavQueueJob")
                .Schedule(inputDeps);
        }
    }
}
