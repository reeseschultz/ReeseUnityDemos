using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;

namespace ReeseUnityDemos
{
    class RandomSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            var entity = World.EntityManager.CreateEntity();
            var buffer = World.EntityManager.AddBuffer<RandomBufferElement>(entity);
            var seed = new System.Random();

            for (int i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
                buffer.Add(new Unity.Mathematics.Random((uint)seed.Next()));
        }

        protected override void OnUpdate() { }
    }
}
