using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.ConvertToEntity;

namespace Reese.Utility
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

            var convertToEntity = GetComponent<ConvertToEntity>();

            if (
                convertToEntity != null &&
                convertToEntity.ConversionMode.Equals(Mode.ConvertAndInjectGameObject)
            ) dstManager.AddComponent(entity, typeof(CopyTransformToGameObject));

            dstManager.AddComponent<FixTranslation>(entity);
        }
    }
}
