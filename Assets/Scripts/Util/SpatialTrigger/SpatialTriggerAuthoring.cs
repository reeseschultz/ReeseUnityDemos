using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialTrigger.</summary>
    public class SpatialTriggerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>This trigger will be activated by any overlapping activators belonging to the same group.</summary>
        [SerializeField]
        List<string> groups = new List<string>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent(entity, typeof(SpatialTrigger));

            dstManager.AddComponent(entity, typeof(SpatialGroupBufferElement));

            var groupBuffer = dstManager.GetBuffer<SpatialGroupBufferElement>(entity);

            groups.Distinct().ToList().ForEach(group => groupBuffer.Add(group));
        }
    }
}
