using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is falling.</summary>
    struct NavFalling : IComponentData { }

    /// <summary>Exists if the agent is jumping.</summary>
    struct NavJumping : IComponentData { }

    /// <summary>Exists if the agent  recently jumped, *past tense*, primarily
    /// so the NavSurfaceSystem can raycast for a new surface potentially
    /// underneath said agent.</summary>
    struct NavJumped : IComponentData { }

    /// <summary>Exists if the agent is lerping.</summary>
    struct NavLerping : IComponentData { }

    /// <summary>Exists if the agent is planning paths or jumps.</summary>
    struct NavPlanning : IComponentData { }
}
