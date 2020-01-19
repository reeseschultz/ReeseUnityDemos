using System;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Demo
{
    class PersonSpawnSystem : JobComponentSystem
    {
        public static readonly int SPAWN_BATCH_MAX = 50;

        static ConcurrentQueue<PersonSpawn> spawnQueue = new ConcurrentQueue<PersonSpawn>();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        EntityQuery personPrefabQuery;

        public static void Enqueue(PersonSpawn spawn)
            => spawnQueue.Enqueue(spawn);

        public static void Enqueue(PersonSpawn[] spawnArray)
            => Array.ForEach(spawnArray, spawn =>
            {
                spawnQueue.Enqueue(spawn);
            });

        public static void Enqueue(PersonSpawn spawn, int spawnCount)
        {
            for (int i = 0; i < spawnCount; ++i) spawnQueue.Enqueue(spawn);
        }

        protected override void OnCreate()
            => personPrefabQuery = GetEntityQuery(typeof(PersonPrefab));

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (spawnQueue.IsEmpty) return inputDeps;

            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            var job = inputDeps;
            for (int i = 0; i < SPAWN_BATCH_MAX; ++i)
            {
                job = Entities
                    .WithNativeDisableParallelForRestriction(randomArray)
                    .ForEach((int entityInQueryIndex, int nativeThreadIndex, ref PersonPrefab prefab) =>
                    {
                        if (
                            !spawnQueue.TryDequeue(out PersonSpawn spawn)
                        ) return;

                        var entity = commandBuffer.Instantiate(entityInQueryIndex, prefab.Value);

                        commandBuffer.AddComponent(entityInQueryIndex, entity, spawn.Person);
                        commandBuffer.AddComponent(entityInQueryIndex, entity, spawn.Rotation);
                        commandBuffer.AddComponent(entityInQueryIndex, entity, spawn.Translation);

                        if (!spawn.Person.RandomizeTranslation) return;

                        var random = randomArray[nativeThreadIndex];

                        commandBuffer.SetComponent(entityInQueryIndex, entity, new Translation
                        {
                            Value = new float3(
                                random.NextInt(-25, 25),
                                2,
                                random.NextInt(-25, 25)
                            )
                        });

                        randomArray[nativeThreadIndex] = random;
                    })
                    .WithoutBurst()
                    .WithName("SpawnJob")
                    .Schedule(job);

                barrier.AddJobHandleForProducer(job);
            }

            return job;
        }
    }
}
