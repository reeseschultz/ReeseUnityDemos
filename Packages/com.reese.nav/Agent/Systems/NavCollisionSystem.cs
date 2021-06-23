using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Reese.Nav
{
    /// <summary>
    /// Projects raycasts in front of any entity which is flocking and walking, and adds a new steering force
    /// whenever those raycasts hit an obstacle. It is supposed to 'gently' steer away from obstacles, it does not guarantee anti-clipping.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavFlockingSystem))]
    public class NavCollisionSystem : SystemBase
    {
        public bool IsDebugging = false;

        const float NUM_RAYS = 17; // For casting 17 rays in a (flockingSettings.CollisionCastingAngle * 2) deg angle along an entity's forward direction.

        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();

        protected override void OnUpdate()
        {
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var settings = navSystem.Settings;
            var flockingSettings = navSystem.FlockingSettings;
            var isDebugging = IsDebugging;

            Entities
                .WithNone<NavProblem>()
                .WithNone<NavPlanning, NavJumping, NavFalling>()
                .WithAll<NavObstacleSteering>()
                .WithAll<NavWalking, NavFlocking, LocalToParent>()
                .WithReadOnly(physicsWorld)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref NavAgent agent, ref NavSteering steering, in LocalToWorld localToWorld, in Rotation rotation, in Parent surface) =>
                {
                    var averageHitDirection = float3.zero;
                    var clostestHitDistance = agent.ObstacleAversionDistance;
                    var hits = 0;

                    for (int i = 0; i < NUM_RAYS; ++i)
                    {
                        var rayModifier = Quaternion.AngleAxis(i / (NUM_RAYS - 1) * flockingSettings.CollisionCastingAngle * 2 - flockingSettings.CollisionCastingAngle, localToWorld.Up);
                        var rayDirection = rotation.Value * rayModifier * Vector3.forward;

                        if (isDebugging) Debug.DrawRay(localToWorld.Position, rayDirection * agent.ObstacleAversionDistance, Color.cyan);

                        var ray = new RaycastInput
                        {
                            Start = localToWorld.Position,
                            End = localToWorld.Position + (float3)rayDirection * agent.ObstacleAversionDistance,
                            Filter = new CollisionFilter()
                            {
                                BelongsTo = NavUtil.ToBitMask(settings.ColliderLayer),
                                CollidesWith = NavUtil.ToBitMask(settings.ObstacleLayer),
                            }
                        };

                        if (physicsWorld.CastRay(ray, out RaycastHit hit))
                        {
                            var distance = Vector3.Distance(localToWorld.Position, hit.Position);

                            clostestHitDistance = math.select(clostestHitDistance, distance, distance < clostestHitDistance);
                            averageHitDirection += localToWorld.Position - hit.Position;

                            hits++;

                            if (isDebugging) Debug.DrawLine(localToWorld.Position, hit.Position, Color.red);
                        }
                    }

                    averageHitDirection /= hits;

                    var scalar = 1 - (clostestHitDistance / agent.ObstacleAversionDistance);

                    // Steer away from the average hit direction
                    // We also scale our steering vector by the closest hit distance. Otherwise, the entity just bounces off walls and spins out of control.
                    steering.CollisionAvoidanceSteering = flockingSettings.ObstacleCollisionAvoidanceStrength * math.normalizesafe(averageHitDirection) * scalar;
                })
                .WithName("NavClipAvoidanceJob")
                .ScheduleParallel();

            buildPhysicsWorld.AddInputDependencyToComplete(Dependency);
        }
    }
}