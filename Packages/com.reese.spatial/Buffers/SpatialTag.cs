using Unity.Collections;
using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialTag : IBufferElementData
    {
        public static implicit operator FixedString128(SpatialTag e) { return e.Value; }
        public static implicit operator SpatialTag(FixedString128 e) { return new SpatialTag{ Value = e }; }
        public static implicit operator SpatialTag(string e) { return new SpatialTag{ Value = e }; }

        public FixedString128 Value;
    }
}
