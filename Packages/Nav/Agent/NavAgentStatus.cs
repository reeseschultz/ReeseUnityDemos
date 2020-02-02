using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is too physically close to other agents,
    /// thus yearning for personal space.</summary>
    public struct NavAvoidant : IComponentData { }

    /// <summary>Exists if the agent is falling.</summary>
    public struct NavFalling : IComponentData { }

    /// <summary>Exists if the agent is jumping.</summary>
    public struct NavJumping : IComponentData { }

    /// <summary>Exists if the agent is lerping.</summary>
    public struct NavLerping : IComponentData { }

    /// <summary>Exists if the agent needs a surface. This component should be
    /// added when spawning an agent. It's also automatically added after the
    /// agent jumps. When this component exists, the NavSurfaceSystem will try
    /// to raycast for a new surface potentially underneath said agent.</summary>
    public struct NavNeedsSurface : IComponentData { }

    /// <summary>Exists if the agent is planning paths or jumps.</summary>
    public struct NavPlanning : IComponentData { }
}
