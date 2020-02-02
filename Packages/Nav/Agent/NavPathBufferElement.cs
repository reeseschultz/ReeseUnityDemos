using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Nav
{
    /// <summary>A buffer of positions NavAgents may traverse to reach their
    /// destinations. Written to in the NavPlanSystem for path and jump
    /// planning. Read in the NavInterpolationSystem for interpolation.
    /// </summary>
    [InternalBufferCapacity(NavConstants.PATH_NODE_MAX)]
    public struct NavPathBufferElement : IBufferElementData
    {
        public static implicit operator float3(NavPathBufferElement e) { return e.Value; }
        public static implicit operator NavPathBufferElement(float3 e) { return new NavPathBufferElement { Value = e }; }

        /// <summary>A waypoint along the path calculated by the NavPlanSystem.
        /// </summary>
        public float3 Value;
    }
}
