using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(SpatialConstants.SPATIAL_EXIT_BUFFER_CAPACITY)]
    public struct SpatialExit : IBufferElementData
    {
        public static implicit operator SpatialEvent(SpatialExit e) { return e.Value; }
        public static implicit operator SpatialExit(SpatialEvent e) { return new SpatialExit { Value = e }; }

        public SpatialEvent Value;
    }
}
