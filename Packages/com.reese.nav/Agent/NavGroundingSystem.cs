using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Reese.Nav
{
    [UpdateAfter(typeof(NavInterpolationSystem))]
    public class NavGroundingSystem : SystemBase
    {
        public bool IsDebugging = false;

        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();

        protected override void OnUpdate()
        {
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            bool isDebugging = IsDebugging;

            Entities
               .WithNone<NavPlanning, NavJumping, NavFalling>()
               .WithAll<NavLerping, LocalToParent, NavTerrainCapable>()
               .WithReadOnly(physicsWorld)
               .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref NavAgent agent, in LocalToWorld localToWorld, in Parent surface) =>
               {
                   var rayInput = new RaycastInput
                   {
                       Start = localToWorld.Position + agent.Offset,
                       End = -math.up() * NavConstants.SURFACE_RAYCAST_DISTANCE_MAX,
                       Filter = new CollisionFilter()
                       {
                           BelongsTo = NavUtil.ToBitMask(NavConstants.COLLIDER_LAYER),
                           CollidesWith = NavUtil.ToBitMask(NavConstants.SURFACE_LAYER),
                       }
                   };

                   if (physicsWorld.CastRay(rayInput, out RaycastHit hit))
                   {
                       if (isDebugging)
                       {
                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + hit.SurfaceNormal * 15, UnityEngine.Color.green);

                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + localToWorld.Up * 7, UnityEngine.Color.cyan);
                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + localToWorld.Right * 7, UnityEngine.Color.cyan);
                           UnityEngine.Debug.DrawLine(hit.Position, hit.Position + localToWorld.Forward * 7, UnityEngine.Color.cyan);
                       }

                       agent.SurfacePointNormal = hit.SurfaceNormal;

                       var currentPosition = translation.Value;
                       currentPosition.y = hit.Position.y + agent.Offset.y;
                       translation.Value = currentPosition;
                   }
               })
               .WithName("GroundingJob")
               .ScheduleParallel();
        }
    }
}
