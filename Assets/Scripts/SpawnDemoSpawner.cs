using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;

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
        Entity prefabEntity;

        void Start()
        {
            if (Button == null || Slider == null) return;

            Button.onClick.AddListener(Spawn);
            Slider.onValueChanged.AddListener(UpdateSpawnCount);

            prefabEntity = entityManager.CreateEntityQuery(typeof(PersonPrefab)).GetSingleton<PersonPrefab>().Value;
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
            var random = new Unity.Mathematics.Random((uint)new System.Random().Next());

            var outputEntities = new NativeArray<Entity>(spawnCount, Allocator.Temp);

            entityManager.Instantiate(prefabEntity, outputEntities);

            for (var i = 0; i < outputEntities.Length; ++i)
            {
                entityManager.AddComponentData(outputEntities[i], new Translation
                {
                    Value = new float3(
                        random.NextInt(-25, 25),
                        2,
                        random.NextInt(-25, 25)
                    )
                });
            }

            outputEntities.Dispose();
        }
    }
}
