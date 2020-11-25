using Unity.Entities;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialActivator.</summary>
    public class SpatialActivatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent(entity, typeof(SpatialActivator));
            dstManager.AddComponent(entity, typeof(SpatialTriggerBufferElement));
        }
    }
}
