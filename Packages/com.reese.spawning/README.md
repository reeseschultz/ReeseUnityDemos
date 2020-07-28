[![openupm](https://img.shields.io/npm/v/com.reese.spawning?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.reese.spawning/)

# Reese's DOTS Spawning

![Video of spawning prefabs with Unity ECS.](/Gifs/spawn-demo.gif)

Generic DOTS runtime spawning for any combination of prefab, components, and buffers.

## IL2CPP

This package is **NOT** compatible with [IL2CPP](https://docs.unity3d.com/Manual/IL2CPP.html) due to use of reflection (see Unity's guidance on [scripting restrictions](https://docs.unity3d.com/Manual/ScriptingRestrictions.html)). [Mono](https://www.mono-project.com/) works fine, however. Preferably reflection would never be used for anything, but there appears to be no way to achieve generic runtime spawning without modifying the `Entities` package to publicly expose internal methods such as `SetComponentDataRaw`.

## Import

There are two ways to import this package, one being with [OpenUPM](https://openupm.com/), the preferred method, and the other via Git URL:

### OpenUPM

This requires [Node.js](https://nodejs.org/en/) `12` or greater. `cd` to your project's directory and run:

```sh
npx openupm-cli add com.reese.spawning
```

### Git

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#spawning`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#spawning`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Usage

If you're unfamiliar with ECS prefab conversion, see [this tutorial](https://reeseschultz.com/spawning-prefabs-with-unity-ecs/). Otherwise, see below:

```csharp
using Reese.Spawning;
using Unity.Entities;
using UnityEngine;

namespace YourNamespace {
    class SomeSpawner : MonoBehaviour
    {
        // Get the default world:
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            // Get the prefab entity (GetSingletonEntity doesn't work for some reason):
            prefabEntity = entityManager.CreateEntityQuery(typeof(SomePrefab)).GetSingleton<SomePrefab>().Value;

            // Enqueue spawning (SpawnSystem and Spawn are from Reese.Spawning):
            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(prefabEntity) //  Optional prefab entity.
                .WithComponentList( // Optional comma-delimited list of IComponentData.
                    new YourComponent
                    {
                        Charisma = 6,
                        Intelligence = 3,
                        Luck = 14,
                        Strength = 16,
                        Wisdom = 9
                    }
                )
                .WithBufferList( // Optional comma-delimited list of IBufferElementData.
                    new SomeBufferElement { }
                ),
                50 // Optional number of times to spawn (default is once).
            );
        }
    }
}
```

Spawning can be done in many ways. To see how to trigger it via button click, see the redundantly-named [SpawnDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/SpawnDemoSpawner.cs).
