using Unity.Entities;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Authors a NavFlocking.</summary>
    public class NavFlockingAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            => dstManager.AddComponent<NavFlocking>(entity);
    }
}
