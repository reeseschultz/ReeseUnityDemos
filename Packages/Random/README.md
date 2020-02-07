# Reese's DOTS Randomization User Guide

![Video of agents spawning and avoiding obstacles.](/Gifs/nav-performance-demo.gif)

Exposes `Unity.Mathematics.Random` number generators compatible with Burst-compiled jobs.

## Import

The randomization code is a stand-alone [UPM package](https://docs.unity3d.com/Manual/Packages.html), meaning you can import it directly into your project as long as you're using >=`2019.3`. While support for [multiple subpackages in a single Git repository won't be available until 2020.1](https://forum.unity.com/threads/git-support-on-package-manager.573673/page-5), I hacked it to work anyway by creating a Git subtree for each package.

To take advantage of this, just run the below line in your shell.

```sh
DEMO_URL=https://github.com/reeseschultz/ReeseUnityDemos.git && echo $DEMO_URL#$(git ls-remote $DEMO_URL random | grep -Po "^([\w\-]+)")
```

Copy the output. Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

It'll take a little while for the import to work. It should install the required dependencies and appropriate versions for you. After it's done doing its thing, note that *Reese's DOTS Randomization*'s version will display as `0.0.0`, since all we care about is the most recent commit hash. [Semantic versioning](https://semver.org/) may be added later.

## Usage

```csharp
class SomeJobSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

        return Entities
            .WithNativeDisableParallelForRestriction(randomArray)
            .ForEach((int nativeThreadIndex, ref SomeComponent someComponent) =>
            {
                var random = RandomArray[nativeThreadIndex];

                someComponent.SomeField = random.Next(0, 1000);

                RandomArray[nativeThreadIndex] = random; // This is NECESSARY.
            })
            .WithName("SomeJob")
            .Schedule(inputDeps);
    }
}
```

We pass the array of random number generators to the Burst-compiled `SomeJob`, created through the `Entities.ForEach` syntactic sugar. Thread safety is accomplished by only modifying the state of each generator via the magic `nativeThreadIndex` (the current thread index of an executing job). Since we are managing safety ourselves, we must however pass the `randomArray` to `WithNativeDisableParallelForRestriction`. Yes, it's safe as long as you use the `nativeThreadIndex` as shown.

Note that, to ensure the state of a given generator updates upon each call to `Execute`, we must set it like so: `RandomArray[nativeThreadIndex] = random`. Otherwise, we'll only modify a copy of a given random generator object, and after a while things won't appear to be very random anymore!
