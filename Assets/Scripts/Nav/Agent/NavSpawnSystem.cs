using System.Collections.Concurrent;
using Unity.Entities;
using Unity.Transforms;

namespace Reese.Nav
{
    struct NavAgentSpawn
    {
        public NavAgent Agent;
        public Rotation Rotation;
        public Translation Translation;
    }

    class NavSpawnSystem : ComponentSystem
    {
        static ConcurrentQueue<NavAgentSpawn> spawnQueue = new ConcurrentQueue<NavAgentSpawn>();
        EntityManager entityManager => World.EntityManager;

        public static void Enqueue(NavAgentSpawn agentSpawn)
            => spawnQueue.Enqueue(agentSpawn);

        public static void Enqueue(NavAgentSpawn[] agentSpawns) {
            for (int i = 0; i < agentSpawns.Length; ++i) spawnQueue.Enqueue(agentSpawns[i]);
        }

        public static void Enqueue(NavAgentSpawn sharedSpawnData, int spawnCount) {
            for (int i = 0; i < spawnCount; ++i) spawnQueue.Enqueue(sharedSpawnData);
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((ref NavAgentPrefab agentPrefab) =>
            {
                for (int i = 0; i < NavConstants.BATCH_MAX; ++i)
                {
                    if (!spawnQueue.TryDequeue(out NavAgentSpawn agentSpawn)) return;

                    var entity = entityManager.Instantiate(agentPrefab.Prefab);

                    entityManager.AddComponent(entity, typeof(NavAgent));
                    entityManager.AddComponent(entity, typeof(Translation));
                    entityManager.AddComponent(entity, typeof(Rotation));
                    entityManager.AddComponent(entity, typeof(NavPathBufferElement));
                    entityManager.AddComponent(entity, typeof(NavJumpBufferElement));
                    entityManager.AddComponent(entity, typeof(Parent));
                    entityManager.AddComponent(entity, typeof(LocalToParent));

                    entityManager.SetComponentData(entity, agentSpawn.Agent);
                    entityManager.SetComponentData(entity, agentSpawn.Rotation);
                    entityManager.SetComponentData(entity, agentSpawn.Translation);
                }
            });
        }
    }
}
