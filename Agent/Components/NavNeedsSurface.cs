using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent needs a surface. This component should be
    /// added when spawning an agent. It's also automatically added after the
    /// agent jumps. When this component exists, the NavSurfaceSystem will try
    /// to raycast for a new surface potentially underneath said agent.</summary>
    [Serializable]
    public struct NavNeedsSurface : IComponentData { }
}
