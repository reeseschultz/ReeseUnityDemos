using Reese.Nav.Quadrant;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
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
        public bool IsDebugging = false;

        protected override void OnUpdate()
        {
            var flockingSettings = navSystem.FlockingSettings;
            var quadrantHashMap = NavQuadrantSystem.QuadrantHashMap;
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var isDebugging = IsDebugging;

            Entities
                .WithAll<NavFlocking, NavWalking, LocalToParent>()
                .WithNone<NavProblem, NavFalling, NavJumping>()
                .WithReadOnly(quadrantHashMap)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach(
                    (Entity entity, ref NavSteering steering, in NavAgent agent, in Translation translation) =>
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

                        var closestQuadrantData = new QuadrantData();

                        NavQuadrantSystem.SearchQuadrantNeighbors(
                            in quadrantHashMap, entityHashMapKey, entity, agent, entityLocalToWorld.Position, flockingSettings,
                            ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos,
                            ref alignmentVec, ref separationVec, ref closestQuadrantData);

                        var closestDistance = Vector3.Distance(entityLocalToWorld.Position, closestQuadrantData.LocalToWorld.Position);

                        closestDistance = math.sqrt(closestDistance);

                        // Steering average:
                        if (separationNeighbors > 0)
                        {
                            separationVec /= separationNeighbors;

                            // Experimental, but this appears to limit clumping of close agents:
                            if (Vector3.SqrMagnitude(separationVec) > 0.5f) separationVec += steering.CurrentHeading;
                        }
                        else separationVec = steering.CurrentHeading;

                        if (alignmentNeighbors > 0) alignmentVec /= alignmentNeighbors;
                        else alignmentVec = steering.CurrentHeading;

                        if (cohesionNeighbors > 0) cohesionPos /= cohesionNeighbors;
                        else cohesionPos = entityLocalToWorld.Position;

                        // Collision implies obstacle avoidance for other agents:
                        var nearestAgentCollisionDistanceFromRadius = closestDistance - agent.AgentAversionDistance;

                        var nearestAgentDirection = math.normalizesafe(entityLocalToWorld.Position - closestQuadrantData.LocalToWorld.Position);

                        var agentAvoidanceSteering = flockingSettings.AgentCollisionAvoidanceStrength * nearestAgentDirection;

                        agentAvoidanceSteering =
                            (nearestAgentCollisionDistanceFromRadius < 0 && !agentAvoidanceSteering.Equals(float3.zero))
                                ? agentAvoidanceSteering
                                : steering.CurrentHeading;

                        if (nearestAgentCollisionDistanceFromRadius < 0 && isDebugging) Debug.DrawRay(entityLocalToWorld.Position, -nearestAgentDirection * 2f, Color.blue);

                        // Normalizing:
                        var alignmentSteering = math.normalizesafe(alignmentVec) * flockingSettings.AlignmentWeight;
                        var cohesionSteering = math.normalizesafe(cohesionPos - entityLocalToWorld.Position) * flockingSettings.CohesionWeight;
                        var separationSteering = math.normalizesafe(separationVec) * flockingSettings.SeparationWeight;

                        steering.AgentAvoidanceSteering = agentAvoidanceSteering;
                        steering.CohesionSteering = cohesionSteering;
                        steering.SeparationSteering = separationSteering;
                        steering.AlignmentSteering = alignmentSteering;
                    }
                )
                .WithName("NavFlockingJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}