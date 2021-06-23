using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>
    /// Exists if flocking behaviours should be applied to the agent.
    /// </summary>
    [Serializable]
    public struct NavFlocking : IComponentData { }
}
