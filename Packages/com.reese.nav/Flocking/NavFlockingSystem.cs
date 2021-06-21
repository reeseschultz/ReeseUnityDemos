using Reese.Nav.Quadrant;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

namespace Reese.Nav
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavQuadrantSystem))]
    public class NavFlockingSystem : SystemBase
    {
        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var flockingSettings = navSystem.FlockingSettings;
            var navSettings = navSystem.Settings;
            var quadrantHashMap = NavQuadrantSystem.QuadrantHashMap;
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var navFollowFromEntity = GetComponentDataFromEntity<NavFollow>(true);

            Entities
                .WithAll<NavFlocking, NavWalking, LocalToParent>()
                .WithNone<NavProblem, NavFalling, NavJumping>()
                .WithReadOnly(quadrantHashMap)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach(
                    (Entity entity, ref NavSteering navSteering, in NavAgent agent, in Translation translation) =>
                    {
                        if (!localToWorldFromEntity.HasComponent(entity)) return;

                        var entityLocalToWorld = localToWorldFromEntity[entity];
                        var entityHashMapKey = NavQuadrantSystem.HashkeyFromPosition(entityLocalToWorld.Position, flockingSettings);

                        var separationNeighbors = 0;
                        var alignmentNeighbors = 0;
                        var cohesionNeighbors = 0;

                        var cohesionPos = float3.zero;
                        var alignmentVec = float3.zero;
                        var separationVec = float3.zero;
                        var avoidanceVec = float3.zero;

                        QuadrantData closestQuadrantData = new QuadrantData();

                        NavQuadrantSystem.SearchQuadrantNeighbors(
                            in quadrantHashMap, entityHashMapKey, entity, agent, entityLocalToWorld.Position, flockingSettings,
                            ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos,
                            ref alignmentVec, ref separationVec, ref closestQuadrantData);

                        var closestDistance = Vector3.Distance(entityLocalToWorld.Position,
                            closestQuadrantData.LocalToWorld.Position);
                        closestDistance = math.sqrt(closestDistance);

                        // Steering average
                        if (separationNeighbors > 0)
                        {
                            separationVec /= separationNeighbors;
                            //
                            // if (Vector3.SqrMagnitude(separationVec) > 0.5f) // So they will not clump together
                            //     separationVec += navSteering.CurrentHeading;
                        }
                        else separationVec = navSteering.CurrentHeading;

                        if (alignmentNeighbors > 0) alignmentVec /= alignmentNeighbors;
                        else alignmentVec = navSteering.CurrentHeading;

                        if (cohesionNeighbors > 0) cohesionPos /= cohesionNeighbors;
                        else cohesionPos = entityLocalToWorld.Position;

                        // Collision = obstacle avoidance for other agents
                        var nearestCollisionDistanceFromRadius =
                            closestDistance - agent.ObstacleAversionDistance;
                        
                        var collisionSteering = flockingSettings.CollisionAvoidanceStrength *
                                                math.normalizesafe(entityLocalToWorld.Position -
                                                                   closestQuadrantData.LocalToWorld.Position);

                        var collisionAvoidanceSteering =
                            (nearestCollisionDistanceFromRadius < 0 && !collisionSteering.Equals(float3.zero))
                                ? collisionSteering
                                : navSteering.CurrentHeading;
                        
                        // Debug.Log($"Closest Distance: {closestDistance} Nearest Collision Dist From Radius: {nearestCollisionDistanceFromRadius} Closest Position: {closestQuadrantData.LocalToWorld.Position}");

                        if (nearestCollisionDistanceFromRadius < 0)
                        {
                             // Debug.DrawLine(entityLocalToWorld.Position, collisionSteering, UnityEngine.Color.red);
                             Debug.DrawLine(entityLocalToWorld.Position, closestQuadrantData.LocalToWorld.Position,
                                 UnityEngine.Color.blue); 
                        }
                        

                        // var pursuitSteering = translation.Value;
                        //
                        // if (navFollowFromEntity.HasComponent(entity))
                        // {
                        //     var entityNavFollow = navFollowFromEntity[entity];
                        //     entityNavFollow.Target
                        // }
                        
                        // Normalizing
                        var alignmentSteering = math.normalizesafe(alignmentVec) * flockingSettings.AlignmentWeight;
                        var cohesionSteering  = math.normalizesafe(cohesionPos - entityLocalToWorld.Position) *
                                                        flockingSettings.CohesionWeight;
                        var separationSteering= math.normalizesafe(separationVec) * flockingSettings.SeparationWeight;

                        
                        navSteering.CollisionAvoidanceSteering = collisionAvoidanceSteering;
                        navSteering.CohesionSteering           = cohesionSteering;
                        navSteering.SeparationSteering         = separationSteering;
                        navSteering.AlignmentSteering          = alignmentSteering;
                    }
                )
                .WithName("NavFlockingJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}