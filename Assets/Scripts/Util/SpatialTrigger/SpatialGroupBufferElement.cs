using Unity.Collections;
using Unity.Entities;

namespace Reese.Demo
{
    [InternalBufferCapacity(10)]
    public struct SpatialGroupBufferElement : IBufferElementData
    {
        public static implicit operator FixedString128(SpatialGroupBufferElement e) { return e.Value; }
        public static implicit operator SpatialGroupBufferElement(FixedString128 e) { return new SpatialGroupBufferElement{ Value = e }; }
        public static implicit operator SpatialGroupBufferElement(string e) { return new SpatialGroupBufferElement{ Value = e }; }

        public FixedString128 Value;
    }
}
