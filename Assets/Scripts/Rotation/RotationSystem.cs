using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Demo
{
    class RotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var elapsedSeconds = (float)Time.ElapsedTime;

            Entities
                .ForEach((ref Rotation rotation, in Rotator rotator) =>
                    rotation.Value = math.slerp(
                        quaternion.Euler(rotator.FromRelativeAngles),
                        quaternion.Euler(rotator.ToRelativeAngles),
                        (math.sin(math.PI * rotator.Frequency * elapsedSeconds) + 1) * 0.5f
                    )
                )
                .WithName("DemoRotatorJob")
                .ScheduleParallel();
        }
    }
}
