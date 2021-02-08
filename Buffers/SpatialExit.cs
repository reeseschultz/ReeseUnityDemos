using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialExit : IBufferElementData
    {
        public static implicit operator Entity(SpatialExit e) { return e.Value; }
        public static implicit operator SpatialExit(Entity e) { return new SpatialExit { Value = e }; }

        public Entity Value;
    }
}
