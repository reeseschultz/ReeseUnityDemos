using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialTrigger.</summary>
    public class SpatialTriggerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        Vector3 center = Vector3.zero;

        [SerializeField]
        Vector3 scale = Vector3.one;

        [SerializeField]
        List<SpatialTriggerActivatorAuthoring> activators = new List<SpatialTriggerActivatorAuthoring>();

        void OnDrawGizmosSelected()
        {
            var renderer = GetComponent<Renderer>();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center + renderer.bounds.center, Vector3.Scale(renderer.bounds.size, scale));
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var renderer = GetComponent<Renderer>();

            dstManager.AddComponentData<SpatialTrigger>(entity, new SpatialTrigger
            {
                Bounds = new AABB
                {
                    Center = center,
                    Extents = Vector3.Scale(renderer.bounds.extents, scale)
                }
            });

            dstManager.AddComponent(entity, typeof(SpatialTriggerActivatorBufferElement));
            var activatorBuffer = dstManager.GetBuffer<SpatialTriggerActivatorBufferElement>(entity);
            activators.ForEach(activator => activatorBuffer.Add(conversionSystem.GetPrimaryEntity(activator)));
        }
    }
}
