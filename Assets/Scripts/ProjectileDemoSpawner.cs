using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    class ProjectileDemoSpawner : MonoBehaviour
    {
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            var outputEntities = new NativeArray<Entity>(3, Allocator.Temp);
            var prefabEntity = entityManager.CreateEntityQuery(typeof(PersonPrefab)).GetSingleton<PersonPrefab>().Value;

            entityManager.Instantiate(prefabEntity, outputEntities);

            for (var i = 0; i < outputEntities.Length; ++i)
            {
                entityManager.AddComponent<Translation>(outputEntities[i]);
                entityManager.AddComponent<Projectile>(outputEntities[i]);
            }

            outputEntities.Dispose();
        }
    }
}
