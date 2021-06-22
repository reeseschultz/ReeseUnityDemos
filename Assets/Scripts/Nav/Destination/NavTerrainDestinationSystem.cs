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
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    public class NavTerrainDestinationSystem : SystemBase
    {
        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
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
            var settings = navSystem.Settings;
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Entities
                .WithNone<NavProblem, NavDestination, NavPlanning>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface, in LocalToWorld localToWorld) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.HasComponent(surface.Value)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var random = randomArray[nativeThreadIndex];
                    var aabb = renderBoundsFromEntity[surface.Value].Value;

                    if (
                        physicsWorld.GetPointOnSurfaceLayer(
                            localToWorld,
                            NavUtil.GetRandomPointInBounds(
                                ref random,
                                aabb,
                                99,
                                aabb.Center
                            ),
                            out var validDestination,
                            settings.ObstacleRaycastDistanceMax,
                            settings.ColliderLayer,
                            settings.SurfaceLayer
                        )
                    )
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new NavDestination
                        {
                            WorldPoint = validDestination
                        });
                    }

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("NavTerrainDestinationJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
            buildPhysicsWorld.AddInputDependencyToComplete(Dependency);
        }
    }
}
