using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ReeseUnityDemos
{
    class PersonSpawnSystem : JobComponentSystem
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        EntityQuery personPrefabQuery;
        static ConcurrentQueue<PersonSpawn> spawnQueue = new ConcurrentQueue<PersonSpawn>();

        protected override void OnCreate()
        {
            personPrefabQuery = GetEntityQuery(typeof(PersonPrefab));
        }

        public static void Enqueue(PersonSpawn personSpawn)
        {
            spawnQueue.Enqueue(personSpawn);
        }

        public static void Enqueue(PersonSpawn[] personSpawns)
        {
            for (int i = 0; i < personSpawns.Length; ++i) spawnQueue.Enqueue(personSpawns[i]);
        }

        public static void Enqueue(PersonSpawn sharedSpawnData, int spawnCount)
        {
            for (int i = 0; i < spawnCount; ++i) spawnQueue.Enqueue(sharedSpawnData);
        }

        struct SpawnJob : IJobParallelFor
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<PersonPrefab> PersonPrefabs;

            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [NativeDisableParallelForRestriction]
            public NativeArray<Unity.Mathematics.Random> RandomArray;

            [NativeSetThreadIndex]
            int threadIndex;

            public void Execute(int i)
            {
                if (
                    PersonPrefabs.Length == 0 ||
                    !spawnQueue.TryDequeue(out PersonSpawn spawn)
                ) return;

                var entity = CommandBuffer.Instantiate(i, PersonPrefabs[0].Prefab);

                CommandBuffer.AddComponent(i, entity, spawn.Person);
                CommandBuffer.AddComponent(i, entity, spawn.Rotation);
                CommandBuffer.AddComponent(i, entity, spawn.Translation);

                if (!spawn.Person.RandomizeTranslation) return;

                var random = RandomArray[threadIndex];

                CommandBuffer.SetComponent(i, entity, new Translation
                {
                    Value = new float3(
                        random.NextInt(-25, 25),
                        2,
                        random.NextInt(-25, 25)
                    )
                });

                RandomArray[threadIndex] = random;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (spawnQueue.IsEmpty) return inputDeps;

            var job = new SpawnJob
            {
                PersonPrefabs = personPrefabQuery.ToComponentDataArray<PersonPrefab>(Allocator.TempJob),
                CommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
                RandomArray = World.GetExistingSystem<RandomSystem>().RandomArray
            }.Schedule(50, 128, inputDeps);

            barrier.AddJobHandleForProducer(job);

            return job;
        }
    }
}
