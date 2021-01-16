using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialTrigger.</summary>
    public class SpatialTriggerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>True if the trigger tracks activator entry events, false if not.</summary>
        [SerializeField]
        bool trackEntries = false;

        /// <summary>True if the trigger tracks activator exit events, false if not.</summary>
        [SerializeField]
        bool trackExits = false;

        /// <summary>The center of the trigger's bounds.</summary>
        [SerializeField]
        Vector3 center = Vector3.zero;

        /// <summary>The scale of the trigger's bounds.</summary>
        [SerializeField]
        Vector3 scale = Vector3.one;

        /// <summary>This trigger will be activated by all activators belonging to the same group.</summary>
        [SerializeField]
        List<string> groups = new List<string>();

        void OnDrawGizmosSelected()
        {
            var renderer = GetComponent<Renderer>();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center + renderer.bounds.center, Vector3.Scale(renderer.bounds.size, scale));
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var renderer = (Renderer)GetComponentsInChildren(typeof(Renderer))[0];

            dstManager.AddComponentData<SpatialTrigger>(entity, new SpatialTrigger
            {
                Bounds = new AABB
                {
                    Center = center,
                    Extents = Vector3.Scale(renderer.bounds.extents, scale)
                },
                TrackEntries = trackEntries,
                TrackExits = trackExits
            });

            dstManager.AddComponent(entity, typeof(SpatialGroupBufferElement));

            var groupBuffer = dstManager.GetBuffer<SpatialGroupBufferElement>(entity);

            groups.Distinct().ToList().ForEach(group => groupBuffer.Add(group));
        }
    }
}
