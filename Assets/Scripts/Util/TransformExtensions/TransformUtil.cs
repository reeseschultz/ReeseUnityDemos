using Unity.Mathematics;

namespace Reese.Demo
{
    public static class TransformUtil
    {
        /// <summary>Transforms a point (equivalent to Matrix4x4.MultiplyPoint3x4, but uses Unity.Mathematics).</summary>
        public static float3 MultiplyPoint3x4(this float4x4 transform, float3 point)
            => math.mul(transform, new float4(point, 1)).xyz;

        public static float3 MultiplyPoint3x4(this float3 point, float4x4 transform)
            => math.mul(transform, new float4(point, 1)).xyz;
    }
}
