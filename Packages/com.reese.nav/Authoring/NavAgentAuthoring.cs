using Unity.Entities;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Authors a NavAgent.</summary>
    public class NavAgentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>The agent's jump angle in degrees.</summary>
        [SerializeField]
        float jumpDegrees = 45;

        /// <summary>Artificial gravity applied to the agent.</summary>
        [SerializeField]
        float jumpGravity = 100;

        /// <summary>The agent's horizontal jump speed multiplier.</summary>
        [SerializeField]
        float jumpSpeedMultiplierX = 1.5f;

        /// <summary>The agent's vertical jump speed mulitiplier.</summary>
        [SerializeField]
        float jumpSpeedMultiplierY = 2;

        /// <summary>The agent's translation speed.</summary>
        [SerializeField]
        float translationSpeed = 20;

        /// <summary>The agent's rotation speed.</summary>
        [SerializeField]
        float rotationSpeed = 0.3f;

        /// <summary>The agent's type.</summary>
        [SerializeField]
        string type = NavConstants.HUMANOID;

        /// <summary>The agent's offset.</summary>
        [SerializeField]
        Vector3 offset = default;

        /// <summary>True if the agent is terrain-capable, false if not.</summary>
        [SerializeField]
        bool isTerrainCapable = default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new NavAgent
            {
                JumpDegrees = jumpDegrees,
                JumpGravity = jumpGravity,
                JumpSpeedMultiplierX = jumpSpeedMultiplierX,
                JumpSpeedMultiplierY = jumpSpeedMultiplierY,
                TranslationSpeed = translationSpeed,
                RotationSpeed = rotationSpeed,
                TypeID = NavUtil.GetAgentType(type),
                Offset = offset
            });

            dstManager.AddComponent<NavNeedsSurface>(entity);
            dstManager.AddComponent<NavFixTranslation>(entity);

            if (isTerrainCapable) dstManager.AddComponent<NavTerrainCapable>(entity);
        }
    }
}
