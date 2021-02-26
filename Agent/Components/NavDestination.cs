using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Nav
{
    /// <summary>Exists if the agent needs a destination.</summary>
    [Serializable]
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
}
