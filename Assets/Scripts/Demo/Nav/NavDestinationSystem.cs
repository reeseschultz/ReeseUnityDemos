using Reese.Nav;
using Reese.Random;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class NavDestinationSystem : JobComponentSystem
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (
                !SceneManager.GetActiveScene().name.Equals("NavPerformanceDemo") &&
                !SceneManager.GetActiveScene().name.Equals("NavMovingJumpDemo")
            ) return inputDeps;

            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            var job = Entities
                .WithNone<NavLerping>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.Exists(surface.Value)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var random = randomArray[nativeThreadIndex];

                    if (jumpableSurfaces.Length == 0)
                    { // For the NavPerformanceDemo scene.
                        var bounds = renderBoundsFromEntity[surface.Value].Value;
                        agent.WorldDestination = NavUtil.GetRandomPointInBounds(ref random, bounds, agent.Offset, 99);
                    }
                    else
                    { // For the NavMovingJumpDemo scene.
                        agent.DestinationSurface = jumpableSurfaces[random.NextInt(0, jumpableSurfaces.Length)];
                        var bounds = renderBoundsFromEntity[agent.DestinationSurface].Value;
                        agent.LocalDestination = NavUtil.GetRandomPointInBounds(ref random, bounds, agent.Offset, 0.7f); // Agents should not try to jump too close to an edge, hence the scale.
                    }

                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("NavDestinationJob")
                .Schedule(inputDeps);

            barrier.AddJobHandleForProducer(job);

            return job;
        }
    }
}
