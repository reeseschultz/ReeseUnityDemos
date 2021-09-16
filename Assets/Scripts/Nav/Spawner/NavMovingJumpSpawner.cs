using Reese.EntityPrefabGroups;
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
        EntityPrefabSystem prefabSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityPrefabSystem>();

        void Start()
        {
            var entities = new NativeArray<Entity>(50, Allocator.Temp);
            var prefab = prefabSystem.GetPrefab(typeof(Dinosaur));

            prefabSystem.EntityManager.Instantiate(prefab, entities);

            for (var i = 0; i < entities.Length; ++i)
            {
                prefabSystem.EntityManager.AddComponentData(entities[i], new NavAgent
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

                prefabSystem.EntityManager.AddComponentData<LocalToWorld>(entities[i], new LocalToWorld
                {
                    Value = float4x4.TRS(
                        new float3(0, 1, 0),
                        quaternion.identity,
                        1
                    )
                });

                prefabSystem.EntityManager.AddComponent<Parent>(entities[i]);
                prefabSystem.EntityManager.AddComponent<LocalToParent>(entities[i]);
                prefabSystem.EntityManager.AddComponent<NavNeedsSurface>(entities[i]);
            }

            entities.Dispose();
        }
    }
}
