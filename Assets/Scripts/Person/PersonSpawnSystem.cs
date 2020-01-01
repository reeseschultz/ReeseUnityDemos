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
        static ConcurrentQueue<PersonSpawn> spawnQueue = new ConcurrentQueue<PersonSpawn>();
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        EntityQuery personPrefabQuery;
        EntityQuery randomBufferQuery;

        protected override void OnCreate()
        {
            personPrefabQuery = GetEntityQuery(typeof(PersonPrefab));
            randomBufferQuery = GetEntityQuery(typeof(RandomBufferElement));
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
            public DynamicBuffer<RandomBufferElement> RandomBuffer;

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

                var randomBuffer = RandomBuffer[threadIndex];

                CommandBuffer.SetComponent(i, entity, new Translation
                {
                    Value = new float3(
                        randomBuffer.Value.NextInt(-25, 25),
                        2,
                        randomBuffer.Value.NextInt(-25, 25)
                    )
                });

                RandomBuffer[threadIndex] = randomBuffer;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (spawnQueue.IsEmpty) return inputDeps;

            var randomBufferEntities = randomBufferQuery.ToEntityArray(Allocator.TempJob);
            if (randomBufferEntities.Length == 0) return inputDeps;
            var randomBuffer = GetBufferFromEntity<RandomBufferElement>()[randomBufferEntities[0]];
            randomBufferEntities.Dispose();

            var job = new SpawnJob
            {
                RandomBuffer = randomBuffer,
                PersonPrefabs = personPrefabQuery.ToComponentDataArray<PersonPrefab>(Allocator.TempJob),
                CommandBuffer = barrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(50, 128, inputDeps);

            barrier.AddJobHandleForProducer(job);

            return job;
        }
    }
}
