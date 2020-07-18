using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Reese.Nav
{
    [UpdateAfter(typeof(NavInterpolationSystem))]
    public class NavGroundedAgentSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();

        protected override void OnUpdate()
        {
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.FinalJobHandle);

            Entities
               .WithNone<NavPlanning, NavJumping, NavFalling>()
               .WithAll<NavLerping, LocalToParent, NavTerrainCapable>()
               .WithReadOnly(physicsWorld)
               .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, in NavAgent agent, in LocalToWorld localToWorld, in Parent surface) =>
               {
                   var rayInput = new RaycastInput
                   {
                       Start = localToWorld.Position + agent.Offset,
                       End = -localToWorld.Up * NavConstants.SURFACE_RAYCAST_DISTANCE_MAX,
                       Filter = new CollisionFilter()
                       {
                           BelongsTo = NavUtil.ToBitMask(NavConstants.COLLIDER_LAYER),
                           CollidesWith = NavUtil.ToBitMask(NavConstants.SURFACE_LAYER),
                       }
                   };

                   if (physicsWorld.CastRay(rayInput, out RaycastHit hit))
                   {
                       var currentPosition = translation.Value;
                       currentPosition = hit.Position + agent.Offset;

                       translation.Value = currentPosition;
                   }
               })
               .WithName("Set_grounded_height_and_rotation")
               .ScheduleParallel();
        }
    }
}