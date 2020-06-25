#pragma warning disable 0649

using Unity.Entities;

namespace Reese.Demo
{
    /// <summary>For authoring a cylinder prefab.</summary>
    [GenerateAuthoringComponent]
    struct CylinderPrefab : IComponentData
    {
        /// <summary>A reference to the cylinder prefab as an Entity.</summary>
        public Entity Value;
    }
}
