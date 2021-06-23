using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using UnityEngine;
using static Reese.Nav.NavSystem;

namespace Reese.Nav
{
    /// <summary>Calculates the current heading via the
    /// buffers attached to a given NavAgent. Said buffers are determined by
    /// the NavPlanSystem, although *this* system is responsible for clearing
    /// the jump buffer. Other steering behaviors from the flocking system are also applied to those buffers.
    /// Jumping and falling are accomplished with artificial gravity and projectile motion math.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    [UpdateAfter(typeof(NavCollisionSystem))]
    public class NavSteeringSystem : SystemBase
    {
        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        static void HandleCompletePath(ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity, Entity entity,
            Rotation rotation, ref NavAgent agent, ref DynamicBuffer<NavPathBufferElement> pathBuffer, Parent surface,
            Translation translation, PhysicsWorld physicsWorld, float elapsedSeconds,
            EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex, NavSettings settings)
        {
            var rayInput = new RaycastInput
            {
                Start = localToWorldFromEntity[entity].Position + agent.Offset,
                End = math.forward(rotation.Value) * settings.ObstacleRaycastDistanceMax,
                Filter = new CollisionFilter
                {
                    BelongsTo = NavUtil.ToBitMask(settings.ColliderLayer),
                    CollidesWith = NavUtil.ToBitMask(settings.ObstacleLayer)
                }
            };

            if (
                !surface.Value.Equals(agent.DestinationSurface) &&
                !NavUtil.ApproxEquals(translation.Value, agent.LocalDestination, settings.StoppingDistance) &&
                !physicsWorld.CastRay(rayInput, out RaycastHit hit)
            )
            {
                agent.JumpSeconds = elapsedSeconds;

                commandBuffer.RemoveComponent<NavWalking>(entityInQueryIndex, entity);
                commandBuffer.RemoveComponent<NavSteering>(entityInQueryIndex, entity);
                commandBuffer.AddComponent<NavJumping>(entityInQueryIndex, entity);
                commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);

                return;
            }

            commandBuffer.RemoveComponent<NavWalking>(entityInQueryIndex, entity);
            commandBuffer.RemoveComponent<NavSteering>(entityInQueryIndex, entity);
            commandBuffer.RemoveComponent<NavDestination>(entityInQueryIndex, entity);
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var elapsedSeconds = (float) Time.ElapsedTime;
            var deltaSeconds = Time.DeltaTime;
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var settings = navSystem.Settings;
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>();
            
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var fallingFromEntity = GetComponentDataFromEntity<NavFalling>(true);
            var jumpingFromEntity = GetComponentDataFromEntity<NavJumping>(true);
            var flockingFromEntity = GetComponentDataFromEntity<NavFlocking>(true);
            
            Entities
                .WithNone<NavProblem, NavPlanning>()
                .WithAll<NavWalking, LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .WithReadOnly(jumpingFromEntity)
                .WithReadOnly(fallingFromEntity)
                .WithReadOnly(flockingFromEntity)
                .WithNativeDisableParallelForRestriction(pathBufferFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Translation translation, ref NavSteering navSteering,
                    ref Rotation rotation, in Parent surface) =>
                {
                    if (!pathBufferFromEntity.HasComponent(entity) ||
                        agent.DestinationSurface.Equals(Entity.Null)) return;

                    var pathBuffer = pathBufferFromEntity[entity];

                    if (pathBuffer.Length == 0)
                    {
                        HandleCompletePath(localToWorldFromEntity, entity, rotation, ref agent, ref pathBuffer, surface,
                            translation, physicsWorld, elapsedSeconds, commandBuffer, entityInQueryIndex, settings);
                        return;
                    }

                    var pathBufferIndex = pathBuffer.Length - 1;

                    if (NavUtil.ApproxEquals(translation.Value, pathBuffer[pathBufferIndex].Value,
                        settings.StoppingDistance)) pathBuffer.RemoveAt(pathBufferIndex);

                    if (pathBuffer.Length == 0) return;

                    pathBufferIndex = pathBuffer.Length - 1;

                    var heading = math.normalizesafe(pathBuffer[pathBufferIndex].Value - translation.Value);

                    // Just to make sure we are not applying steering behaviours while falling or jumping
                    if (
                        !jumpingFromEntity.HasComponent(entity) && 
                        !fallingFromEntity.HasComponent(entity) &&
                        flockingFromEntity.HasComponent(entity) )
                    {
                        navSteering.AgentAvoidanceSteering.y = 0; navSteering.SeparationSteering.y = 0; navSteering.AlignmentSteering.y = 0; navSteering.CohesionSteering.y = 0;
                        heading = math.normalizesafe(
                            heading +
                            navSteering.AgentAvoidanceSteering +
                            navSteering.SeparationSteering +
                            navSteering.AlignmentSteering +
                            navSteering.CohesionSteering
                        );

                        if (!navSteering.CollisionAvoidanceSteering.Equals(float3.zero))
                        {
                            heading = math.normalizesafe(heading + navSteering.CollisionAvoidanceSteering);
                        }
                    }
                    
                    navSteering.CurrentHeading = heading;
                })
                .WithName("NavSteeringJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
            buildPhysicsWorld.AddInputDependencyToComplete(Dependency);

            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            Entities
                .WithNone<NavProblem>()
                .WithAny<NavFalling, NavJumping>()
                .WithAll<LocalToParent>()
                .WithReadOnly(fallingFromEntity)
                .WithReadOnly(jumpBufferFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in NavAgent agent,
                    in Parent surface) =>
                {
                    if (agent.DestinationSurface.Equals(Entity.Null)) return;

                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);

                    if (!jumpBufferFromEntity.HasComponent(entity)) return;
                    var jumpBuffer = jumpBufferFromEntity[entity];

                    if (jumpBuffer.Length == 0 && !fallingFromEntity.HasComponent(entity)) return;

                    var destination = NavUtil.MultiplyPoint3x4(
                        localToWorldFromEntity[agent.DestinationSurface].Value,
                        agent.LocalDestination
                    );

                    var velocity = Vector3.Distance(translation.Value, destination) /
                                   (math.sin(2 * math.radians(agent.JumpDegrees)) / agent.JumpGravity);
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(agent.JumpDegrees));
                    var waypoint = translation.Value + math.up() * float.NegativeInfinity;

                    if (!fallingFromEntity.HasComponent(entity))
                    {
                        var xVelocity = math.sqrt(velocity) * math.cos(math.radians(agent.JumpDegrees)) *
                                        agent.JumpSpeedMultiplierX;

                        waypoint = NavUtil.MultiplyPoint3x4( // To world (from local in terms of destination surface).
                            localToWorldFromEntity[agent.DestinationSurface].Value,
                            jumpBuffer[0].Value
                        );

                        waypoint = NavUtil.MultiplyPoint3x4( // To local (in terms of agent's current surface).
                            math.inverse(localToWorldFromEntity[surface.Value].Value),
                            waypoint
                        );

                        translation.Value = Vector3.MoveTowards(translation.Value, waypoint, xVelocity * deltaSeconds);
                    }

                    translation.Value.y += (yVelocity - (elapsedSeconds - agent.JumpSeconds) * agent.JumpGravity) *
                                           deltaSeconds * agent.JumpSpeedMultiplierY;

                    if (elapsedSeconds - agent.JumpSeconds >= settings.JumpSecondsMax)
                    {
                        commandBuffer.RemoveComponent<NavJumping>(entityInQueryIndex, entity);
                        commandBuffer.AddComponent<NavFalling>(entityInQueryIndex, entity);
                    }

                    if (!NavUtil.ApproxEquals(translation.Value, waypoint, 1)) return;

                    commandBuffer.AddComponent<NavNeedsSurface>(entityInQueryIndex, entity);
                    commandBuffer.RemoveComponent<NavJumping>(entityInQueryIndex, entity);
                })
                .WithName("NavGravJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}