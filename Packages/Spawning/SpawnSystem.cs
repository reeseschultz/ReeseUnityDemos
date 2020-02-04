using System;
using System.Collections.Concurrent;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Reese.Spawning
{
    /// <summary>A system for spawning from any set of prefabs and components.
    /// </summary>
    public class SpawnSystem : JobComponentSystem
    {
        /// <summary>Number of spawn jobs to schedule per frame. The default
        /// value should work fine.</summary>
        public int spawnBatchCount = 50;

        /// <summary>The spawn queue.</summary>
        static ConcurrentQueue<Spawn> queue = new ConcurrentQueue<Spawn>();

        /// <summary>For creating a command buffer to spawn entities.</summary>
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        /// <summary>Enqueues a single spawn.</summary>
        public static void Enqueue(Spawn spawn)
            => queue.Enqueue(spawn);

        /// <summary>Enqueues an array of spawns.</summary>
        public static void Enqueue(Spawn[] spawnArray)
            => Array.ForEach(spawnArray, spawn =>
            {
                queue.Enqueue(spawn);
            });

        /// <summary>Enqueues the same spawn data 'spawnCount' times.
        /// </summary>
        public static void Enqueue(Spawn spawn, int spawnCount)
        {
            for (int i = 0; i < spawnCount; ++i) queue.Enqueue(spawn);
        }

        struct SpawnJob : IJob
        {
            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [NativeSetThreadIndex]
            int nativeThreadIndex;

            public void Execute()
            {
                if (!queue.TryDequeue(out Spawn spawn)) return;

                var entity = CommandBuffer.Instantiate(nativeThreadIndex, spawn.PrefabEntity);

                var addComponent = typeof(EntityCommandBuffer.Concurrent)
                    .GetMethods()
                    .Where(method => method.Name == "AddComponent")
                    .Select(method => new
                    {
                        Method = method,
                        Params = method.GetParameters(),
                        Args = method.GetGenericArguments()
                    })
                    .Where(method => method.Params.Length == 3)
                    .Select(method => method.Method)
                    .First();

                for (int i = 0; i < spawn.Count; ++i) {
                    var genericAddComponent = addComponent.MakeGenericMethod(spawn[i].GetType());

                    genericAddComponent.Invoke(
                        CommandBuffer,
                        new object[] { nativeThreadIndex, entity, spawn[i] }
                    );
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (queue.IsEmpty) return inputDeps;

            var job = inputDeps;
            for (int i = 0; i < spawnBatchCount; ++i)
            {
                job = new SpawnJob
                {
                    CommandBuffer = barrier.CreateCommandBuffer().ToConcurrent()
                }.Schedule(job);

                barrier.AddJobHandleForProducer(job);
            }

            return job;
        }
    }
}
