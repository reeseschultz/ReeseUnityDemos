using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Reese.Nav
{
    /// <summary>Thread-safe system for dequeuing NavAgents for path and jump
    /// planning.</summary>
    class NavQueueSystem : JobComponentSystem
    {
        /// <summary>The path planning queue.</summary>
        static ConcurrentQueue<Entity> pathPlanningQueue = new ConcurrentQueue<Entity>();

        /// <summary>The jump planning queue.</summary>
        static ConcurrentQueue<Entity> jumpPlanningQueue = new ConcurrentQueue<Entity>();

        /// <summary>Dequeues an entity for path planning.</summary>
        public static Entity DequeuePathPlanning()
        {
            pathPlanningQueue.TryDequeue(out Entity entity);
            return entity;
        }

        /// <summary>Dequeues an entity for jump planning.</summary>
        public static Entity DequeueJumpPlanning()
        {
            jumpPlanningQueue.TryDequeue(out Entity entity);
            return entity;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var jumpingFromEntity = GetComponentDataFromEntity<NavJumping>(true);

            return Entities
                .WithReadOnly(jumpingFromEntity)
                .ForEach((Entity entity, ref NavAgent agent) =>
                {
                    if (jumpingFromEntity.Exists(entity)) jumpPlanningQueue.Enqueue(entity);
                    else if (!agent.HasQueuedPathPlanning && agent.HasDestination)
                    {
                        agent.HasQueuedPathPlanning = true;
                        pathPlanningQueue.Enqueue(entity);
                    }
                })
                .WithName("NavQueueJob")
                .WithoutBurst()
                .Schedule(inputDeps);
        }
    }
}
