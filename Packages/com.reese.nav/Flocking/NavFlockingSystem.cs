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
            var isDebugging = IsDebugging;

            Entities
                .WithAll<NavFlocking, NavWalking, LocalToParent>()
                .WithNone<NavProblem, NavFalling, NavJumping>()
                .WithReadOnly(quadrantHashMap)
                .ForEach((Entity entity, ref NavSteering steering, in NavAgent agent, in Translation translation, in LocalToWorld localToWorld) =>
                    {
                        var entityHashMapKey = NavQuadrantSystem.HashPosition(localToWorld.Position, flockingSettings);

                        var separationNeighbors = 0;
                        var alignmentNeighbors = 0;
                        var cohesionNeighbors = 0;

                        var cohesionPos = float3.zero;
                        var alignmentVec = float3.zero;
                        var separationVec = float3.zero;

                        var closestQuadrantData = new QuadrantData();

                        NavQuadrantSystem.SearchQuadrantNeighbors(
                            in quadrantHashMap, entityHashMapKey, entity, agent, localToWorld.Position, flockingSettings,
                            ref separationNeighbors, ref alignmentNeighbors, ref cohesionNeighbors, ref cohesionPos,
                            ref alignmentVec, ref separationVec, ref closestQuadrantData);

                        var closestDistance = math.distancesq(localToWorld.Position, closestQuadrantData.LocalToWorld.Position);

                        if (separationNeighbors > 0)
                        {
                            separationVec /= separationNeighbors;

                            if (math.lengthsq(separationVec) > 0.5f) separationVec += steering.CurrentHeading; // Limit clumping of close agents.
                        }
                        else separationVec = steering.CurrentHeading;

                        if (alignmentNeighbors > 0) alignmentVec /= alignmentNeighbors;
                        else alignmentVec = steering.CurrentHeading;

                        if (cohesionNeighbors > 0) cohesionPos /= cohesionNeighbors;
                        else cohesionPos = localToWorld.Position;

                        var nearestAgentCollisionDistanceFromRadius = closestDistance - agent.AgentAversionDistance;
                        var nearestAgentDirection = math.normalizesafe(localToWorld.Position - closestQuadrantData.LocalToWorld.Position);
                        var agentAvoidanceSteering = flockingSettings.AgentCollisionAvoidanceStrength * nearestAgentDirection;

                        agentAvoidanceSteering =
                            (nearestAgentCollisionDistanceFromRadius < 0 && !agentAvoidanceSteering.Equals(float3.zero))
                                ? agentAvoidanceSteering
                                : steering.CurrentHeading;

                        if (nearestAgentCollisionDistanceFromRadius < 0 && isDebugging) Debug.DrawRay(localToWorld.Position, -nearestAgentDirection * 2f, Color.blue);

                        var alignmentSteering = math.normalizesafe(alignmentVec) * flockingSettings.AlignmentWeight;
                        var cohesionSteering = math.normalizesafe(cohesionPos - localToWorld.Position) * flockingSettings.CohesionWeight;
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
