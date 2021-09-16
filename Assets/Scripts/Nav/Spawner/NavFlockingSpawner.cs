using Reese.EntityPrefabGroups;
using Reese.Nav;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace Reese.Demo
{
    class NavFlockingSpawner : MonoBehaviour
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

        EntityPrefabSystem prefabSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityPrefabSystem>();

        Entity cylinderPrefab = default;
        Entity dinosaurPrefab = default;
        Entity currentPrefab = default;

        void Start()
        {
            if (SpawnButton == null || Slider == null) return;

            SpawnButton.onClick.AddListener(Spawn);
            PrefabButton.onClick.AddListener(TogglePrefab);
            Slider.onValueChanged.AddListener(UpdateSpawnCount);

            cylinderPrefab = prefabSystem.GetPrefab(typeof(Cylinder));
            dinosaurPrefab = prefabSystem.GetPrefab(typeof(Dinosaur));

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
            var outputEntities = new NativeArray<Entity>(spawnCount, Allocator.Temp);

            prefabSystem.EntityManager.Instantiate(currentPrefab, outputEntities);

            for (var i = 0; i < outputEntities.Length; ++i)
            {
                prefabSystem.EntityManager.AddComponentData(outputEntities[i], new NavAgent
                {
                    TranslationSpeed = 20,
                    ObstacleAversionDistance = 4f,
                    AgentAversionDistance = 3f,
                    SeparationPerceptionRadius = 1f,
                    AlignmentPerceptionRadius = currentPrefab.Equals(cylinderPrefab) ? 1f : 3f,
                    CohesionPerceptionRadius = currentPrefab.Equals(cylinderPrefab) ? 1f : 3f,
                    RotationSpeed = 0.3f,
                    TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                    Offset = new float3(0, 1, 0)
                });

                prefabSystem.EntityManager.AddComponentData<LocalToWorld>(outputEntities[i], new LocalToWorld
                {
                    Value = float4x4.TRS(
                        new float3(0, 1, 0),
                        quaternion.identity,
                        1
                    )
                });

                prefabSystem.EntityManager.AddComponent<Parent>(outputEntities[i]);
                prefabSystem.EntityManager.AddComponent<LocalToParent>(outputEntities[i]);
                prefabSystem.EntityManager.AddComponent<NavNeedsSurface>(outputEntities[i]);
                prefabSystem.EntityManager.AddComponent<NavFlocking>(outputEntities[i]);
                prefabSystem.EntityManager.AddComponent<NavObstacleSteering>(outputEntities[i]);
            }

            outputEntities.Dispose();
        }
    }
}
