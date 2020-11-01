using Reese.Nav;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;

namespace Reese.Demo
{
    class NavTerrainSpawner : MonoBehaviour
    {
        [SerializeField]
        Button SpawnButton = null;

        [SerializeField]
        Button PrefabButton = null;

        [SerializeField]
        Text SpawnText = null;

        [SerializeField]
        Text PrefabText = null;

        [SerializeField]
        Slider Slider = null;

        [SerializeField]
        float3 SpawnOffset = new float3(0, 1, 0);

        int enqueueCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity cylinderPrefab;
        Entity dinosaurPrefab;
        Entity currentPrefab;

        void Start()
        {
            if (SpawnButton == null || Slider == null) return;

            SpawnButton.onClick.AddListener(Enqueue);
            PrefabButton.onClick.AddListener(TogglePrefab);
            Slider.onValueChanged.AddListener(UpdateEnqueueCount);

            currentPrefab = cylinderPrefab = entityManager.CreateEntityQuery(typeof(CylinderPrefab)).GetSingleton<CylinderPrefab>().Value;
            dinosaurPrefab = entityManager.CreateEntityQuery(typeof(DinosaurPrefab)).GetSingleton<DinosaurPrefab>().Value;
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
            var outputEntities = new NativeArray<Entity>(enqueueCount, Allocator.Temp);

            entityManager.Instantiate(currentPrefab, outputEntities);

            for (var i = 0; i < outputEntities.Length; ++i)
            {
                entityManager.AddComponentData(outputEntities[i], new NavAgent
                {
                    TranslationSpeed = 20,
                    RotationSpeed = 0.3f,
                    TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                    Offset = new float3(0, 1, 0)
                });

                entityManager.AddComponentData(outputEntities[i], new Translation
                {
                    Value = SpawnOffset
                });

                entityManager.AddComponent<LocalToWorld>(outputEntities[i]);
                entityManager.AddComponent<Parent>(outputEntities[i]);
                entityManager.AddComponent<LocalToParent>(outputEntities[i]);
                entityManager.AddComponent<NavNeedsSurface>(outputEntities[i]);
                entityManager.AddComponent<NavTerrainCapable>(outputEntities[i]);
            }

            outputEntities.Dispose();
        }
    }
}
