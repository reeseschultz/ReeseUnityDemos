using Unity.Collections;
using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialTagBufferElement : IBufferElementData
    {
        public static implicit operator FixedString128(SpatialTagBufferElement e) { return e.Value; }
        public static implicit operator SpatialTagBufferElement(FixedString128 e) { return new SpatialTagBufferElement{ Value = e }; }
        public static implicit operator SpatialTagBufferElement(string e) { return new SpatialTagBufferElement{ Value = e }; }

        public FixedString128 Value;
    }
}
