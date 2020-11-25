using Unity.Entities;

namespace Reese.Demo
{
    public struct SpatialEvent : IComponentData
    {
        public Entity Activator;
    }
}
