using UnityEngine;
using Unity.Mathematics;
using Reese.Nav;
using Unity.Entities;
using Reese.Spawning;
using Unity.Transforms;

namespace Reese.Demo
{
    class NavPointAndClickDemoSpawner : MonoBehaviour
    {
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            var prefabEntity = entityManager.CreateEntityQuery(typeof(NavAgentPrefab)).GetSingleton<NavAgentPrefab>().Value;

            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(prefabEntity)
                .WithComponentList(
                    new NavAgent
                    {
                        JumpDegrees = 45,
                        JumpGravity = 100,
                        JumpSpeedMultiplierX = 2,
                        JumpSpeedMultiplierY = 4,
                        TranslationSpeed = 40,
                        TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                        Offset = new float3(0, 1, 0)
                    },
                    new NavNeedsSurface { },
                    new Translation
                    {
                        Value = new float3(0, 1, 0)
                    }
                )
            );
        }
    }
}
