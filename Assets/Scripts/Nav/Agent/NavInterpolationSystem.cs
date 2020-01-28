using Unity.Burst;
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
    /// <summary>Interpolates "walking" and jumping via the path and jump
    /// buffers attached to a given NavAgent. Said buffers are determined by
    /// the NavPlanSystem, although *this* system is responsible for clearing
    /// the jump buffer. "Walking" is a simple lerp via Vector3.MoveTowards,
    /// and it also includes appropriate "look at" rotation. Jumping is
    /// is accomplished with artificial gravity and projectile motion math.
    /// For more info on how the jumping works, see
    /// https://reeseschultz.com/projectile-motion-with-unity-dots. Similar
    /// code supports the ProjectileDemo scene.
    /// </summary>
    class NavInterpolationSystem : JobComponentSystem
    {
        /// <summary>Used for raycasting in order to detect an obstacle in
        /// front of the agent.</summary>
        BuildPhysicsWorld buildPhysicsWorldSystem => World.GetExistingSystem<BuildPhysicsWorld>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var elapsedSeconds = (float)Time.ElapsedTime;
            var deltaSeconds = Time.DeltaTime;
            var physicsWorld = buildPhysicsWorldSystem.PhysicsWorld;
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>(true);
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            var walkJob = Entities
                .WithAll<Parent, LocalToParent>()
                .WithReadOnly(physicsWorld)
                .WithReadOnly(pathBufferFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, ref NavAgent agent, ref Translation translation, ref Rotation rotation) =>
                {
                    if (
                        !agent.HasQueuedPathPlanning ||
                        !agent.HasDestination ||
                        agent.IsJumping
                    ) return;

                    if (!agent.IsLerping) agent.IsLerping = true;

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

                    var worldPosition4 = ((Matrix4x4)localToWorldFromEntity[entity].Value).GetColumn(3);
                    var worldPosition3 = new float3(worldPosition4.x, worldPosition4.y, worldPosition4.z);

                    if (
                        NavUtil.ApproxEquals(translation.Value, destination) &&
                        ++agent.PathBufferIndex > pathBuffer.Length - 1
                    )
                    {
                        var rayInput = new RaycastInput
                        {
                            Start = translation.Value,
                            End = math.forward(rotation.Value) * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                            Filter = CollisionFilter.Default
                        };

                        if (
                            !physicsWorld.CastRay(rayInput, out RaycastHit hit) &&
                            !NavUtil.ApproxEquals(worldPosition3, agent.WorldDestination)
                        )
                        {
                            agent.IsJumping = true;
                            agent.JumpSeconds = elapsedSeconds;
                            return;
                        }

                        agent.ClearDestination();
                        return;
                    }

                    var lookAt = agent.WorldDestination;
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

            return Entities
                .WithAll<Parent, LocalToParent>()
                .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                .ForEach((Entity entity, ref NavAgent agent, ref Translation translation) => {
                    if (!agent.IsJumping) return;

                    var jumpBuffer = jumpBufferFromEntity[entity];

                    if (jumpBuffer.Length == 0) return;

                    var destination = jumpBuffer[0].Value;
                    var velocity = Vector3.Distance(translation.Value, agent.WorldDestination) / (math.sin(2 * math.radians(agent.JumpDegrees)) / agent.JumpGravity);
                    var xVelocity = math.sqrt(velocity) * math.cos(math.radians(agent.JumpDegrees));
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(agent.JumpDegrees));

                    translation.Value = Vector3.MoveTowards(translation.Value, destination, xVelocity * deltaSeconds);
                    translation.Value.y += (yVelocity - (elapsedSeconds - agent.JumpSeconds) * agent.JumpGravity) * deltaSeconds;

                    if (!NavUtil.ApproxEquals(translation.Value, destination)) return;

                    agent.IsJumping = false;
                    agent.HasJumped = true;
                    jumpBuffer.Clear();
                })
                .WithName("NavJumpJob")
                .Schedule(walkJob);
        }
    }
}
