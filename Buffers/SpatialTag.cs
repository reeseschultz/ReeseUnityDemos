using Unity.Collections;
using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(SpatialConstants.SPATIAL_TAG_BUFFER_CAPACITY)]
    public struct SpatialTag : IBufferElementData
    {
        public static implicit operator FixedString128Bytes(SpatialTag e) { return e.Value; }
        public static implicit operator SpatialTag(FixedString128Bytes e) { return new SpatialTag{ Value = e }; }
        public static implicit operator SpatialTag(string e) { return new SpatialTag{ Value = e }; }

        public FixedString128Bytes Value;
    }
}
