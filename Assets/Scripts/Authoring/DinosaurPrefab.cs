#pragma warning disable 0649

using System;
using Unity.Entities;

namespace Reese.Demo
{
    /// <summary>For authoring a dinosaur prefab.</summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct DinosaurPrefab : IComponentData
    {
        /// <summary>A reference to the dinosaur prefab as an Entity.</summary>
        public Entity Value;
    }
}
