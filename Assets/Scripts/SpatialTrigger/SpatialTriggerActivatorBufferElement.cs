using Unity.Entities;

namespace Reese.Demo
{
    [InternalBufferCapacity(10)]
    public struct SpatialTriggerActivatorBufferElement : IBufferElementData
    {
        public static implicit operator Entity(SpatialTriggerActivatorBufferElement e) { return e.Value; }
        public static implicit operator SpatialTriggerActivatorBufferElement(Entity e) { return new SpatialTriggerActivatorBufferElement { Value = e }; }

        public Entity Value;
    }
}
