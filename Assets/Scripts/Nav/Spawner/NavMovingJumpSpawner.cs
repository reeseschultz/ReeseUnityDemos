using Reese.Nav;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    class NavMovingJumpSpawner : MonoBehaviour
    {
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            var entities = new NativeArray<Entity>(50, Allocator.Temp);
            var prefab = entityManager.CreateEntityQuery(typeof(Dinosaur), typeof(Prefab)).GetSingletonEntity();

            entityManager.Instantiate(prefab, entities);

            for (var i = 0; i < entities.Length; ++i)
            {
                entityManager.AddComponentData(entities[i], new NavAgent
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

                entityManager.AddComponentData<LocalToWorld>(entities[i], new LocalToWorld
                {
                    Value = float4x4.TRS(
                        new float3(0, 1, 0),
                        quaternion.identity,
                        1
                    )
                });

                entityManager.AddComponent<Parent>(entities[i]);
                entityManager.AddComponent<LocalToParent>(entities[i]);
                entityManager.AddComponent<NavNeedsSurface>(entities[i]);
            }

            entities.Dispose();
        }
    }
}
