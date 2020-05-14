﻿using Unity.Collections;
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
    /// math. For more info on how the jumping works, see
    /// https://reeseschultz.com/projectile-motion-with-unity-dots. Similar
    /// code supports the ProjectileDemo scene.
    /// </summary>
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    class NavInterpolationSystem : JobComponentSystem
    {
        /// <summary>For raycasting in order to detect an obstacle in
        /// front of the agent.</summary>
        BuildPhysicsWorld buildPhysicsWorldSystem => World.GetExistingSystem<BuildPhysicsWorld>();

        /// <summary>For adding and removing statuses to and from the agent.
        /// </summary>
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var elapsedSeconds = (float)Time.ElapsedTime;
            var deltaSeconds = Time.DeltaTime;
            var physicsWorld = buildPhysicsWorldSystem.PhysicsWorld;
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>(true);
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            var walkJob = Entities
                .WithNone<NavPlanning, NavJumping>()
                .WithAll<NavLerping, LocalToParent>()
                .WithReadOnly(pathBufferFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Translation translation, ref Rotation rotation, in Parent surface) =>
                {
                    var pathBuffer = pathBufferFromEntity[entity];

                    if (pathBuffer.Length == 0) return;

                    if (agent.PathBufferIndex >= pathBuffer.Length)
                    {
                        --agent.PathBufferIndex;
                        return;
                    }

                    var worldDestination = agent.GetWorldDestination(
                        localToWorldFromEntity[surface.Value].Value
                    );
                    var localDestination = agent.LocalDestination;
                    localDestination.y = agent.Offset.y;

                    var worldPosition = localToWorldFromEntity[entity].Position;
                    var localPosition = translation.Value;

                    var worldWaypoint = pathBuffer[agent.PathBufferIndex].Value;
                    var localWaypoint = NavUtil.MultiplyPoint3x4(
                        math.inverse(localToWorldFromEntity[surface.Value].Value),
                        worldWaypoint
                    );
                    localWaypoint.y = agent.Offset.y;

                    if (
                        NavUtil.ApproxEquals(localPosition, localWaypoint, 1) &&
                        ++agent.PathBufferIndex > pathBuffer.Length - 1
                    )
                    {
                        var rayInput = new RaycastInput
                        {
                            Start = worldPosition,
                            End = math.forward(rotation.Value) * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                            Filter = CollisionFilter.Default
                        };

                        if (
                            !physicsWorld.CastRay(rayInput, out RaycastHit hit) &&
                            !NavUtil.ApproxEquals(localPosition, localDestination, 1)
                        )
                        {
                            agent.JumpSeconds = elapsedSeconds;
                            commandBuffer.AddComponent<NavJumping>(entityInQueryIndex, entity);
                            commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                            return;
                        }

                        commandBuffer.RemoveComponent<NavLerping>(entityInQueryIndex, entity);
                        commandBuffer.RemoveComponent<NavDestination>(entityInQueryIndex, entity);
                        agent.PathBufferIndex = 0;
                        return; 
                    }

                    var lookAt = localDestination;
                    lookAt.y = localPosition.y;
                    rotation.Value = quaternion.LookRotationSafe(lookAt - localPosition, math.up());

                    translation.Value = Vector3.MoveTowards(localPosition, localWaypoint, agent.TranslationSpeed * deltaSeconds);

                    agent.LastDestination = worldDestination;
                })
                .WithName("NavWalkJob")
                .Schedule(
                    JobHandle.CombineDependencies(
                        inputDeps,
                        buildPhysicsWorldSystem.FinalJobHandle
                    )
                );

            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var fallingFromEntity = GetComponentDataFromEntity<NavFalling>();

            var artificialGravityJob = Entities
                .WithAny<NavFalling, NavJumping>()
                .WithAll<LocalToParent>()
                .WithReadOnly(fallingFromEntity)
                .WithReadOnly(jumpBufferFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in NavAgent agent, in Parent surface) =>
                {
                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);

                    var jumpBuffer = jumpBufferFromEntity[entity];

                    if (jumpBuffer.Length == 0 && !fallingFromEntity.Exists(entity)) return;

                    var worldDestination = agent.GetWorldDestination(
                        localToWorldFromEntity[surface.Value].Value
                    );

                    var velocity = Vector3.Distance(translation.Value, worldDestination) / (math.sin(2 * math.radians(agent.JumpDegrees)) / agent.JumpGravity);
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(agent.JumpDegrees));
                    var worldWaypoint = translation.Value + math.up() * float.NegativeInfinity;

                    if (!fallingFromEntity.Exists(entity)) {
                        var xVelocity = math.sqrt(velocity) * math.cos(math.radians(agent.JumpDegrees));

                        worldWaypoint = NavUtil.MultiplyPoint3x4(
                            math.inverse(localToWorldFromEntity[surface.Value].Value),
                            jumpBuffer[0].Value
                        );

                        translation.Value = Vector3.MoveTowards(translation.Value, worldWaypoint, xVelocity * deltaSeconds);
                    }

                    translation.Value.y += (yVelocity - (elapsedSeconds - agent.JumpSeconds) * agent.JumpGravity) * deltaSeconds;

                    if (elapsedSeconds - agent.JumpSeconds >= NavConstants.JUMP_SECONDS_MAX) {
                        commandBuffer.RemoveComponent<NavJumping>(entityInQueryIndex, entity);
                        commandBuffer.AddComponent<NavFalling>(entityInQueryIndex, entity);
                    }

                    if (!NavUtil.ApproxEquals(translation.Value, worldWaypoint, 1)) return;

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
