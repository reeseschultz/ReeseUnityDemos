using Reese.EntityPrefabGroups;
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
            var prefab = entityManager.GetPrefab<Person>();

            var entities = new NativeArray<Entity>(3, Allocator.Temp);
            entityManager.Instantiate(prefab, entities);

            for (var i = 0; i < entities.Length; ++i)
            {
                entityManager.AddComponent<Translation>(entities[i]);
                entityManager.AddComponent<Projectile>(entities[i]);
            }

            entities.Dispose();
        }
    }
}
