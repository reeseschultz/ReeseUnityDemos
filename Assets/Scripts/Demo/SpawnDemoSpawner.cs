using Reese.Nav;
using Reese.Spawning;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;

namespace Reese.Demo
{
    class SpawnDemoSpawner : MonoBehaviour
    {
        public bool IsForAgents;
        public Button Button;
        public Text SpawnText;
        public Slider Slider;

        int enqueueCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity prefabEntity;

        void Start()
        {
            if (Button == null || Slider == null) return;

            Button.onClick.AddListener(Enqueue);
            Slider.onValueChanged.AddListener(UpdateEnqueueCount);

            if (!IsForAgents) prefabEntity = entityManager.CreateEntityQuery(typeof(PersonPrefab)).GetSingleton<PersonPrefab>().Value;
            else prefabEntity = entityManager.CreateEntityQuery(typeof(NavAgentPrefab)).GetSingleton<NavAgentPrefab>().Value;
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
            if (!IsForAgents) {
                var random = new Unity.Mathematics.Random((uint)new System.Random().Next());

                for (int i = 0; i < enqueueCount; ++i) SpawnSystem.Enqueue(new Spawn()
                    .WithPrefab(prefabEntity)
                    .WithComponentList(
                        new Translation
                        {
                            Value = new float3(
                                random.NextInt(-25, 25),
                                2,
                                random.NextInt(-25, 25)
                            )
                        }
                    )
                );

                return;
            }

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
                    new Parent { },
                    new LocalToParent { },
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
