using Reese.Nav;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class NavTerrainDestinationSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("NavTerrainDemo"))
                Enabled = false;
        }

        protected override void OnUpdate()
        {
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithNone<NavHasProblem, NavNeedsDestination, NavPlanning>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface, in LocalToWorld localToWorld) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.HasComponent(surface.Value)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var random = randomArray[nativeThreadIndex];

                    if (
                        physicsWorld.GetPointOnSurfaceLayer(
                            localToWorld,
                            NavUtil.GetRandomPointInBounds(
                                ref random,
                                renderBoundsFromEntity[surface.Value].Value,
                                99
                            ),
                            out var validDestination
                        )
                    )
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new NavNeedsDestination
                        {
                            Destination = validDestination
                        });
                    }

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("NavTerrainDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }   
}
