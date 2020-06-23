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
        Button Button = null;

        [SerializeField]
        Text SpawnText = null;

        [SerializeField]
        Slider Slider = null;

        int enqueueCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity prefabEntity;

        void Start()
        {
            if (Button == null || Slider == null) return;

            Button.onClick.AddListener(Enqueue);
            Slider.onValueChanged.AddListener(UpdateEnqueueCount);

            prefabEntity = entityManager.CreateEntityQuery(typeof(NavAgentPrefab)).GetSingleton<NavAgentPrefab>().Value;
        }

        void UpdateEnqueueCount(float count)
        {
            enqueueCount = (int)count;

            if (SpawnText == null) return;

            SpawnText.text = "Spawn " + enqueueCount;

            if (enqueueCount == 1) SpawnText.text += " Entity";
            else SpawnText.text += " Entities";
        }

        void Enqueue()
        {
            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(prefabEntity)
                .WithComponentList(
                    new NavAgent
                    {
                        JumpDegrees = 45,
                        JumpGravity = 200,
                        TranslationSpeed = 20,
                        TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                        Offset = new float3(0, 1, 0)
                    },
                    new NavNeedsSurface { },
                    new Translation
                    {
                        Value = new float3(0, 1, 0)
                    }
                ),
                enqueueCount
            );
        }
    }
}
