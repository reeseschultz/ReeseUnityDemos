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
    class NavMovingJumpDestinationSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("NavMovingJumpDemo"))
                Enabled = false;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Entities
                .WithNone<NavProblem, NavDestination>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.HasComponent(surface.Value)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var random = randomArray[nativeThreadIndex];

                    var destinationSurface = jumpableSurfaces[random.NextInt(0, jumpableSurfaces.Length)];

                    var localPoint = NavUtil.GetRandomPointInBounds(
                        ref random,
                        renderBoundsFromEntity[destinationSurface].Value,
                        3,
                        float3.zero
                    );

                    var worldPoint = NavUtil.MultiplyPoint3x4(
                        localToWorldFromEntity[destinationSurface.Value].Value,
                        localPoint
                    );

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new NavDestination
                    {
                        WorldPoint = worldPoint
                    });

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("NavMovingJumpDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
