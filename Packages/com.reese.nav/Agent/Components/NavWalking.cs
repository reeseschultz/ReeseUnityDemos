using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is walking.</summary>
    [Serializable]
    public struct NavWalking : IComponentData { }
}
