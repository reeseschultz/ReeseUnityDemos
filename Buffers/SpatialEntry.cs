using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(SpatialConstants.SPATIAL_ENTRY_BUFFER_CAPACITY)]
    public struct SpatialEntry : IBufferElementData
    {
        public static implicit operator SpatialEvent(SpatialEntry e) { return e.Value; }
        public static implicit operator SpatialEntry(SpatialEvent e) { return new SpatialEntry { Value = e }; }

        public SpatialEvent Value;
    }
}
