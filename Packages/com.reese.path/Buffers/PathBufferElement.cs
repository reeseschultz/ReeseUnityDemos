using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Path
{
    /// <summary>A buffer of positions agents may traverse to reach their
    /// destinations. Written to in the PathPlanSystem.
    /// </summary>
    [InternalBufferCapacity(PathConstants.PATH_NODE_MAX)]
    public struct PathBufferElement : IBufferElementData
    {
        public static implicit operator float3(PathBufferElement e) { return e.Value; }
        public static implicit operator PathBufferElement(float3 e) { return new PathBufferElement { Value = e }; }

        /// <summary>A waypoint along the path calculated by the PathPlanSystem.
        /// </summary>
        public float3 Value;
    }
}
