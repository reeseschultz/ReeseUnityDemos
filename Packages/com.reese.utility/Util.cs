using Unity.Mathematics;

namespace Reese.Utility
{
    public static class Util
    {
        /// <summary>Transforms a point (equivalent to Matrix4x4.MultiplyPoint3x4, but uses Unity.Mathematics).</summary>
        public static float3 MultiplyPoint3x4(this float4x4 transform, float3 point)
            => math.mul(transform, new float4(point, 1)).xyz;

        /// <summary>Transforms a point (equivalent to Matrix4x4.MultiplyPoint3x4, but uses Unity.Mathematics).</summary>
        public static float3 MultiplyPoint3x4(this float3 point, float4x4 transform)
            => math.mul(transform, new float4(point, 1)).xyz;

        /// <summary>Converts the layer to a bit mask. Valid layers range from
        /// 8 to 30, inclusive. All other layers are invalid, and will always
        /// result in layer 8, since they are used by Unity internally. See
        /// https://docs.unity3d.com/Manual/class-TagManager.html and
        /// https://docs.unity3d.com/Manual/Layers.html for more information.
        /// </summary>
        public static uint ToBitMask(int layer)
            => (layer < 8 || layer > 30) ? 1u << 8 : 1u << layer;

        /// <summary>Inverts a bit mask, meaning that it applies to all layers
        /// *except* for the one expressed in said mask.</summary>
        public static uint InvertBitMask(uint bitMask)
            => ~bitMask;
    }
}
