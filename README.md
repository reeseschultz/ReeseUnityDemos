[![openupm](https://img.shields.io/npm/v/com.reese.random?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.reese.random/)

# Reese's DOTS Randomization

![Video of agents spawning and avoiding obstacles.](/Gifs/nav-performance-demo.gif)

Exposes `Unity.Mathematics.Random` number generators compatible with Burst-compiled jobs.

## Import

There are two ways to import this package, one being with [OpenUPM](https://openupm.com/), the preferred method, and the other via Git URL:

### OpenUPM

This requires [Node.js](https://nodejs.org/en/) `12` or greater. `cd` to your project's directory and run:

```sh
npx openupm-cli add com.reese.random
```

### Git

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#random`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#random`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Usage

```csharp
namespace YourNamespace {
    class YourJobSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Entities
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((int nativeThreadIndex, ref YourComponent yourComponent) =>
                {
                    var random = randomArray[nativeThreadIndex];

                    yourComponent.SomeField = random.Next(0, 1000);

                    randomArray[nativeThreadIndex] = random; // This is NECESSARY.
                })
                .WithName("SomeJob")
                .ScheduleParallel();
        }
    }
}
```

We pass the array of random number generators to the Burst-compiled `SomeJob`, created through the `Entities.ForEach` syntactic sugar. Thread safety is accomplished by only modifying the state of each generator via the magic `nativeThreadIndex` (the current thread index of an executing job). Since we are managing safety ourselves, we must however pass the `randomArray` to `WithNativeDisableParallelForRestriction`. Yes, it's safe as long as you use the `nativeThreadIndex` as shown.

Note that, to ensure the state of a given generator updates upon each call to `Execute`, we must set it like so: `RandomArray[nativeThreadIndex] = random`. Otherwise, we'll only modify a copy of a given random generator object, and after a while things won't appear to be very random anymore!
