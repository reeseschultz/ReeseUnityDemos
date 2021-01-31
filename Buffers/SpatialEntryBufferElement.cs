using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialEntryBufferElement : IBufferElementData
    {
        public static implicit operator Entity(SpatialEntryBufferElement e) { return e.Value; }
        public static implicit operator SpatialEntryBufferElement(Entity e) { return new SpatialEntryBufferElement { Value = e }; }

        public Entity Value;
    }
}
