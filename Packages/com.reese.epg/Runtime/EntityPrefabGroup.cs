using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reese.EntityPrefabGroups
{
    /// <summary>Initializes a group of entity prefabs.</summary>
    [AddComponentMenu("Reese/Entity Prefab Group")]
    [RequireComponent(typeof(ConvertToEntity))]
    [DisallowMultipleComponent]
    public class EntityPrefabGroup : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        [SerializeField]
        internal bool generatePrefabHelperClasses = default;

        [SerializeField]
        internal bool generateGroupHelperClasses = default;

        [SerializeField]
        internal List<GameObject> Prefabs = default;

        EntityPrefabSystem prefabSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityPrefabSystem>();

#if UNITY_EDITOR
        [MenuItem("Reese/Entity Prefab Group/Regenerate Prefab Helper Classes for Current Scene")]
        static void RegeneratePrefabHelpersForCurrentScene()
        {
            var groups = EntityPrefabUtility.GetEntityPrefabGroups();
            var groupNames = EntityPrefabUtility.GetGroupNames(true, groups).ToList();
            EntityPrefabUtility.RegeneratePrefabHelperClasses(groupNames, groups);
        }

        [MenuItem("Reese/Entity Prefab Group/Clear Prefab Helper Classes for All Scenes")]
        static void ClearPrefabHelpersForAllScenes()
        {
            var path = Path.Combine(Application.dataPath, EntityPrefabUtility.PACKAGE_DIRECTORY_NAME);

            if (!Directory.Exists(path)) return;

            EntityPrefabUtility.ClearFilesInDirectory(path);
        }
#endif

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            if (!prefabSystem.Initialized) prefabSystem.Initialize(Prefabs.Count);

            foreach (var prefab in Prefabs) referencedPrefabs.Add(prefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!prefabSystem.Initialized) prefabSystem.Initialize(Prefabs.Count);

            foreach (var prefab in Prefabs)
            {
                var prefabEntity = conversionSystem.GetPrimaryEntity(prefab);

                if (prefabEntity == Entity.Null) continue;

                if (generatePrefabHelperClasses) prefabSystem.AddPrefab(prefab.name, prefabEntity);

                if (generateGroupHelperClasses) prefabSystem.AddGroup(gameObject.name, prefabEntity);
            }
        }
    }
}
