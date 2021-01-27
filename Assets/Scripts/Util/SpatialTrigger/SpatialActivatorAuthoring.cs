using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialActivator.</summary>
    public class SpatialActivatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>This activator will activate any overlapping triggers belonging to the same group.</summary>
        [SerializeField]
        List<string> groups = new List<string>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<SpatialActivator>(entity);

            dstManager.AddComponent(entity, typeof(SpatialGroupBufferElement));

            var groupBuffer = dstManager.GetBuffer<SpatialGroupBufferElement>(entity);

            groups.Distinct().ToList().ForEach(group => groupBuffer.Add(group)); 
        }
    }
}
