using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.ConvertToEntity;

namespace Reese.Demo
{
    /// <summary>Authors a sticky.</summary>
    public class StickyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>World direction in which the object should attempt to stick to another.</summary>
        [SerializeField]
        Vector3 worldDirection = Vector3.down;

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

        /// <summary>Radius of raycasting SphereGeometry used to stick this object to another.</summary>
        [SerializeField]
        float radius = default;

        /// <summary>Number of attempts the StickySystem has to stick the object. The StickyFailed component will be added to it in case of failure.</summary>
        [SerializeField]
        int stickAttempts = 10;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var filter = new CollisionFilter
            {
                BelongsTo = belongsTo,
                CollidesWith = collidesWith,
                GroupIndex = groupIndex
            };

            if (useDefaultCollisionFilter) filter = CollisionFilter.Default;

            dstManager.AddComponentData(entity, new Sticky
            {
                Filter = filter,
                WorldDirection = worldDirection,
                Radius = radius,
                StickAttempts = stickAttempts
            });

            var convertToEntity = GetComponent<ConvertToEntity>();

            if (
                convertToEntity != null &&
                convertToEntity.ConversionMode.Equals(Mode.ConvertAndInjectGameObject)
            ) dstManager.AddComponent(entity, typeof(CopyTransformToGameObject));

            dstManager.AddComponent<FixTranslation>(entity);
        }
    }
}
