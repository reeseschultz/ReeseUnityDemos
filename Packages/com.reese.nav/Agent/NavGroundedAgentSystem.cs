using Reese.Demo;
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

            var isDebugging = DebugMode.IsDebugging;
            var drawUnitVectors = DebugMode.DrawUnitVectors;

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
                       var currentForward = math.forward(rotation.Value);
                       rotation.Value = quaternion.LookRotationSafe(currentForward, hit.SurfaceNormal);

                       if (isDebugging && drawUnitVectors)
                       {
                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + hit.SurfaceNormal * 15, UnityEngine.Color.green);

                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + localToWorld.Right * 5, UnityEngine.Color.cyan);

                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + math.forward(rotation.Value) * 15, UnityEngine.Color.white);
                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + currentForward * 7, UnityEngine.Color.blue);
                       }

                       var currentPosition = translation.Value;
                       currentPosition.y = hit.Position.y + agent.Offset.y;
                       translation.Value = currentPosition;
                   }
               })
               .WithName("Set_grounded_height_and_rotation")
               .ScheduleParallel();
        }
    }
}