using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    /// <summary>Exists if the agent is falling.</summary>
    public struct NavFalling : IComponentData { }

    /// <summary>Exists if the agent has a problem.</summary>
    public struct NavHasProblem : IComponentData
    {
        /// <summary>The problematic status preventing further path planning.
        /// See: https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html
        /// </summary>
        public PathQueryStatus Value;
    }

    /// <summary>Exists if the agent is jumping.</summary>
    public struct NavJumping : IComponentData { }

    /// <summary>Exists if the agent is lerping.</summary>
    public struct NavLerping : IComponentData { }

    /// <summary>Exists if the agent needs a destination.</summary>
    public struct NavNeedsDestination : IComponentData
    {
        /// <summary>The 3D destination coordinate.</summary>
        public float3 Destination;

        /// <summary>True if teleporting to the specified destination, false if
        /// not (the default).</summary>
        public bool Teleport;
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
}
