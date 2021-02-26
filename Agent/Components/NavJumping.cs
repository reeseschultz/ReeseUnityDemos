using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is jumping.</summary>
    [Serializable]
    public struct NavJumping : IComponentData { }
}
