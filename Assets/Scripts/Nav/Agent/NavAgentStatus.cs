using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is falling.</summary>
    struct NavFalling : IComponentData { }

    /// <summary>Exists if the agent is jumping.</summary>
    struct NavJumping : IComponentData { }

    /// <summary>Exists if the agent is lerping.</summary>
    struct NavLerping : IComponentData { }

    /// <summary>Exists if the agent needs a surface. This component should be
    /// added when spawning an agent. It's also automatically added after the
    /// agent jumps. When this component exists, the NavSurfaceSystem will try
    /// to raycast for a new surface potentially underneath said agent.</summary>
    struct NavNeedsSurface : IComponentData { }

    /// <summary>Exists if the agent is planning paths or jumps.</summary>
    struct NavPlanning : IComponentData { }
}
