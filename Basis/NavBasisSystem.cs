using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;

namespace Reese.Nav
{
    /// <summary>Pretty much only exists to create a default basis and ensure
    /// that parent-child relationships are maintained in lieu of
    /// Unity.Physics' efforts to destroy them.</summary>
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    class NavBasisSystem : JobComponentSystem
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

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            // Below job is needed because Unity.Physics has been observed to remove
            // the Parent component, thus it can only reliably be added later at
            // runtime and not in authoring :(. Please submit an issue or PR if
            // you've a cleaner solution.
            var addParentJob = Entities
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
                .Schedule(inputDeps);

            barrier.AddJobHandleForProducer(addParentJob);

            return addParentJob;
        }
    }
}
