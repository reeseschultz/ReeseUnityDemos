using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Nav
{
    /// <summary>A buffer of a single "jumpable" position intended for
    /// NavAgents. Written to in the NavPlanSystem. Read in the
    /// NavInterpolationSystem, but also written to from there in order to
    /// clear the buffer when jumping is complete.</summary>
    [InternalBufferCapacity(1)]
    public struct NavJumpBufferElement : IBufferElementData
    {
        public static implicit operator float3(NavJumpBufferElement e) { return e.Value; }
        public static implicit operator NavJumpBufferElement(float3 e) { return new NavJumpBufferElement { Value = e }; }

        /// <summary>The position of the intended jump destination.</summary>
        public float3 Value;
    }
}
