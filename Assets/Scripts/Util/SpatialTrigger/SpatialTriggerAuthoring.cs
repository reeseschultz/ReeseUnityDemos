using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a SpatialTrigger.</summary>
    public class SpatialTriggerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>A bit mask describing which layers this object belongs to.</summary>
        [SerializeField]
        uint belongsTo = default;

        /// <summary>A bit mask describing which layers this object can collide with.</summary>
        [SerializeField]
        uint collidesWith = default;

        /// <summary>An optional override for the bit mask checks. If the value in both objects is equal and positive, the objects always collide. If the value in both objects is equal and negative, the objects never collide.</summary>
        [SerializeField]
        int groupIndex = default;

        /// <summary>True if using the default collision filter, false if not.</summary>
        [SerializeField]
        bool useDefaultCollisionFilter = true;

        /// <summary>This trigger will be activated by any overlapping activators belonging to the same group.</summary>
        [SerializeField]
        List<string> groups = new List<string>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SpatialTrigger
            {
                Filter = useDefaultCollisionFilter ? CollisionFilter.Default : new CollisionFilter
                {
                    BelongsTo = belongsTo,
                    CollidesWith = collidesWith,
                    GroupIndex = groupIndex
                }
            });

            dstManager.AddComponent(entity, typeof(SpatialGroupBufferElement));

            var groupBuffer = dstManager.GetBuffer<SpatialGroupBufferElement>(entity);

            groups.Distinct().ToList().ForEach(group => groupBuffer.Add(group));
        }
    }
}
