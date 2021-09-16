using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Reese.EntityPrefabGroups
{
    /// <summary>Initializes a group of entity prefabs.</summary>
    [RequireComponent(typeof(ConvertToEntity))]
    [DisallowMultipleComponent]
    public class EntityPrefabGroup : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        [SerializeField]
        GameObject[] prefabs = default;

        EntityPrefabSystem prefabSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityPrefabSystem>();

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            foreach (var prefab in prefabs) referencedPrefabs.Add(prefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!prefabSystem.Initialized) prefabSystem.Initialize();

            var prefabEntities = new NativeArray<Entity>(prefabs.Length, Allocator.TempJob);
            for (var i = 0; i < prefabs.Length; ++i)
                prefabEntities[i] = conversionSystem.TryGetPrimaryEntity(prefabs[i]);

            var groupBuffer = dstManager.AddBuffer<PrefabGroup>(entity);
            groupBuffer.AddRange(prefabEntities.Reinterpret<PrefabGroup>());

            prefabSystem.AddGroup(entity);
            prefabSystem.AddPrefabs(prefabEntities);

            prefabEntities.Dispose();
        }
    }
}
