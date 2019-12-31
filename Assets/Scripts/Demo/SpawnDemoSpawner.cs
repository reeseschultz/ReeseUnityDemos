using UnityEngine;
using UnityEngine.UI;

namespace ReeseUnityDemos
{
    class SpawnDemoSpawner : MonoBehaviour
    {
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

            if (enqueueCount == 1) SpawnText.text += " Person";
            else SpawnText.text += " People";
        }

        void enqueue()
        {
            if (Button == null) return;

            PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Person = new Person
                {
                    RandomizeTranslation = true
                }
            }, enqueueCount);
        }
    }
}
