using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Interpolates "walking," jumping and falling via the
    /// buffers attached to a given NavAgent. Said buffers are determined by
    /// the NavPlanSystem, although *this* system is responsible for clearing
    /// the jump buffer. "Walking" is a simple lerp via Vector3.MoveTowards,
    /// and it also includes appropriate "look at" rotation. Jumping and
    /// falling are accomplished with artificial gravity and projectile motion
    /// math.
    /// </summary>
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    class NavInterpolationSystem : JobComponentSystem
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var elapsedSeconds = (float)Time.ElapsedTime;
            var deltaSeconds = Time.DeltaTime;
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>(true);
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            var walkJob = Entities
                .WithNone<NavPlanning, NavJumping>()
                .WithAll<NavLerping, LocalToParent>()
                .WithReadOnly(pathBufferFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Translation translation, ref Rotation rotation, in Parent surface) =>
                {
                    var pathBuffer = pathBufferFromEntity[entity];

                    if (pathBuffer.Length == 0) return;

                    if (agent.PathBufferIndex >= pathBuffer.Length)
                    {
                        --agent.PathBufferIndex;
                        return;
                    }

                    var localDestination = agent.LocalDestination;

                    var localWaypoint = pathBuffer[agent.PathBufferIndex].Value;
                    localWaypoint.y = agent.Offset.y;

                    if (
                        NavUtil.ApproxEquals(translation.Value, localWaypoint, 1) &&
                        ++agent.PathBufferIndex > pathBuffer.Length - 1
                    )
                    {
                        var rayInput = new RaycastInput
                        {
                            Start = localToWorldFromEntity[entity].Position + agent.Offset,
                            End = math.forward(rotation.Value) * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                            Filter = CollisionFilter.Default // TODO : Resolve via Issue #3.
                        };

                        if (
                            !physicsWorld.CastRay(rayInput, out RaycastHit hit) &&
                            !NavUtil.ApproxEquals(translation.Value, localDestination, 1)
                        )
                        {
                            agent.JumpSeconds = elapsedSeconds;
                            commandBuffer.AddComponent<NavJumping>(entityInQueryIndex, entity);
                            commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                            return;
                        }

                        commandBuffer.RemoveComponent<NavLerping>(entityInQueryIndex, entity);
                        commandBuffer.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);
                        agent.PathBufferIndex = 0;
                        return;
                    }

                    var lookAt = localDestination;
                    lookAt.y = translation.Value.y;
                    rotation.Value = quaternion.LookRotationSafe(lookAt - translation.Value, math.up());

                    translation.Value = Vector3.MoveTowards(translation.Value, localWaypoint, agent.TranslationSpeed * deltaSeconds);
                })
                .WithName("NavWalkJob")
                .Schedule(
                    JobHandle.CombineDependencies(
                        inputDeps,
                        buildPhysicsWorld.FinalJobHandle
                    )
                );

            barrier.AddJobHandleForProducer(walkJob);

            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var parentFromEntity = GetComponentDataFromEntity<Parent>();
            var fallingFromEntity = GetComponentDataFromEntity<NavFalling>();

            var artificialGravityJob = Entities
                .WithAny<NavFalling, NavJumping>()
                .WithAll<LocalToParent>()
                .WithReadOnly(fallingFromEntity)
                .WithReadOnly(jumpBufferFromEntity)
                .WithReadOnly(parentFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in NavAgent agent) =>
                {
                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);

                    var jumpBuffer = jumpBufferFromEntity[entity];

                    if (jumpBuffer.Length == 0 && !fallingFromEntity.Exists(entity)) return;

                    var destination = localToWorldFromEntity[agent.Destination].Position;
                    var velocity = Vector3.Distance(translation.Value, destination) / (math.sin(2 * math.radians(agent.JumpDegrees)) / agent.JumpGravity);
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(agent.JumpDegrees));
                    var waypoint = translation.Value + math.up() * float.NegativeInfinity;

                    if (!fallingFromEntity.Exists(entity))
                    {
                        var xVelocity = math.sqrt(velocity) * math.cos(math.radians(agent.JumpDegrees));
                        var agentSurface = parentFromEntity[entity].Value;
                        var destinationSurface = parentFromEntity[agent.Destination].Value;

                        waypoint = NavUtil.MultiplyPoint3x4( // To world (from local in terms of destination surface).
                            localToWorldFromEntity[destinationSurface].Value,
                            jumpBuffer[0].Value
                        );

                        waypoint = NavUtil.MultiplyPoint3x4( // To local (in terms of agent's current surface).
                            math.inverse(localToWorldFromEntity[agentSurface].Value),
                            waypoint
                        );

                        translation.Value = Vector3.MoveTowards(translation.Value, waypoint, xVelocity * deltaSeconds);
                    }

                    translation.Value.y += (yVelocity - (elapsedSeconds - agent.JumpSeconds) * agent.JumpGravity) * deltaSeconds;

                    if (elapsedSeconds - agent.JumpSeconds >= NavConstants.JUMP_SECONDS_MAX)
                    {
                        commandBuffer.RemoveComponent<NavJumping>(entityInQueryIndex, entity);
                        commandBuffer.AddComponent<NavFalling>(entityInQueryIndex, entity);
                    }

                    if (!NavUtil.ApproxEquals(translation.Value, waypoint, 1)) return;

                    commandBuffer.AddComponent<NavNeedsSurface>(entityInQueryIndex, entity);
                    commandBuffer.RemoveComponent<NavJumping>(entityInQueryIndex, entity);
                })
                .WithName("NavArtificialGravityJob")
                .Schedule(walkJob);

            barrier.AddJobHandleForProducer(artificialGravityJob);

            return artificialGravityJob;
        }
    }
}
