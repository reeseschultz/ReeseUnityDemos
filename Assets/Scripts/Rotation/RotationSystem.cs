using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    class RotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var elapsedSeconds = (float)Time.ElapsedTime;

            Entities
                .ForEach((ref Rotation rotation, in Rotator rotator) =>
                    rotation.Value = Quaternion.Lerp(
                         Quaternion.Euler(rotator.FromRelativeAngles),
                         Quaternion.Euler(rotator.ToRelativeAngles),
                         (Mathf.Sin(Mathf.PI * rotator.Frequency * elapsedSeconds) + 1) * 0.5f
                     )
                )
                .WithName("DemoRotatorJob")
                .ScheduleParallel();
        }
    }
}
