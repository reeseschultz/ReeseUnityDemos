#pragma warning disable 0649

using System;
using Unity.Entities;

namespace Reese.Demo
{
    /// <summary>For authoring a cylinder prefab.</summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct CylinderPrefab : IComponentData
    {
        /// <summary>A reference to the cylinder prefab as an Entity.</summary>
        public Entity Value;
    }
}
