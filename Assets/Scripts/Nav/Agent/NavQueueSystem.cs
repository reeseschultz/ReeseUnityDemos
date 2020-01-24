using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Reese.Nav
{
    /// <summary>Thread-safe system for enqueuing and dequeuing NavAgents for
    /// path and jump planning in the NavPlanSystem.</summary>
    class NavQueueSystem : JobComponentSystem
    {
        /// <summary>The path planning queue.</summary>
        static ConcurrentQueue<Entity> pathPlanningQueue = new ConcurrentQueue<Entity>();

        /// <summary>The jump planning queue.</summary>
        static ConcurrentQueue<Entity> jumpPlanningQueue = new ConcurrentQueue<Entity>();

        /// <summary>Enqueues an entity for path planning.</summary>
        public static void EnqueuePathPlanning(Entity entity)
            => pathPlanningQueue.Enqueue(entity);

        /// <summary>Dequeues an entity for path planning.</summary>
        public static Entity DequeuePathPlanning()
        {
            pathPlanningQueue.TryDequeue(out Entity entity);
            return entity;
        }

        /// <summary>Enqueues an entity for jump planning.</summary>
        public static void EnqueueJumpPlanning(Entity entity)
            => jumpPlanningQueue.Enqueue(entity);

        /// <summary>Dequeues an entity for jump planning.</summary>
        public static Entity DequeueJumpPlanning()
        {
            jumpPlanningQueue.TryDequeue(out Entity entity);
            return entity;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
            => Entities
                .ForEach((Entity entity, ref NavAgent agent) => {
                    if (agent.IsJumping) EnqueueJumpPlanning(entity);
                    else if (!agent.HasQueuedPathPlanning && agent.HasDestination)
                    {
                        agent.HasQueuedPathPlanning = true;
                        EnqueuePathPlanning(entity);
                    }
                })
                .WithName("NavQueueJob")
                .Schedule(inputDeps);
    }
}
