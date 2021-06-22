using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Nav
{
    public class NavFollowSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var needsDestinationFromEntity = GetComponentDataFromEntity<NavDestination>(true);

            Entities
                .WithAll<NavAgent, LocalToWorld>()
                .WithNone<NavProblem, NavFalling, NavJumping>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(needsDestinationFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, in NavFollow follow) =>
                {
                    if (
                        !localToWorldFromEntity.HasComponent(follow.Target) ||
                        !needsDestinationFromEntity.HasComponent(follow.Target)
                    ) return;

                    var followerPosition = localToWorldFromEntity[entity].Position;
                    var targetPosition = localToWorldFromEntity[follow.Target].Position;
                    var distance = math.distance(followerPosition, targetPosition);

                    if (follow.MaxDistance > 0 && distance > follow.MaxDistance)
                    {
                        commandBuffer.RemoveComponent<NavFollow>(entityInQueryIndex, entity);
                        return;
                    }

                    if (distance < follow.MinDistance) return;

                    var targetDestination = needsDestinationFromEntity[follow.Target];

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new NavDestination
                    {
                        WorldPoint = targetPosition,
                        Tolerance = targetDestination.Tolerance,
                        CustomLerp = targetDestination.CustomLerp
                    });
                })
                .WithName("NavFollowJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
