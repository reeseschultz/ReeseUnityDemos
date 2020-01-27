using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Demo
{
    class NavTranslatorSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var elapsedSeconds = (float)Time.ElapsedTime;
            var deltaSeconds = Time.DeltaTime;

            return Entities
                .WithAny<NavTranslator>()
                .ForEach((ref Translation translation, ref Rotation rotation) =>
                {
                    translation.Value.y = math.sin(elapsedSeconds) * 10;
                })
                .WithName("NavTranslatorJob")
                .Schedule(inputDeps);
        }
    }
}
