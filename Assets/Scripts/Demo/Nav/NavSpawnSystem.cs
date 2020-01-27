using System;
using System.Collections.Concurrent;
using Reese.Nav;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Reese.Demo
{
    struct NavAgentSpawn
    {
        public NavAgent Agent;
        public Rotation Rotation;
        public Translation Translation;
    }

    class NavSpawnSystem : JobComponentSystem
    {
        static ConcurrentQueue<NavAgentSpawn> spawnQueue = new ConcurrentQueue<NavAgentSpawn>();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        public static void Enqueue(NavAgentSpawn spawn)
            => spawnQueue.Enqueue(spawn);

        public static void Enqueue(NavAgentSpawn[] spawnArray)
            => Array.ForEach(spawnArray, spawn =>
            {
                spawnQueue.Enqueue(spawn);
            });

        public static void Enqueue(NavAgentSpawn spawn, int spawnCount)
        {
            for (int i = 0; i < spawnCount; ++i) spawnQueue.Enqueue(spawn);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (spawnQueue.IsEmpty) return inputDeps;

            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            var job = inputDeps;
            for (int i = 0; i < NavConstants.BATCH_MAX; ++i)
            {
                job = Entities
                    .ForEach((int entityInQueryIndex, ref NavAgentPrefab prefab) =>
                    {
                        if (!spawnQueue.TryDequeue(out NavAgentSpawn spawn)) return;

                        var entity = commandBuffer.Instantiate(entityInQueryIndex, prefab.Value);

                        commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(NavAgent));
                        commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(Translation));
                        commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(Rotation));
                        commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(NavPathBufferElement));
                        commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(NavJumpBufferElement));
                        commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(Parent));
                        commandBuffer.AddComponent(entityInQueryIndex, entity, typeof(LocalToParent));

                        commandBuffer.SetComponent(entityInQueryIndex, entity, spawn.Agent);
                        commandBuffer.SetComponent(entityInQueryIndex, entity, spawn.Rotation);
                        commandBuffer.SetComponent(entityInQueryIndex, entity, spawn.Translation);
                    })
                    .WithoutBurst()
                    .WithName("NavSpawnJob")
                    .Schedule(job);

                barrier.AddJobHandleForProducer(job);
            }

            return job;
        }
    }
}
