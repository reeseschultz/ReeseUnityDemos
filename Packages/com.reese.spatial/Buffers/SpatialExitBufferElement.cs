using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialExitBufferElement : IBufferElementData
    {
        public static implicit operator Entity(SpatialExitBufferElement e) { return e.Value; }
        public static implicit operator SpatialExitBufferElement(Entity e) { return new SpatialExitBufferElement { Value = e }; }

        public Entity Value;
    }
}
