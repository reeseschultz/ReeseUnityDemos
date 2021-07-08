using Reese.Path;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    ///<summary>This is an EXAMPLE of how to interpolate agents with the pathing package.</summary>
    [UpdateAfter(typeof(PathPlanSystem))]
    public class PathLerpSystem : SystemBase
    {
        public static readonly float STOPPING_DISTANCE = 1;
        public static readonly float TRANSLATION_SPEED = 20;
        public static readonly float ROTATION_SPEED = 0.3f;

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("PathDemo")) Enabled = false;
        }

        static void Translate(float deltaSeconds, DynamicBuffer<PathBufferElement> pathBuffer, ref Translation translation)
        {
            var heading = math.normalizesafe(pathBuffer[pathBuffer.Length - 1] - translation.Value);

            translation.Value += heading * TRANSLATION_SPEED * deltaSeconds;
        }

        static void Rotate(float deltaSeconds, DynamicBuffer<PathBufferElement> pathBuffer, Translation translation, ref Rotation rotation)
        {
            var lookAt = pathBuffer[0].Value;
            lookAt.y = translation.Value.y;

            var lookRotation = quaternion.LookRotationSafe(translation.Value, math.up());

            rotation.Value = math.slerp(rotation.Value, lookRotation, deltaSeconds / ROTATION_SPEED);
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var deltaSeconds = Time.DeltaTime;

            Entities
                .WithNone<PathProblem, PathDestination, PathPlanning>()
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, ref DynamicBuffer<PathBufferElement> pathBuffer, in PathAgent agent) =>
                {
                    if (pathBuffer.Length == 0)
                    {
                        commandBuffer.RemoveComponent<PathBufferElement>(entityInQueryIndex, entity);

                        return;
                    }

                    var currentWaypoint = pathBuffer.Length - 1;

                    if (math.distance(translation.Value, pathBuffer[currentWaypoint]) < STOPPING_DISTANCE)
                    {
                        pathBuffer.RemoveAt(currentWaypoint);

                        if (pathBuffer.Length == 0) return;
                    }

                    Translate(deltaSeconds, pathBuffer, ref translation);

                    Rotate(deltaSeconds, pathBuffer, translation, ref rotation);
                })
                .WithName("PathLerpJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
