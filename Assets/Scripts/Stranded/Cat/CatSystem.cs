using Reese.Spatial;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo.Stranded
{
    [UpdateAfter(typeof(SpatialStartSystem)), UpdateBefore(typeof(SpatialEndSystem))]
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

            Entities
                .WithAll<SpatialTrigger, Cat>()
                .ForEach((in DynamicBuffer<SpatialOverlapBufferElement> overlaps) => // Do NOT modify the buffer, hence the in keyword.
                {
                    // There could be code here to process what currently overlaps in a given frame.
                })
                .WithoutBurst() // Can NOT use Burst when logging. Remove this line if you're not logging in the job!
                .WithName("CatOverlapJob")
                .ScheduleParallel();

            Entities
                .WithAll<SpatialTrigger, Cat>()
                .WithChangeFilter<SpatialEntryBufferElement>()
                .WithNone<Hopping>()
                .ForEach((Entity entity, in DynamicBuffer<SpatialEntryBufferElement> entryBuffer, in Translation translation) =>
                {
                    var controller = go.GetComponent<CatSoundController>();

                    for (var i = entryBuffer.Length - 1; i >= 0; --i) // Traversing from the end of the buffer for performance reasons.
                    {
                        if (controller != null) controller.Meow();

                        commandBuffer.AddComponent(entity, new Hopping
                        {
                            OriginalPosition = translation.Value,
                            Height = 10,
                            StartSeconds = elapsedSeconds,
                            Duration = 1
                        });

                        Debug.Log(entryBuffer[i].Value + " is making me purr! Purrrrrrrr!");
                    }
                })
                .WithoutBurst() // Can NOT use Burst when logging. Remove this line if you're not logging in the job!
                .WithName("CatEntryJob")
                .Run();

            Entities
                .WithAll<SpatialTrigger, Cat>()
                .WithChangeFilter<SpatialExitBufferElement>()
                .ForEach((in DynamicBuffer<SpatialExitBufferElement> exitBuffer) =>
                {
                    for (var i = exitBuffer.Length - 1; i >= 0; --i) // Traversing from the end of the buffer for performance reasons.
                    {
                        Debug.Log(exitBuffer[i].Value + " is making me meow for attention! MEEEOWWWWWWW!");
                    }
                })
                .WithoutBurst() // Can NOT use Burst when logging. Remove this line if you're not logging in the job!
                .WithName("CatExitJob")
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
