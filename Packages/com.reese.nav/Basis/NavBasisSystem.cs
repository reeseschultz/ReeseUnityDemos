using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Physics.Systems;

namespace Reese.Nav
{
    /// <summary>Pretty much only exists to create a default basis and ensure
    /// that parent-child relationships are maintained in lieu of
    /// Unity.Physics' efforts to destroy them.</summary>
    [UpdateAfter(typeof(TransformSystemGroup))]
    public class NavBasisSystem : SystemBase
    {
        /// <summary>The default basis that all other bases and basis-lacking
        /// surfaces are parented to.</summary>
        public Entity DefaultBasis { get; private set; }

        /// <summary>For ensuring parent-child relationships.</summary>
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            DefaultBasis = World.EntityManager.CreateEntity();
            World.EntityManager.AddComponent(DefaultBasis, typeof(NavBasis));
            World.EntityManager.AddComponent(DefaultBasis, typeof(LocalToWorld));
            World.EntityManager.AddComponent(DefaultBasis, typeof(Translation));
            World.EntityManager.AddComponent(DefaultBasis, typeof(Rotation));
            // entityManager.SetName(defaultBasis, "DefaultBasis"); // Used to make builds fail. Not sure if it still does.
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            // Below job is needed because Unity.Physics removes the Parent
            // component for dynamic bodies.
            Entities
                .WithNone<Parent>()
                .ForEach((Entity entity, int entityInQueryIndex, in NavBasis basis) =>
                {
                    if (basis.ParentBasis.Equals(Entity.Null)) return;

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new Parent
                    {
                        Value = basis.ParentBasis
                    });

                    commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(LocalToParent));
                })
                .WithoutBurst()
                .WithName("NavAddParentToBasisJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
