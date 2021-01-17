using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.ConvertToEntity;

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

            var convertToEntity = GetComponent<ConvertToEntity>();
            if (convertToEntity != null && convertToEntity.ConversionMode.Equals(Mode.ConvertAndInjectGameObject))
            {
                dstManager.AddComponent(entity, typeof(CopyTransformToGameObject));

                var renderer = GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }

            dstManager.AddComponent<NavFixTranslation>(entity); // TODO : Transform extensions package.
        }
    }
}
