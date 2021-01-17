using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Reese.Demo
{
    public struct Sticky : IComponentData
    {
        public CollisionFilter Filter;
        public float3 WorldDirection;
        public float Radius;
        public float Offset;
        public int StickAttempts;
    }
}
