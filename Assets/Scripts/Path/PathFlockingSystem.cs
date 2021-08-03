using Reese.Demo;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Path
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PathQuadrantSystem))]
    public class PathFlockingSystem : SystemBase
    {
        public bool IsDebugging = false;

        PathFlockingSettingsSystem flockingSettingsSystem => World.GetOrCreateSystem<PathFlockingSettingsSystem>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var flockingSettings = flockingSettingsSystem.FlockingSettings;
            var quadrantHashMap = PathQuadrantSystem.QuadrantHashMap;
            var isDebugging = IsDebugging;

            Entities
                .WithNone<PathProblem>()
                .WithReadOnly(quadrantHashMap)
                .ForEach((Entity entity, ref PathSteering steering, in PathFlocking flocking, in PathAgent agent, in LocalToWorld localToWorld) =>
                    {
                        var entityHashMapKey = PathQuadrantSystem.HashPosition(localToWorld.Position, flockingSettings);

                        var separationNeighbors = 0;
                        var alignmentNeighbors = 0;
                        var cohesionNeighbors = 0;

                        var cohesionPos = float3.zero;
                        var alignmentVec = float3.zero;
                        var separationVec = float3.zero;

                        var closestQuadrantData = new QuadrantData();

                        PathQuadrantSystem.SearchQuadrantNeighbors(
                            in quadrantHashMap, entityHashMapKey, entity, flocking, localToWorld.Position, flockingSettings,
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

                        var nearestAgentCollisionDistanceFromRadius = closestDistance - flocking.AgentAversionDistance;
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
                .WithName("PathFlockingJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            Dependency.Complete();
        }
    }
}
