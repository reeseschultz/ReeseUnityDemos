using Reese.Nav;
using Reese.Random;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Reese.Demo
{
    class NavDestinationSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (
                !SceneManager.GetActiveScene().name.Equals("NavPerformanceDemo") &&
                !SceneManager.GetActiveScene().name.Equals("NavMovingJumpDemo")
            ) return inputDeps;

            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            return Entities
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int nativeThreadIndex, ref NavAgent agent) =>
                {
                    if (
                        agent.HasDestination ||
                        agent.Surface.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.Exists(agent.Surface)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[agent.Surface];

                    var random = randomArray[nativeThreadIndex];

                    if (jumpableSurfaces.Length == 0)
                    { // For the NavPerformanceDemo scene.
                        var bounds = renderBoundsFromEntity[agent.Surface].Value;
                        agent.WorldDestination = NavUtil.GetRandomPointInBounds(ref random, bounds, agent.Offset, 99);
                    }
                    else
                    { // For the NavMovingJumpDemo scene.
                        agent.DestinationSurface = jumpableSurfaces[random.NextInt(0, jumpableSurfaces.Length)];
                        var bounds = renderBoundsFromEntity[agent.DestinationSurface].Value;
                        agent.LocalDestination = NavUtil.GetRandomPointInBounds(ref random, bounds, agent.Offset, 0.7f); // Agents should not try to jump too close to an edge, hence the scale.
                    }

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("NavDestinationJob")
                .Schedule(inputDeps);
        }
    }
}
