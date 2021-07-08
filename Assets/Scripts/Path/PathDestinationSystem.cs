using Reese.Nav;
using Reese.Path;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class PathDestinationSystem : SystemBase
    {
        AABB surfaceAabb = default;

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("PathDemo"))
            {
                Enabled = false;
                return;
            }

            var surfaceBounds = GameObject.Find("Surface").GetComponent<Renderer>().bounds;

            surfaceAabb = new AABB
            {
                Center = surfaceBounds.center,
                Extents = surfaceBounds.extents
            };
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            var surfaceAABB = surfaceAabb;

            Entities
                .WithAll<PathAgent>()
                .WithNone<PathProblem, PathDestination, PathBufferElement>()
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex) =>
                {
                    var random = randomArray[nativeThreadIndex];

                    var pos = NavUtil.GetRandomPointInBounds(
                        ref random,
                        surfaceAABB,
                        1,
                        float3.zero
                    );

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new PathDestination
                    {
                        WorldPoint = pos
                    });

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("PathDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
