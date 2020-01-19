using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ReeseUnityDemos
{
    [RequiresEntityConversion]
    class PersonPrefabAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject PersonPrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new PersonPrefab
            {
                Value = conversionSystem.GetPrimaryEntity(PersonPrefab)
            });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(PersonPrefab);
        }
    }
}
