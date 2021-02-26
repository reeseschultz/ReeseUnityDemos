using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is falling.</summary>
    [Serializable]
    public struct NavFalling : IComponentData { }
}
