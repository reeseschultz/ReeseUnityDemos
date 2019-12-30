using System.Threading;

namespace ReeseUnityDemos
{
    static class Util
    {
        public static class ConcurrentRandom
        {
            static readonly System.Random unsafeRandom = new System.Random();
            static readonly ThreadLocal<System.Random> safeRandom = new ThreadLocal<System.Random>(() =>
            {
                lock (unsafeRandom) return new System.Random(unsafeRandom.Next());
            });

            public static int NextInt(int max) => safeRandom.Value.Next(max);
            public static int NextInt(int min, int max) => safeRandom.Value.Next(min, max);
            public static double NextDouble() => safeRandom.Value.NextDouble();
        }
    }
}
