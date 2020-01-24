using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Interpolates "walking" and jumping via the path and jump
    /// buffers attached to a given NavAgent. Said buffers are determined by
    /// the NavPlanSystem, although *this* system is responsible for clearing
    /// the jump buffer. "Walking" is a simple lerp via Vector3.MoveTowards,
    /// and it also includes appropriate "look at" rotation. Jumping is
    /// is accomplished with artificial gravity and projectile motion math.
    /// This is not integrated with Unity.Physics, but does allow for gravity
    /// to be based upon the normal of the current surface's basis, which is a
    /// feature, not a bug. Later integration with Unity.Physics may be
    /// considered.</summary>
    class NavInterpolationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct JumpJob : IJobForEachWithEntity<NavAgent, Translation, Parent>
        {
            [ReadOnly]
            public float ElapsedSeconds;

            [ReadOnly]
            public float DeltaSeconds;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<NavJumpBufferElement> JumpBufferFromEntity;

            public void Execute(Entity entity, int index, ref NavAgent agent, ref Translation translation, [ReadOnly] ref Parent parent)
            {
                if (parent.Value.Equals(Entity.Null) || !agent.IsJumping) return;

                var jumpBuffer = JumpBufferFromEntity[entity];

                if (jumpBuffer.Length == 0) return;

                var destination = jumpBuffer[0].Value + agent.Offset;

                var velocity = Vector3.Distance(translation.Value, agent.WorldDestination) / (math.sin(2 * math.radians(agent.JumpDegrees)) / agent.JumpGravity);
                var xVelocity = math.sqrt(velocity) * math.cos(math.radians(agent.JumpDegrees));
                var yVelocity = math.sqrt(velocity) * math.sin(math.radians(agent.JumpDegrees));

                translation.Value = Vector3.MoveTowards(translation.Value, destination, xVelocity * DeltaSeconds);
                translation.Value.y += (yVelocity - (ElapsedSeconds - agent.JumpSeconds) * agent.JumpGravity) * DeltaSeconds;

                if (!NavUtil.ApproxEquals(translation.Value, destination)) return;

                agent.IsJumping = false;
                agent.HasJumped = true;
                jumpBuffer.Clear();
            }
        }

        [BurstCompile]
        struct WalkJob : IJobForEachWithEntity<NavAgent, Translation, Rotation, Parent>
        {
            [ReadOnly]
            public float ElapsedSeconds;

            [ReadOnly]
            public float DeltaSeconds;

            [ReadOnly]
            public BufferFromEntity<NavPathBufferElement> PathBufferFromEntity;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> LocalToWorldFromEntity;

            public void Execute(Entity entity, int index, ref NavAgent agent, ref Translation translation, ref Rotation rotation, [ReadOnly] ref Parent parent)
            {
                if (parent.Value.Equals(Entity.Null) || !agent.HasQueuedPathPlanning || !agent.HasDestination || agent.IsJumping) return;

                var pathBuffer = PathBufferFromEntity[entity];

                if (pathBuffer.Length == 0) return;

                if (agent.PathBufferIndex >= pathBuffer.Length)
                {
                    --agent.PathBufferIndex;
                    return;
                }

                var destination = pathBuffer[agent.PathBufferIndex].Value;

                if (!agent.DestinationSurface.Equals(Entity.Null))
                {
                    var destinationTransform = (Matrix4x4)LocalToWorldFromEntity[agent.DestinationSurface].Value;
                    agent.WorldDestination = destinationTransform.MultiplyPoint3x4(agent.LocalDestination);
                }

                var worldPosition4 = ((Matrix4x4)LocalToWorldFromEntity[entity].Value).GetColumn(3);
                var worldPosition3 = new float3(worldPosition4.x, worldPosition4.y, worldPosition4.z);

                if (
                    NavUtil.ApproxEquals(translation.Value, destination) &&
                    ++agent.PathBufferIndex > pathBuffer.Length - 1
                )
                {
                    if (!NavUtil.ApproxEquals(worldPosition3, agent.WorldDestination))
                    {
                        agent.IsJumping = true;
                        agent.JumpSeconds = ElapsedSeconds;
                        return;
                    }

                    agent.HasQueuedPathPlanning = agent.HasDestination = false;
                    agent.PathBufferIndex = 0;

                    return;
                }

                var lookAt = agent.WorldDestination;
                lookAt.y = worldPosition3.y;
                rotation.Value = quaternion.LookRotationSafe(lookAt - worldPosition3, math.up());

                translation.Value = Vector3.MoveTowards(translation.Value, destination, agent.TranslationSpeed * DeltaSeconds);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Not using Entities.ForEach here since, in 2019.3, the 'in'
            // keyword *appears* not to be optimized by the compiler like
            // '[ReadOnly]', resulting in the PlanSystem and *especially*
            // the built-in EndFrameParentSystem taking on too much load
            // from simply reading the Parent component in the following
            // jobs.

            var job = new JumpJob
            {
                ElapsedSeconds = (float)Time.ElapsedTime,
                DeltaSeconds = Time.DeltaTime,
                JumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>()
            }.Schedule(this, inputDeps);

            job = new WalkJob
            {
                ElapsedSeconds = (float)Time.ElapsedTime,
                DeltaSeconds = Time.DeltaTime,
                PathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>(true),
                LocalToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(this, job);

            return job;
        }
    }
}
