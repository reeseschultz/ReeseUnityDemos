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
    class NavTerrainSpawner : MonoBehaviour
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

        int enqueueCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity cylinderPrefab = default;
        Entity dinosaurPrefab = default;
        Entity currentPrefab = default;

        void Start()
        {
            if (SpawnButton == null || Slider == null) return;

            SpawnButton.onClick.AddListener(Enqueue);
            PrefabButton.onClick.AddListener(TogglePrefab);
            Slider.onValueChanged.AddListener(UpdateEnqueueCount);

            cylinderPrefab = entityManager.GetPrefab<Cylinder>();
            dinosaurPrefab = entityManager.GetPrefab<Dinosaur>();

            currentPrefab = cylinderPrefab;
        }

        void UpdateEnqueueCount(float count)
        {
            enqueueCount = (int)count;

            if (SpawnText == null) return;

            SpawnText.text = "Spawn " + enqueueCount;

            if (enqueueCount == 1) SpawnText.text += " Entity";
            else SpawnText.text += " Entities";
        }

        void TogglePrefab() {
            if (currentPrefab.Equals(dinosaurPrefab)) {
                currentPrefab = cylinderPrefab;
                PrefabText.text = "Spawning Cylinders";
            } else {
                currentPrefab = dinosaurPrefab;
                PrefabText.text = "Spawning Dinosaurs";
            }
        }

        void Enqueue()
        {
            var entities = new NativeArray<Entity>(enqueueCount, Allocator.Temp);

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

                entityManager.AddComponentData(entities[i], new Translation
                {
                    Value = SpawnOffset
                });

                entityManager.AddComponent<LocalToWorld>(entities[i]);
                entityManager.AddComponent<Parent>(entities[i]);
                entityManager.AddComponent<LocalToParent>(entities[i]);
                entityManager.AddComponent<NavNeedsSurface>(entities[i]);
                entityManager.AddComponent<NavTerrainCapable>(entities[i]);
            }

            entities.Dispose();
        }
    }
}
