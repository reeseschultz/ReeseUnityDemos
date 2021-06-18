using System;
using Unity.Entities;

namespace Reese.Path
{
    /// <summary>Exists if the agent is planning paths.</summary>
    [Serializable]
    public struct PathPlanning : IComponentData { }
}
