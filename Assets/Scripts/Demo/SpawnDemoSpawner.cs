using Reese.Nav;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Demo
{
    class SpawnDemoSpawner : MonoBehaviour
    {
        public bool IsForAgents;
        public Button Button;
        public Text SpawnText;
        public Slider Slider;

        int enqueueCount = 1;

        void Start()
        {
            if (Button == null || Slider == null) return;

            Button.onClick.AddListener(enqueue);
            Slider.onValueChanged.AddListener(updateEnqueueCount);
        }

        void updateEnqueueCount(float count)
        {
            enqueueCount = (int)count;

            if (SpawnText == null) return;

            SpawnText.text = "Spawn " + enqueueCount;

            if (enqueueCount == 1) SpawnText.text += " Entity";
            else SpawnText.text += " Entities";
        }

        void enqueue()
        {
            if (Button == null) return;

            if (!IsForAgents) PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Person = new Person
                {
                    RandomizeTranslation = true
                }
            }, enqueueCount);
            else NavSpawnSystem.Enqueue(new NavAgentSpawn
            {
               Agent = new NavAgent
                {
                    JumpDegrees = 45,
                    JumpGravity = 200,
                    TranslationSpeed = 20,
                    TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                    Offset = new float3(0, 1, 0)
                },
                Translation = new Translation
                {
                    Value = new float3(0, 1, 0)
                } 
            }, enqueueCount);
        }
    }
}
