#pragma warning disable 0649

using System;
using Unity.Entities;

namespace Reese.Demo
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct PersonPrefab : IComponentData
    {
        public Entity Value;
    }
}
