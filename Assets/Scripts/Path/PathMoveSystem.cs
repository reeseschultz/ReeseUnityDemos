using Reese.Path;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PathSteeringSystem))]
    public class PathMoveSystem : SystemBase
    {
        public static readonly float TRANSLATION_SPEED = 20;
        public static readonly float ROTATION_SPEED = 0.3f;

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("PathDemo")) Enabled = false;
        }
        static void Translate(float deltaSeconds, PathSteering steering, ref Translation translation)
            => translation.Value += steering.CurrentHeading * TRANSLATION_SPEED * deltaSeconds;

        static void Rotate(float deltaSeconds, PathSteering steering, Translation translation, ref Rotation rotation)
        {
            var lookAt = steering.CurrentHeading;
            lookAt.y = translation.Value.y;

            var lookRotation = quaternion.LookRotationSafe(lookAt - translation.Value, math.up());

            rotation.Value = math.slerp(rotation.Value, lookRotation, deltaSeconds / ROTATION_SPEED);
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var deltaSeconds = Time.DeltaTime;

            Entities
                .WithNone<PathProblem, PathDestination, PathPlanning>()
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, in PathSteering steering) =>
                {
                    Translate(deltaSeconds, steering, ref translation);
                    Rotate(deltaSeconds, steering, translation, ref rotation);
                })
                .WithName("PathMoveJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
