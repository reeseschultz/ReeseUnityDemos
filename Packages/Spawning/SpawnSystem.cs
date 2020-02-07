using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Reese.Spawning
{
    /// <summary>A system for spawning entities with any prefab, components,
    /// and buffers (or lack thereof).</summary>
    public class SpawnSystem : JobComponentSystem
    {
        /// <summary>Number of spawn jobs to schedule per frame. The default
        /// value should work fine.</summary>
        public int spawnBatchCount = 50;

        /// <summary>The spawn queue.</summary>
        static ConcurrentQueue<Spawn> queue = new ConcurrentQueue<Spawn>();

        /// <summary>Reflected EntityCommandBuffer.Concurrent.AddBuffer for
        /// casting with runtime types instead of being restricted to
        /// compile-time.</summary>
        static MethodInfo addBuffer = typeof(EntityCommandBuffer.Concurrent)
            .GetMethods()
            .Where(method => method.Name == "AddBuffer")
            .Select(method => new
            {
                Method = method,
                Params = method.GetParameters(),
                Args = method.GetGenericArguments()
            })
            .Where(method => method.Params.Length == 2)
            .Select(method => method.Method)
            .First();

        /// <summary>Reflected EntityCommandBuffer.Concurrent.AddComponent for
        /// casting with runtime types instead of being restricted to
        /// compile-time.</summary>
        static MethodInfo addComponent = typeof(EntityCommandBuffer.Concurrent)
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

                var entity = spawn.Prefab.Equals(Entity.Null) ? CommandBuffer.CreateEntity(nativeThreadIndex) : CommandBuffer.Instantiate(nativeThreadIndex, spawn.Prefab);

                foreach (IBufferElementData buffer in spawn.BufferList) addBuffer.MakeGenericMethod(buffer.GetType()).Invoke(
                    CommandBuffer,
                    new object[] { nativeThreadIndex, entity }
                );

                foreach (IComponentData component in spawn.ComponentList) addComponent.MakeGenericMethod(component.GetType()).Invoke(
                    CommandBuffer,
                    new object[] { nativeThreadIndex, entity, component }
                );
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
