using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent needs to traverse complex terrain.</summary>
    [Serializable]
    public struct NavTerrainCapable : IComponentData { }
}
