using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Demo
{
    public struct SpatialActivator : IComponentData
    {
        public AABB Bounds;
    }
}
