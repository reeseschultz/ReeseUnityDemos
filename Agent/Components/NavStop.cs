using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent needs to stop moving (waits for jumping or falling to complete).</summary>
    [Serializable]
    public struct NavStop : IComponentData { }
}
