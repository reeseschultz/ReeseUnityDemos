using Reese.Path;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    ///<summary>This is an EXAMPLE of how to translate and rotate agents with the pathing package.</summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class PathSteeringSystem : SystemBase
    {
        public static readonly float STOPPING_DISTANCE = 1;

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("PathDemo")) Enabled = false;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var deltaSeconds = Time.DeltaTime;

            var flockingFromEntity = GetComponentDataFromEntity<PathFlocking>(true);

            Entities
                .WithNone<PathProblem, PathDestination, PathPlanning>()
                .WithReadOnly(flockingFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, ref DynamicBuffer<PathBufferElement> pathBuffer, ref PathSteering steering) =>
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

                    var heading = math.normalizesafe(pathBuffer[pathBuffer.Length - 1].Value - translation.Value);

                    if (flockingFromEntity.HasComponent(entity))
                    {
                        steering.AgentAvoidanceSteering.y = steering.SeparationSteering.y = steering.AlignmentSteering.y = steering.CohesionSteering.y = 0;

                        heading = math.normalizesafe(
                            heading +
                            steering.AgentAvoidanceSteering +
                            steering.SeparationSteering +
                            steering.AlignmentSteering +
                            steering.CohesionSteering
                        );

                        if (!steering.CollisionAvoidanceSteering.Equals(float3.zero)) heading = math.normalizesafe(heading + steering.CollisionAvoidanceSteering);
                    }

                    steering.CurrentHeading = heading;
                })
                .WithName("PathSteeringJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
