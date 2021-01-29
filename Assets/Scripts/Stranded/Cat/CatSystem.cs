using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo.Stranded
{
    [UpdateAfter(typeof(SpatialEventSystem))]
    class CatSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        GameObject go = default;

        protected override void OnUpdate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded")) return;

            if (go == null)
            {
                go = GameObject.Find("Cat");
                if (go == null) return;
            }

            var commandBuffer = barrier.CreateCommandBuffer();

            var elapsedSeconds = (float)Time.ElapsedTime;

            Entities // Example handling of the spatial entry buffer.
                .WithAll<Cat>()
                .WithChangeFilter<SpatialEntryBufferElement>()
                .WithNone<Hopping>()
                .ForEach((Entity entity, ref DynamicBuffer<SpatialEntryBufferElement> entryBuffer, in Translation translation) =>
                {
                    var controller = go.GetComponent<CatSoundController>();

                    if (controller == null) return;

                    for (var i = entryBuffer.Length - 1; i >= 0; --i) // Traversing from end of buffer so removal is straightforward and performant.
                    {
                        controller.Meow();

                        commandBuffer.AddComponent(entity, new Hopping
                        {
                            OriginalPosition = translation.Value,
                            Height = 10,
                            StartSeconds = elapsedSeconds,
                            Duration = 1
                        });

                        Debug.Log(entryBuffer[0].Value + " has entered the cat's trigger bounds.");

                        entryBuffer.RemoveAt(i); // If you don't remove exits, they'll pile up in the buffer and eventually consume lots of heap memory.
                    }
                })
                .WithoutBurst()
                .WithName("CatMeowJob")
                .Run();

            Entities // Example handling of the spatial exit buffer.
                .WithAll<Cat>()
                .WithChangeFilter<SpatialExitBufferElement>()
                .ForEach((Entity entity, ref DynamicBuffer<SpatialExitBufferElement> exitBuffer) =>
                {
                    for (var i = exitBuffer.Length - 1; i >= 0; --i) // Traversing from end of buffer so removal is straightforward and performant.
                    {
                        Debug.Log(exitBuffer[i].Value + " has exited the cat's trigger bounds.");

                        // Potentially handle different kinds of exit events here.

                        exitBuffer.RemoveAt(i); // If you don't remove exits, they'll pile up in the buffer and eventually consume lots of heap memory.
                    }
                })
                .WithoutBurst() // Not using Burst since there's logging in the job.
                .WithName("ExitJob")
                .ScheduleParallel();

            var parallelCommandBuffer = commandBuffer.AsParallelWriter();

            Entities
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in Hopping hopping) =>
                {
                    var position = translation.Value;

                    position.y = hopping.Height * math.sin(
                        math.PI * ((elapsedSeconds - hopping.StartSeconds) / hopping.Duration)
                    );

                    translation.Value = position;

                    if (elapsedSeconds - hopping.StartSeconds < hopping.Duration) return;

                    translation.Value = hopping.OriginalPosition;
                    parallelCommandBuffer.RemoveComponent<Hopping>(entityInQueryIndex, entity);
                })
                .WithName("CatHopJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
