using Unity.Entities;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialTriggerActivator.</summary>
    public class SpatialTriggerActivatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent(entity, typeof(SpatialTriggerActivator));
            dstManager.AddComponent(entity, typeof(SpatialTriggerBufferElement));
        }
    }
}
