using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;

namespace Reese.Random
{
    /// <summary>Handles initialization, storage, and disposal of
    /// thread-indexed random number generators of the
    /// Unity.Mathematics.Random variety.</summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class RandomSystem : ComponentSystem
    {
        /// <summary>A NativeArray of thread-indexed Unity.Mathematics.Random
        /// number generators.</summary>
        public NativeArray<Unity.Mathematics.Random> RandomArray { get; private set; }

        protected override void OnCreate()
        {
            var randomArray = new Unity.Mathematics.Random[JobsUtility.MaxJobThreadCount];
            var seed = new System.Random();

            for (int i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
                randomArray[i] = new Unity.Mathematics.Random((uint)seed.Next());

            RandomArray = new NativeArray<Unity.Mathematics.Random>(randomArray, Allocator.Persistent);
        }

        protected override void OnDestroy()
            => RandomArray.Dispose();

        protected override void OnUpdate() { }
    }
}
