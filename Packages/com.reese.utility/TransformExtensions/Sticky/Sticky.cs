using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Reese.Utility
{
    [Serializable]
    public struct Sticky : IComponentData
    {
        /// <summary>The collision filter to use.</summary>
        public CollisionFilter Filter;

        /// <summary>The world direction unit vector in which collider-casting occurs to stick the attached entity.</summary>
        public float3 WorldDirection;

        /// <summary>Radius of collider-casting SphereGeometry used to stick this entity to another.</summary>
        public float Radius;

        /// <summary>Number of attempts the StickySystem has to stick the object. The StickyFailed component will be added to it in case of failure.</summary>
        public int StickAttempts;
    }
}
