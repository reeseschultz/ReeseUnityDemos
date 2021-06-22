using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Reese.Nav
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavMoveSystem))]
    public class NavGroundSystem : SystemBase
    {
        public bool IsDebugging = false;

        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();

        protected override void OnUpdate()
        {
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var settings = navSystem.Settings;
            var isDebugging = IsDebugging;

            Entities
               .WithNone<NavProblem>()
               .WithNone<NavPlanning, NavJumping, NavFalling>()
               .WithAll<NavWalking, LocalToParent, NavTerrainCapable>()
               .WithReadOnly(physicsWorld)
               .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref NavAgent agent, in LocalToWorld localToWorld, in Parent surface) =>
               {
                   var rayInput = new RaycastInput
                   {
                       Start = localToWorld.Position + agent.Offset,
                       End = -math.up() * settings.SurfaceRaycastDistanceMax,
                       Filter = new CollisionFilter()
                       {
                           BelongsTo = NavUtil.ToBitMask(settings.ColliderLayer),
                           CollidesWith = NavUtil.ToBitMask(settings.SurfaceLayer),
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
               .WithName("NavGroundingJob")
               .ScheduleParallel();

            buildPhysicsWorld.AddInputDependencyToComplete(Dependency);
        }
    }
}
