using System;
using Unity.Entities;
using Unity.Physics;

namespace Reese.Spatial
{
    [Serializable]
    public struct SpatialTrigger : IComponentData
    {
        public CollisionFilter Filter;
    }
}
