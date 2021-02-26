using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is following an entity.</summary>
    [Serializable]
    public struct NavFollow : IComponentData
    {
        /// <summary>The target entity that this entity will follow.</summary>
        public Entity Target;

        /// <summary>Maximum distance before this agent will stop following the target entity. If less than or equal to zero, this agent will follow the target entity no matter how far it is away.</summary>
        public float MaxDistance;

        /// <summary>Minimum distance this agent maintains between itself and the target entity it follows.</summary>
        public float MinDistance;
    }
}
