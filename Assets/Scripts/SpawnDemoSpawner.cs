using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Reese.EntityPrefabGroups;
using SpawnDemo;

namespace Reese.Demo
{
    class SpawnDemoSpawner : MonoBehaviour
    {
        [SerializeField]
        Button Button = default;

        [SerializeField]
        Text SpawnText = default;

        [SerializeField]
        Slider Slider = default;

        int spawnCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityPrefabSystem prefabSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityPrefabSystem>();

        Entity prefab = default;

        void Start()
        {
            if (Button == null || Slider == null) return;

            Button.onClick.AddListener(Spawn);
            Slider.onValueChanged.AddListener(UpdateSpawnCount);

            prefabSystem.TryGet(Prefabs.PersonPrefab, out prefab);
        }

        void UpdateSpawnCount(float count)
        {
            spawnCount = (int)count;

            if (SpawnText == null) return;

            SpawnText.text = "Spawn " + spawnCount;

            if (spawnCount == 1) SpawnText.text += " Entity";
            else SpawnText.text += " Entities";
        }

        void Spawn()
        {
            if (prefab == Entity.Null) return;

            var random = new Unity.Mathematics.Random((uint)new System.Random().Next());

            var entities = new NativeArray<Entity>(spawnCount, Allocator.Temp);

            entityManager.Instantiate(prefab, entities);

            for (var i = 0; i < entities.Length; ++i)
            {
                entityManager.AddComponentData(entities[i], new Translation
                {
                    Value = new float3(
                        random.NextInt(-25, 25),
                        2,
                        random.NextInt(-25, 25)
                    )
                });
            }

            entities.Dispose();
        }
    }
}
