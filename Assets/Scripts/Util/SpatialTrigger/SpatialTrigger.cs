using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Demo
{
    public struct SpatialTrigger : IComponentData
    {
        public AABB Bounds;
    }
}
