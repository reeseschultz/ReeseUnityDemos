using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialEntry : IBufferElementData
    {
        public static implicit operator Entity(SpatialEntry e) { return e.Value; }
        public static implicit operator SpatialEntry(Entity e) { return new SpatialEntry { Value = e }; }

        public Entity Value;
    }
}
