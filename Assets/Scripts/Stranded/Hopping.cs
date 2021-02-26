using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Demo.Stranded
{
    [Serializable]
    public struct Hopping : IComponentData
    {
        public float3 OriginalPosition;
        public float Height;
        public float StartSeconds;
        public float Duration;
    }
}
