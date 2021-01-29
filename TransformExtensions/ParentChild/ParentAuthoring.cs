using Unity.Entities;
using UnityEngine;

namespace Reese.Utility
{
    /// <summary>Authors a parent.</summary>
    public class ParentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) { }
    }
}
