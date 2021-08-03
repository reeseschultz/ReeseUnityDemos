# Reese's DOTS Randomization

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)
[![openupm](https://img.shields.io/npm/v/com.reese.random?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.reese.random/)

Exposes `Unity.Mathematics.Random` number generators compatible with Burst-compiled jobs.

## Support

Need support or software customization? [Read more about consulting with the maintainer, Reese, and other services he provides...](https://reese.codes)

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

## Credits

* The demos extensively use [Mini Mike's Metro Minis](https://mikelovesrobots.github.io/mmmm) (licensed with [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/?)) by [Mike Judge](https://github.com/mikelovesrobots). That project is embedded in this one by way of `Assets/MMMM/`. I modified its directory structure, and generated my own prefabs rather than using the included ones.
* One demo leverages animations from [Mixamo](https://www.mixamo.com) by [Adobe](https://www.adobe.com/).
* The sounds mixed in the demos are from [Freesound](https://freesound.org/); only ones licensed with [CC0](https://creativecommons.org/share-your-work/public-domain/cc0/) are used here.
* The navigation package uses [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) (licensed with [MIT](https://opensource.org/licenses/MIT)) by [Unity Technologies](https://github.com/Unity-Technologies).
* The navigation package also uses [PathUtils](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/PathUtils) (licensed with [zlib](https://opensource.org/licenses/Zlib)) by [Mikko Mononen](https://github.com/memononen), and modified by [Unity Technologies](https://github.com/Unity-Technologies). Did you know that Mikko is credited in [Death Stranding](https://en.wikipedia.org/wiki/Death_Stranding) for [Recast & Detour](https://github.com/recastnavigation/recastnavigation)?

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer, Reese, offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever, [unless you reach an agreement with him about support...](https://reese.codes)

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
