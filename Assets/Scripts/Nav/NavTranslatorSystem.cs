using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Demo
{
    partial class NavTranslatorSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var elapsedSeconds = (float)Time.ElapsedTime;
            var deltaSeconds = Time.DeltaTime;

            Entities
                .WithAny<NavTranslator>()
                .ForEach((ref Translation translation, ref Rotation rotation) =>
                {
                    translation.Value.y = math.sin(elapsedSeconds) * 10;
                })
                .WithName("NavTranslatorJob")
                .ScheduleParallel();
        }
    }
}
