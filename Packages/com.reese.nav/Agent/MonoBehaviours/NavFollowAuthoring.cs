using Unity.Entities;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Authors a NavFollow.</summary>
    public class NavFollowAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        GameObject target = default;

        [SerializeField]
        float maxDistance = default;

        [SerializeField]
        float minDistance = default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new NavFollow
            {
                Target = conversionSystem.GetPrimaryEntity(target),
                MaxDistance = maxDistance,
                MinDistance = minDistance
            });
        }
    }
}
