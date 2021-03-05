using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialOverlap : IBufferElementData
    {
        public static implicit operator SpatialEvent(SpatialOverlap e) { return e.Value; }
        public static implicit operator SpatialOverlap(SpatialEvent e) { return new SpatialOverlap { Value = e }; }

        public SpatialEvent Value;
    }
}
