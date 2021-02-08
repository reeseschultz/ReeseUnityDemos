using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialOverlap : IBufferElementData
    {
        public static implicit operator Entity(SpatialOverlap e) { return e.Value; }
        public static implicit operator SpatialOverlap(Entity e) { return new SpatialOverlap { Value = e }; }

        public Entity Value;
    }
}
