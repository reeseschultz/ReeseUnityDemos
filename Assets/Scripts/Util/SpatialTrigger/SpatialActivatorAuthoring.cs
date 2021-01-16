using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialActivator.</summary>
    public class SpatialActivatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>The center of the activator's bounds.</summary>
        [SerializeField]
        Vector3 center = Vector3.zero;

        /// <summary>The scale of the activator's bounds.</summary>
        [SerializeField]
        Vector3 scale = Vector3.one;

        /// <summary>This activator will activate all triggers belonging to the same group.</summary>
        [SerializeField]
        List<string> groups = new List<string>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var renderer = (Renderer)GetComponentsInChildren(typeof(Renderer))[0];

            dstManager.AddComponentData<SpatialActivator>(entity, new SpatialActivator 
            {
                Bounds = new AABB
                {
                    Center = center,
                    Extents = Vector3.Scale(renderer.bounds.extents, scale)
                }
            });

            dstManager.AddComponent(entity, typeof(SpatialGroupBufferElement));

            var groupBuffer = dstManager.GetBuffer<SpatialGroupBufferElement>(entity);

            groups.Distinct().ToList().ForEach(group => groupBuffer.Add(group)); 
        }
    }
}
