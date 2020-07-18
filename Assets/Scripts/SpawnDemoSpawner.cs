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
        [SerializeField]
        Button Button = default;

        [SerializeField]
        Text SpawnText = default;

        [SerializeField]
        Slider Slider = default;

        int enqueueCount = 1;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity prefabEntity;

        void Start()
        {
            if (Button == null || Slider == null) return;

            Button.onClick.AddListener(Enqueue);
            Slider.onValueChanged.AddListener(UpdateEnqueueCount);

            prefabEntity = entityManager.CreateEntityQuery(typeof(PersonPrefab)).GetSingleton<PersonPrefab>().Value;
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
        }
    }
}
