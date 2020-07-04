using Reese.Nav;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    [UpdateAfter(typeof(NavSurfaceSystem))]
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
            var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.FinalJobHandle);

            Entities
                .WithNone<NavNeedsDestination>()
                .WithNone<NavPlanning>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface, in LocalToWorld localToWorld) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.Exists(surface.Value)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var random = randomArray[nativeThreadIndex];

                    if (
                        collisionWorld.GetValidDestination(
                            localToWorld,
                            NavUtil.GetRandomPointInBounds(
                                ref random,
                                renderBoundsFromEntity[surface.Value].Value,
                                99
                            ),
                            out var validDestination))
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

    public static class CollisionWorldExtentions
    {
        public static bool GetValidDestination(this CollisionWorld collisionWorld, LocalToWorld localToWorld, float3 position, out float3 validPosition)
        {
            var rayInput = new RaycastInput()
            {
                Start = position + localToWorld.Up * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                End = position + -localToWorld.Up * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                Filter = new CollisionFilter()
                {
                    BelongsTo = NavUtil.ToBitMask(NavConstants.COLLIDER_LAYER),
                    CollidesWith = NavUtil.ToBitMask(NavConstants.SURFACE_LAYER),
                    GroupIndex = 0
                }
            };

            validPosition = float3.zero;
            if (collisionWorld.CastRay(rayInput, out var hit))
            {
                validPosition = hit.Position;
                return true;
            }
            return false;
        }
    }
}
