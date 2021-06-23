using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>
    /// Exists if the agent should try steering away from obstacles. This is an optional addition for flocking agents.
    /// </summary>
    [Serializable]
    public struct NavObstacleSteering : IComponentData { }
}
