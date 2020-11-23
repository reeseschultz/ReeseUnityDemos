using Unity.Entities;

namespace Reese.Demo
{
    public struct SpatialTriggerEvent : IComponentData
    {
        public Entity Activator;
    }
}
