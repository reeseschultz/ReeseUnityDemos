using Unity.Entities;

namespace Reese.Spatial
{
    [InternalBufferCapacity(10)]
    public struct SpatialOverlapBufferElement : IBufferElementData
    {
        public static implicit operator Entity(SpatialOverlapBufferElement e) { return e.Value; }
        public static implicit operator SpatialOverlapBufferElement(Entity e) { return new SpatialOverlapBufferElement { Value = e }; }

        public Entity Value;
    }
}
