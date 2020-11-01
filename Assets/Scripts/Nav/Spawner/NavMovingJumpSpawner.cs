using UnityEngine;
using Unity.Mathematics;
using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

namespace Reese.Demo
{
    class NavMovingJumpSpawner : MonoBehaviour
    {
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            var outputEntities = new NativeArray<Entity>(50, Allocator.Temp);
            var prefabEntity = entityManager.CreateEntityQuery(typeof(DinosaurPrefab)).GetSingleton<DinosaurPrefab>().Value;

            entityManager.Instantiate(prefabEntity, outputEntities);

            for (var i = 0; i < outputEntities.Length; ++i)
            {
                entityManager.AddComponentData(outputEntities[i], new NavAgent
                {
                    JumpDegrees = 45,
                    JumpGravity = 100,
                    JumpSpeedMultiplierX = 1.5f,
                    JumpSpeedMultiplierY = 2,
                    TranslationSpeed = 20,
                    RotationSpeed = 0.3f,
                    TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                    Offset = new float3(0, 1, 0)
                });

                entityManager.AddComponentData<LocalToWorld>(outputEntities[i], new LocalToWorld
                {
                    Value = float4x4.TRS(
                        new float3(0, 1, 0),
                        quaternion.identity,
                        1
                    )
                });

                entityManager.AddComponent<Parent>(outputEntities[i]);
                entityManager.AddComponent<LocalToParent>(outputEntities[i]);
                entityManager.AddComponent<NavNeedsSurface>(outputEntities[i]);
            }

            outputEntities.Dispose();
        }
    }
}
