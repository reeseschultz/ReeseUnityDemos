using Unity.Entities;

namespace Reese.Demo
{
    [InternalBufferCapacity(10)]
    public struct SpatialPreviousEntryBufferElement : IBufferElementData
    {
        public static implicit operator Entity(SpatialPreviousEntryBufferElement e) { return e.Value; }
        public static implicit operator SpatialPreviousEntryBufferElement(Entity e) { return new SpatialPreviousEntryBufferElement { Value = e }; }

        public Entity Value;
    }
}
