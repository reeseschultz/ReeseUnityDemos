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
    [UpdateAfter(typeof(NavDestinationSystem))]
    public class NavInterpolationSystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetExistingSystem<BuildPhysicsWorld>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        static void HandleCompletePath(ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity, Entity entity, Rotation rotation, ref NavAgent agent, ref DynamicBuffer<NavPathBufferElement> pathBuffer, Parent surface, Translation translation, PhysicsWorld physicsWorld, float elapsedSeconds, EntityCommandBuffer.ParallelWriter commandBuffer, int entityInQueryIndex)
        {
            var rayInput = new RaycastInput
            {
                Start = localToWorldFromEntity[entity].Position + agent.Offset,
                End = math.forward(rotation.Value) * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                Filter = new CollisionFilter
                {
                    BelongsTo = NavUtil.ToBitMask(NavConstants.COLLIDER_LAYER),
                    CollidesWith = NavUtil.ToBitMask(NavConstants.OBSTACLE_LAYER)
                }
            };

            if (
                !surface.Value.Equals(agent.DestinationSurface) &&
                !NavUtil.ApproxEquals(translation.Value, agent.LocalDestination, 1) &&
                !physicsWorld.CastRay(rayInput, out RaycastHit hit)
            )
            {
                agent.JumpSeconds = elapsedSeconds;

                commandBuffer.RemoveComponent<NavWalking>(entityInQueryIndex, entity);
                commandBuffer.AddComponent<NavJumping>(entityInQueryIndex, entity);
                commandBuffer.AddComponent<NavPlanning>(entityInQueryIndex, entity);

                return;
            }

            commandBuffer.RemoveComponent<NavLerping>(entityInQueryIndex, entity);
            commandBuffer.RemoveComponent<NavWalking>(entityInQueryIndex, entity);
            commandBuffer.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);

            return;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var elapsedSeconds = (float)Time.ElapsedTime;
            var deltaSeconds = Time.DeltaTime;
            var physicsWorld = buildPhysicsWorld.PhysicsWorld;
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>();
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithNone<NavHasProblem, NavPlanning>()
                .WithAll<NavWalking, LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(pathBufferFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, ref Translation translation, ref Rotation rotation, in Parent surface) =>
                {
                    if (!pathBufferFromEntity.HasComponent(entity) || agent.DestinationSurface.Equals(Entity.Null)) return;

                    var pathBuffer = pathBufferFromEntity[entity];

                    if (pathBuffer.Length == 0)
                    {
                        HandleCompletePath(localToWorldFromEntity, entity, rotation, ref agent, ref pathBuffer, surface, translation, physicsWorld, elapsedSeconds, commandBuffer, entityInQueryIndex);
                        return;
                    }

                    var pathBufferLength = pathBuffer.Length - 1;

                    if (NavUtil.ApproxEquals(translation.Value, pathBuffer[pathBufferLength].Value, 1)) pathBuffer.RemoveAt(pathBufferLength);

                    if (pathBuffer.Length == 0) return;

                    pathBufferLength = pathBuffer.Length - 1;

                    translation.Value = Vector3.MoveTowards(translation.Value, pathBuffer[pathBufferLength].Value, agent.TranslationSpeed * deltaSeconds);

                    var lookAt = NavUtil.MultiplyPoint3x4( // To world (from local in terms of destination surface).
                        localToWorldFromEntity[agent.DestinationSurface].Value,
                        pathBuffer[pathBufferLength].Value
                    );

                    lookAt = NavUtil.MultiplyPoint3x4( // To local (in terms of agent's current surface).
                        math.inverse(localToWorldFromEntity[surface.Value].Value),
                        lookAt
                    );

                    lookAt.y = translation.Value.y;

                    var lookRotation = quaternion.LookRotationSafe(lookAt - translation.Value, math.up());

                    if (math.length(agent.SurfacePointNormal) > 0.01f)
                        lookRotation = Quaternion.FromToRotation(math.up(), agent.SurfacePointNormal) * lookRotation;

                    rotation.Value = math.slerp(rotation.Value, lookRotation, deltaSeconds / agent.RotationSpeed);
                })
                .WithName("NavWalkJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var fallingFromEntity = GetComponentDataFromEntity<NavFalling>();

            Entities
                .WithNone<NavHasProblem>()
                .WithAny<NavFalling, NavJumping>()
                .WithAll<LocalToParent>()
                .WithReadOnly(fallingFromEntity)
                .WithReadOnly(jumpBufferFromEntity)
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in NavAgent agent, in Parent surface) =>
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

                    var velocity = Vector3.Distance(translation.Value, destination) / (math.sin(2 * math.radians(agent.JumpDegrees)) / agent.JumpGravity);
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(agent.JumpDegrees));
                    var waypoint = translation.Value + math.up() * float.NegativeInfinity;

                    if (!fallingFromEntity.HasComponent(entity))
                    {
                        var xVelocity = math.sqrt(velocity) * math.cos(math.radians(agent.JumpDegrees)) * agent.JumpSpeedMultiplierX;

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

                    translation.Value.y += (yVelocity - (elapsedSeconds - agent.JumpSeconds) * agent.JumpGravity) * deltaSeconds * agent.JumpSpeedMultiplierY;

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
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
