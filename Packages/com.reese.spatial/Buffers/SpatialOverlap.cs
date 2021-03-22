using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(SpatialConstants.SPATIAL_OVERLAP_BUFFER_CAPACITY)]
    public struct SpatialOverlap : IBufferElementData
    {
        public static implicit operator SpatialEvent(SpatialOverlap e) { return e.Value; }
        public static implicit operator SpatialOverlap(SpatialEvent e) { return new SpatialOverlap { Value = e }; }

        public SpatialEvent Value;
    }
}
