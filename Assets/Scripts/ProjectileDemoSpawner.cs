using Reese.EntityPrefabGroups;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    class ProjectileDemoSpawner : MonoBehaviour
    {
        EntityPrefabSystem prefabSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityPrefabSystem>();

        void Start()
        {
            var prefab = prefabSystem.GetPrefab(typeof(Person));

            var entities = new NativeArray<Entity>(3, Allocator.Temp);
            prefabSystem.EntityManager.Instantiate(prefab, entities);

            for (var i = 0; i < entities.Length; ++i)
            {
                prefabSystem.EntityManager.AddComponent<Translation>(entities[i]);
                prefabSystem.EntityManager.AddComponent<Projectile>(entities[i]);
            }

            entities.Dispose();
        }
    }
}
