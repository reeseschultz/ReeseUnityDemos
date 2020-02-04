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
            var avoidantFromEntity = GetComponentDataFromEntity<NavAvoidant>(true);

            var walkJob = Entities
                .WithNone<NavPlanning, NavJumping>()
                .WithAll<NavLerping, Parent, LocalToParent>()
                .WithReadOnly(pathBufferFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(avoidantFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Translation translation, ref Rotation rotation) =>
                {
                    var pathBuffer = pathBufferFromEntity[entity];

                    if (pathBuffer.Length == 0) return;

                    if (agent.PathBufferIndex >= pathBuffer.Length)
                    {
                        --agent.PathBufferIndex;
                        return;
                    }

                    var destination = pathBuffer[agent.PathBufferIndex].Value;

                    if (!agent.DestinationSurface.Equals(Entity.Null))
                    {
                        var destinationTransform = (Matrix4x4)localToWorldFromEntity[agent.DestinationSurface].Value;
                        agent.WorldDestination = destinationTransform.MultiplyPoint3x4(agent.LocalDestination);
                    }

                    var avoidant = avoidantFromEntity.Exists(entity);
                    var worldDestination = avoidant ? agent.AvoidanceDestination : agent.WorldDestination;
                    var worldPosition4 = ((Matrix4x4)localToWorldFromEntity[entity].Value).GetColumn(3);
                    var worldPosition3 = new float3(worldPosition4.x, worldPosition4.y, worldPosition4.z);

                    if (
                        NavUtil.ApproxEquals(translation.Value, destination, 1) &&
                        ++agent.PathBufferIndex > pathBuffer.Length - 1
                    )
                    {
                        if (!avoidant) {
                            var rayInput = new RaycastInput
                            {
                                Start = translation.Value,
                                End = math.forward(rotation.Value) * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                                Filter = CollisionFilter.Default
                            };

                            if (
                                !physicsWorld.CastRay(rayInput, out RaycastHit hit) &&
                                !NavUtil.ApproxEquals(worldPosition3, worldDestination, 1)
                            )
                            {
                                agent.JumpSeconds = elapsedSeconds;
                                commandBuffer.AddComponent<NavJumping>(entityInQueryIndex, entity);
                                commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                                return;
                            }
                        }
 
                        commandBuffer.RemoveComponent<NavLerping>(entityInQueryIndex, entity);

                        if (avoidant) {
                            commandBuffer.RemoveComponent<NavAvoidant>(entityInQueryIndex, entity);
                            commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);
                        }

                        agent.PathBufferIndex = 0;

                        return;
                    }

                    var lookAt = worldDestination;
                    lookAt.y = worldPosition3.y;
                    rotation.Value = quaternion.LookRotationSafe(lookAt - worldPosition3, math.up());

                    translation.Value = Vector3.MoveTowards(translation.Value, destination, agent.TranslationSpeed * deltaSeconds);

                    agent.LastDestination = agent.WorldDestination;
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

            return Entities
                .WithAny<NavFalling, NavJumping>()
                .WithAll<Parent, LocalToParent>()
                .WithReadOnly(fallingFromEntity)
                .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Translation translation) =>
                {
                    commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity); // For sticking the landing.

                    var jumpBuffer = jumpBufferFromEntity[entity];

                    if (jumpBuffer.Length == 0 && !fallingFromEntity.Exists(entity)) return;

                    var velocity = Vector3.Distance(translation.Value, agent.WorldDestination) / (math.sin(2 * math.radians(agent.JumpDegrees)) / agent.JumpGravity);
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(agent.JumpDegrees));
                    var destination = translation.Value + math.up() * float.NegativeInfinity;

                    if (!fallingFromEntity.Exists(entity)) {
                        var xVelocity = math.sqrt(velocity) * math.cos(math.radians(agent.JumpDegrees));
                        destination = jumpBuffer[0].Value;
                        translation.Value = Vector3.MoveTowards(translation.Value, destination, xVelocity * deltaSeconds);
                    }

                    translation.Value.y += (yVelocity - (elapsedSeconds - agent.JumpSeconds) * agent.JumpGravity) * deltaSeconds;

                    if (elapsedSeconds - agent.JumpSeconds >= NavConstants.JUMP_SECONDS_MAX) {
                        commandBuffer.RemoveComponent<NavJumping>(entityInQueryIndex, entity);
                        commandBuffer.AddComponent<NavFalling>(entityInQueryIndex, entity);
                    }

                    if (!NavUtil.ApproxEquals(translation.Value, destination, 1)) return;

                    commandBuffer.AddComponent<NavNeedsSurface>(entityInQueryIndex, entity);
                    commandBuffer.RemoveComponent<NavJumping>(entityInQueryIndex, entity);

                    jumpBuffer.Clear();
                })
                .WithoutBurst()
                .WithName("NavArtificialGravityJob")
                .Schedule(walkJob);
        }
    }
}
