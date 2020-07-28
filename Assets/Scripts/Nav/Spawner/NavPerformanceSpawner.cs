using Reese.Nav;
using Reese.Spawning;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;

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
            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(currentPrefab)
                .WithComponentList(
                    new NavAgent
                    {
                        JumpDegrees = 45,
                        JumpGravity = 200,
                        TranslationSpeed = 20,
                        TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                        Offset = new float3(0, 1, 0)
                    },
                    new Parent { },
                    new LocalToParent { },
                    new LocalToWorld
                    {
                        Value = float4x4.TRS(
                            SpawnOffset,
                            quaternion.identity,
                            1
                        )
                    },
                    new Translation
                    {
                        Value = SpawnOffset
                    },
                    new NavNeedsSurface { }
                ),
                enqueueCount
            );
        }
    }
}
