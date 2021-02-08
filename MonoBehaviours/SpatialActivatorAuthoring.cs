using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Reese.Spatial
{
    /// <summary>Authors a SpatialActivator.</summary>
    public class SpatialActivatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>This activator will activate any overlapping triggers belonging to the same tag.</summary>
        [SerializeField]
        List<string> tags = new List<string>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<SpatialActivator>(entity);

            dstManager.AddComponent(entity, typeof(SpatialTag));

            var tagBuffer = dstManager.GetBuffer<SpatialTag>(entity);

            tags.Distinct().ToList().ForEach(group => tagBuffer.Add(group)); 
        }
    }
}
