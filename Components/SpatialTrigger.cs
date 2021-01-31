using Unity.Entities;
using Unity.Physics;

namespace Reese.Spatial
{
    public struct SpatialTrigger : IComponentData
    {
        public CollisionFilter Filter;
    }
}
