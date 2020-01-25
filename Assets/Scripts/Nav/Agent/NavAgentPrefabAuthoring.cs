using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Reese.Nav
{
    ///<summary>Authors a prefab for NavAgents.</summary>
    [RequiresEntityConversion]
    public class NavAgentPrefabAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        /// <summary>A reference to the NavAgent prefab GameObject.</summary>
        public GameObject NavAgentPrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            => dstManager.AddComponentData(entity, new NavAgentPrefab
            {
                Value = conversionSystem.GetPrimaryEntity(NavAgentPrefab)
            });

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
            => referencedPrefabs.Add(NavAgentPrefab);
    }
}
