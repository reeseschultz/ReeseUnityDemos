using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ReeseUnityDemos
{
    class ProjectileDemoSpawner : MonoBehaviour
    {
        void Start()
        {
            PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Translation = new Translation
                {
                    Value = new float3(0, 0, 0)
                }
            });

            PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Translation = new Translation
                {
                    Value = new float3(5, 0, 0)
                }
            });

            PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Translation = new Translation
                {
                    Value = new float3(-5, 0, 0)
                }
            });
        }
    }
}
