using Unity.Entities;

namespace Reese.Demo
{
    [InternalBufferCapacity(10)]
    public struct SpatialTriggerBufferElement : IBufferElementData
    {
        public static implicit operator Entity(SpatialTriggerBufferElement e) { return e.Value; }
        public static implicit operator SpatialTriggerBufferElement(Entity e) { return new SpatialTriggerBufferElement { Value = e }; }

        public Entity Value;
    }
}
