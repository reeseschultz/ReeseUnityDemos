using Unity.Entities;
using UnityEngine;

namespace Reese.Path
{
    /// <summary>Authors an agent.</summary>
    public class PathAgentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>The agent's type.</summary>
        [SerializeField]
        string type = PathConstants.HUMANOID;

        /// <summary>The agent's offset.</summary>
        [SerializeField]
        Vector3 offset = default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new PathAgent
            {
                TypeID = PathUtil.GetAgentType(type),
                Offset = offset
            });
        }
    }
}
