using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a child.</summary>
    public class ChildAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        ParentAuthoring parent = default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Parent
            {
                Value = conversionSystem.GetPrimaryEntity(parent)
            });

            dstManager.AddComponent<NavFixTranslation>(entity); // TODO : Transform extensions package.
        }
    }
}
