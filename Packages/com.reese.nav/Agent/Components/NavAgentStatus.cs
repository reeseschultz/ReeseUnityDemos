using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is falling.</summary>
    public struct NavFalling : IComponentData { }

    /// <summary>Exists if the agent is following an entity.</summary>
    public struct NavFollow : IComponentData
    {
        /// <summary>The target entity that this entity will follow.</summary>
        public Entity Target;

        /// <summary>Maximum distance before this agent will stop following the target entity. If less than or equal to zero, this agent will follow the target entity no matter how far it is away.</summary>
        public float MaxDistance;

        /// <summary>Minimum distance this agent maintains between itself and the target entity it follows.</summary>
        public float MinDistance;
    }

    /// <summary>Exists if the agent has a problem.</summary>
    public struct NavHasProblem : IComponentData
    {
        /// <summary>The problematic status preventing further path planning. See: https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html</summary>
        public PathQueryStatus Value;
    }

    /// <summary>Exists if the agent is jumping.</summary>
    public struct NavJumping : IComponentData { }

    /// <summary>Exists if the agent is walking.</summary>
    public struct NavWalking : IComponentData { }

    /// <summary>Exists if the user needs to handle lerping.</summary>
    public struct NavCustomLerping : IComponentData { }

    /// <summary>Exists if the agent needs a destination.</summary>
    public struct NavDestination : IComponentData
    {
        /// <summary>The 3D world destination coordinate.</summary>
        public float3 WorldPoint;

        /// <summary>True if teleporting to the specified destination, false if not (the default).</summary>
        public bool Teleport;

        /// <summary>If this destination is within the provided tolerance of the last destination for a given agent, it will be ignored. Useful for mouselook since many new destinations can block interpolation.</summary>
        public float Tolerance;

        /// <summary>True if lerping should be disabled by the navigation
        /// package so that it can be handled by user code. Users must
        /// then manage components and buffers themselves (the
        /// NavPathBufferElement class is the most important to that end).
        /// False if otherwise. Note that the NavCustomLerping component
        /// will exist on the agent if planning is done and custom lerping
        /// is required.</summary>
        public bool CustomLerp;
    }

    /// <summary>Exists if the agent needs a surface. This component should be
    /// added when spawning an agent. It's also automatically added after the
    /// agent jumps. When this component exists, the NavSurfaceSystem will try
    /// to raycast for a new surface potentially underneath said agent.</summary>
    public struct NavNeedsSurface : IComponentData { }

    /// <summary>Exists if the agent is planning paths or jumps.</summary>
    public struct NavPlanning : IComponentData { }

    /// <summary>Exists if the agent needs to stop moving (waits for jumping or falling to complete).</summary>
    public struct NavStop : IComponentData { }

    /// <summary>Exists if the agent needs to traverse complex terrain.</summary>
    public struct NavTerrainCapable : IComponentData { }

    /// <summary>Exists if the agent's translation needs to be fixed.</summary>
    public struct NavFixTranslation : IComponentData { }
}
