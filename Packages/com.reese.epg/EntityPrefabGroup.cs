using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        [Tooltip("If true, collects prefabs referenced from other authoring scripts (attached to this GameObject) into the group.")]
        bool collectOtherPrefabs = true;

        [SerializeField]
        GameObject[] prefabs = default;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            foreach (var prefab in prefabs) referencedPrefabs.Add(prefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var prefabsDeduped = new HashSet<Entity>();

            foreach (var prefab in prefabs)
            {
                var prefabEntity = conversionSystem.GetPrimaryEntity(prefab);

                if (prefabEntity == Entity.Null) continue;

                prefabsDeduped.Add(prefabEntity);
            }

            if (collectOtherPrefabs)
            {
                var otherPrefabEntities = GetOtherAuthoringPrefabEntities(conversionSystem);

                foreach (var otherPrefab in otherPrefabEntities)
                    prefabsDeduped.Add(otherPrefab);
            }

            var groupBuffer = dstManager.AddBuffer<PrefabGroup>(entity);
            foreach (var prefabEntity in prefabsDeduped)
                groupBuffer.Add(prefabEntity);
        }

        List<Entity> GetOtherAuthoringPrefabEntities(GameObjectConversionSystem conversionSystem)
        {
            var referencedPrefabsArray = GetComponents<IDeclareReferencedPrefabs>();

            var entities = new List<Entity>();
            foreach (var referencedPrefabs in referencedPrefabsArray)
            {
                var fields = referencedPrefabs.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                    .ToArray();

                foreach (var field in fields)
                {
                    var go = field.GetValue(referencedPrefabs) as GameObject;

                    if (go == null) continue;

                    var entity = conversionSystem.GetPrimaryEntity(go);

                    if (entity == Entity.Null) continue;

                    entities.Add(entity);
                }
            }

            return entities;
        }
    }
}
