using Reese.Nav;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class NavFlockingDestinationSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("NavFlockingDemo")) Enabled = false;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Entities
                .WithNone<NavProblem, NavDestination>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.HasComponent(surface.Value)
                    ) return;

                    var random = randomArray[nativeThreadIndex];

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new NavDestination
                    {
                        WorldPoint = NavUtil.GetRandomPointInBounds(
                            ref random,
                            renderBoundsFromEntity[surface.Value].Value,
                            99,
                            float3.zero
                        )
                    });

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("NavFlockingDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}