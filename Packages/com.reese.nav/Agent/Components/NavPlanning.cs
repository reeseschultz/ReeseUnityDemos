using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is planning paths or jumps.</summary>
    [Serializable]
    public struct NavPlanning : IComponentData { }
}
