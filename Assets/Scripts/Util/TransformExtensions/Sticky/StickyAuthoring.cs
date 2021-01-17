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
        [SerializeField]
        Vector3 worldDirection = Vector3.down;

        [SerializeField]
        uint belongsTo = default;

        [SerializeField]
        uint collidesWith = default;

        [SerializeField]
        int groupIndex = default;

        [SerializeField]
        bool useDefaultCollisionFilter = default;

        [SerializeField]
        float radius = default;

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
