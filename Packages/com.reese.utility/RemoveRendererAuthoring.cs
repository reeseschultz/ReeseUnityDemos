using Unity.Entities;
using UnityEngine;

namespace Reese.Utility
{
    /// <summary>Removes mesh renderer from injected GameObject in authoring.</summary>
    public class RemoveRendererAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var renderer = GetComponent<Renderer>();

            if (renderer == null) return;

            Destroy(renderer);
        }
    }
}
