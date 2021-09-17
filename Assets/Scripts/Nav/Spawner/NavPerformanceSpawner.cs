using Reese.Nav;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Reese.EntityPrefabGroups;

namespace Reese.Demo
{
    class NavPerformanceSpawner : MonoBehaviour
    {
        [SerializeField]
        Button SpawnButton = default;

        [SerializeField]
        Button PrefabButton = default;

        [SerializeField]
        Text SpawnText = default;

        [SerializeField]
        Text PrefabText = default;

        [SerializeField]
        Slider Slider = default;

        [SerializeField]
        float3 SpawnOffset = new float3(0, 1, 0);

        int spawnCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity cylinderPrefab = default;
        Entity dinosaurPrefab = default;
        Entity currentPrefab = default;

        void Start()
        {
            if (SpawnButton == null || Slider == null) return;

            SpawnButton.onClick.AddListener(Spawn);
            PrefabButton.onClick.AddListener(TogglePrefab);
            Slider.onValueChanged.AddListener(UpdateSpawnCount);

            cylinderPrefab = entityManager.GetPrefab<Cylinder>();
            dinosaurPrefab = entityManager.GetPrefab<Dinosaur>();

            currentPrefab = cylinderPrefab;
        }

        void UpdateSpawnCount(float count)
        {
            spawnCount = (int)count;

            if (SpawnText == null) return;

            SpawnText.text = "Spawn " + spawnCount;

            if (spawnCount == 1) SpawnText.text += " Entity";
            else SpawnText.text += " Entities";
        }

        void TogglePrefab()
        {
            if (currentPrefab.Equals(dinosaurPrefab))
            {
                currentPrefab = cylinderPrefab;
                PrefabText.text = "Spawning Cylinders";
            }
            else
            {
                currentPrefab = dinosaurPrefab;
                PrefabText.text = "Spawning Dinosaurs";
            }
        }

        void Spawn()
        {
            var entities = new NativeArray<Entity>(spawnCount, Allocator.Temp);

            entityManager.Instantiate(currentPrefab, entities);

            for (var i = 0; i < entities.Length; ++i)
            {
                entityManager.AddComponentData(entities[i], new NavAgent
                {
                    TranslationSpeed = 20,
                    RotationSpeed = 0.3f,
                    TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                    Offset = new float3(0, 1, 0)
                });

                entityManager.AddComponentData<LocalToWorld>(entities[i], new LocalToWorld
                {
                    Value = float4x4.TRS(
                        new float3(0, 1, 0),
                        quaternion.identity,
                        1
                    )
                });

                entityManager.AddComponent<Parent>(entities[i]);
                entityManager.AddComponent<LocalToParent>(entities[i]);
                entityManager.AddComponent<NavNeedsSurface>(entities[i]);
            }

            entities.Dispose();
        }
    }
}
