using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;

namespace ReeseUnityDemos
{
    [InternalBufferCapacity(JobsUtility.MaxJobThreadCount)]
    public struct RandomBufferElement : IBufferElementData
    {
        public static implicit operator Unity.Mathematics.Random(RandomBufferElement e) { return e.Value; }
        public static implicit operator RandomBufferElement(Unity.Mathematics.Random e) { return new RandomBufferElement { Value = e }; }

        public Unity.Mathematics.Random Value;
    }
}
