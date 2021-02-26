using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Demo
{
    [Serializable]
    public struct Rotator : IComponentData
    {
        public float3 FromRelativeAngles;
        public float3 ToRelativeAngles;
        public float Frequency;
    }
}
