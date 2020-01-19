using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    class PointAndClickDemoSpawner : MonoBehaviour
    {
        void Start()
        {
            PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Translation = new Translation
                {
                    Value = new float3(0, 1, 0)
                }
            });

            PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Translation = new Translation
                {
                    Value = new float3(5, 1, 0)
                }
            });

            PersonSpawnSystem.Enqueue(new PersonSpawn
            {
                Translation = new Translation
                {
                    Value = new float3(-5, 1, 0)
                }
            });
        }
    }
}
