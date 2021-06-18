using Unity.Entities;
using UnityEngine;

namespace Reese.Path
{
    /// <summary>Authors a PathFollow.</summary>
    public class PathFollowAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        GameObject target = default;

        [SerializeField]
        float maxDistance = default;

        [SerializeField]
        float minDistance = default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new PathFollow
            {
                Target = conversionSystem.GetPrimaryEntity(target),
                MaxDistance = maxDistance,
                MinDistance = minDistance
            });
        }
    }
}
