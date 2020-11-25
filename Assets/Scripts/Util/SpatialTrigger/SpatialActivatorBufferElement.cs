using Unity.Entities;

namespace Reese.Demo
{
    [InternalBufferCapacity(10)]
    public struct SpatialActivatorBufferElement : IBufferElementData
    {
        public static implicit operator Entity(SpatialActivatorBufferElement e) { return e.Value; }
        public static implicit operator SpatialActivatorBufferElement(Entity e) { return new SpatialActivatorBufferElement { Value = e }; }

        public Entity Value;
    }
}
