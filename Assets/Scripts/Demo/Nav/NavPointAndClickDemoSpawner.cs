using UnityEngine;
using Unity.Mathematics;
using Reese.Nav;

namespace Reese.Demo
{
    class NavPointAndClickDemoSpawner : MonoBehaviour
    {
        void Start()
        {
            NavSpawnSystem.Enqueue(new NavAgentSpawn
            {
                Agent = new NavAgent
                {
                    JumpDegrees = 45,
                    JumpGravity = 200,
                    TranslationSpeed = 20,
                    TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                    Offset = new float3(0, 1, 0),
                },
                Translation = new Unity.Transforms.Translation
                {
                    Value = new float3(0, 1, 0)
                }
            });
        }
    }
}
