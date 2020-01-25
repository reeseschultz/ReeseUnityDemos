using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>For authoring a NavAgent prefab.</summary>
    struct NavAgentPrefab : IComponentData
    {
        /// <summary>A reference to the NavAgent prefab as an Entity.</summary>
        public Entity Value;
    }
}