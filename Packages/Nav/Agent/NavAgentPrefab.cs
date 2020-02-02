using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>For authoring a NavAgent prefab.</summary>
    public struct NavAgentPrefab : IComponentData
    {
        /// <summary>A reference to the NavAgent prefab as an Entity.</summary>
        public Entity Value;
    }
}