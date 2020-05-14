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
                .WithNone<NavDestination>()
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
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new NavDestination{
                            Value = NavUtil.GetRandomPointInBounds(
                                ref random,
                                renderBoundsFromEntity[surface.Value].Value,
                                agent.Offset,
                                99
                            )
                        });
                    }
                    else
                    { // For the NavMovingJumpDemo scene.
                        var destinationSurface = jumpableSurfaces[random.NextInt(0, jumpableSurfaces.Length)];

                        commandBuffer.AddComponent(entityInQueryIndex, entity, new NavDestination{
                            Value = NavUtil.GetRandomPointInBounds(
                                ref random,
                                renderBoundsFromEntity[destinationSurface].Value,
                                agent.Offset,
                                0.7f
                            )
                        });
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
