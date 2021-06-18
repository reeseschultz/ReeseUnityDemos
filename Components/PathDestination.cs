using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Path
{
    /// <summary>Exists if the agent needs a destination.</summary>
    [Serializable]
    public struct PathDestination : IComponentData
    {
        /// <summary>The 3D world destination coordinate.</summary>
        public float3 WorldPoint;

        /// <summary>If this destination is within the provided tolerance of the last destination for a given agent, it will be ignored. Useful for mouselook since many new destinations can block interpolation.</summary>
        public float Tolerance;
    }
}
