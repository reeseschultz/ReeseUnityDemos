using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using Unity.Mathematics;

namespace Reese.Utility
{
    public class FixTranslationSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            // Corrects the translation of children with parents not at the origin:

            Entities
                .WithChangeFilter<PreviousParent>()
                .WithAll<FixTranslation>()
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in PreviousParent previousParent, in Parent parent) =>
                {
                    if (previousParent.Value.Equals(Entity.Null) || !localToWorldFromEntity.HasComponent(parent.Value)) return;

                    var parentTransform = localToWorldFromEntity[parent.Value];

                    if (parentTransform.Position.Equals(float3.zero))
                    {
                        commandBuffer.RemoveComponent<FixTranslation>(entityInQueryIndex, entity);
                        return;
                    }

                    translation.Value = translation.Value.MultiplyPoint3x4(math.inverse(parentTransform.Value));

                    commandBuffer.RemoveComponent<FixTranslation>(entityInQueryIndex, entity);
                })
                .WithName("FixTranslationJob")
                .ScheduleParallel();

            // Corrects transforms by re-parenting entities with missing LocalToParent components:

            Entities
                .WithNone<LocalToParent>()
                .ForEach((Entity entity, int entityInQueryIndex, in Parent parent) =>
                {
                    commandBuffer.RemoveComponent<Parent>(entityInQueryIndex, entity);

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new Parent
                    {
                        Value = parent.Value
                    });

                    commandBuffer.AddComponent<LocalToParent>(entityInQueryIndex, entity);
                })
                .WithName("ReparentJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
