using Unity.Entities;
using Unity.Physics;

namespace Reese.Demo
{
    public struct SpatialTrigger : IComponentData
    {
        public CollisionFilter Filter;
    }
}
