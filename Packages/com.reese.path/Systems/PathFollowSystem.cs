using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Path
{
    public class PathFollowSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var destinationFromEntity = GetComponentDataFromEntity<PathDestination>(true);

            Entities
                .WithAll<PathAgent>()
                .WithNone<PathProblem>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(destinationFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in PathFollow follow) =>
                {
                    if (
                        !localToWorldFromEntity.HasComponent(follow.Target) ||
                        !destinationFromEntity.HasComponent(follow.Target)
                    ) return;

                    var followerPosition = localToWorldFromEntity[entity].Position;
                    var targetPosition = localToWorldFromEntity[follow.Target].Position;
                    var distance = math.distance(followerPosition, targetPosition);

                    if (follow.MaxDistance > 0 && distance > follow.MaxDistance)
                    {
                        commandBuffer.RemoveComponent<PathFollow>(entityInQueryIndex, entity);
                        return;
                    }

                    if (distance < follow.MinDistance) return;

                    var targetDestination = destinationFromEntity[follow.Target];

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new PathDestination
                    {
                        WorldPoint = targetPosition,
                        Tolerance = targetDestination.Tolerance
                    });
                })
                .WithName("PathFollowJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
