using UnityEngine;
using Unity.Mathematics;
using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;

namespace Reese.Demo
{
    class NavPointAndClickSpawner : MonoBehaviour
    {
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            var prefabEntity = entityManager.CreateEntityQuery(typeof(DinosaurPrefab)).GetSingleton<DinosaurPrefab>().Value;
            var entity = entityManager.Instantiate(prefabEntity);

            entityManager.AddComponentData(entity, new NavAgent
            {
                JumpDegrees = 45,
                JumpGravity = 100,
                JumpSpeedMultiplierX = 2,
                JumpSpeedMultiplierY = 4,
                TranslationSpeed = 40,
                RotationSpeed = 0.3f,
                TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                Offset = new float3(0, 1, 0)
            });

            entityManager.AddComponentData<LocalToWorld>(entity, new LocalToWorld 
            {
                Value = float4x4.TRS(
                    new float3(0, 1, 0),
                    quaternion.identity,
                    1
                )
            });

            entityManager.AddComponent<Parent>(entity);
            entityManager.AddComponent<LocalToParent>(entity);
            entityManager.AddComponent<NavNeedsSurface>(entity);
        }
    }
}
