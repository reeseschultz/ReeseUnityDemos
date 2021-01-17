using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    /// <summary>Authors a parent.</summary>
    public class ParentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (dstManager.HasComponent<Parent>(entity)) return;

            dstManager.AddComponent<Parent>(entity);
        }
    }
}
