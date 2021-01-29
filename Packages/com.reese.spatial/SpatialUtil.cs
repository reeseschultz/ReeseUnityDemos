using Unity.Mathematics;

namespace Reese.Spatial
{
    public static class SpatialUtil
    {
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
