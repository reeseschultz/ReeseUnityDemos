# Reese's DOTS Spawning User Guide

![Video of spawning prefabs with Unity ECS.](/Gifs/spawn-demo.gif)

Genericized and evil DOTS runtime spawning using reflection.

## Import

The spawning code is a stand-alone [UPM package](https://docs.unity3d.com/Manual/Packages.html), meaning you can import it directly into your project as long as you're using >=`2019.3`.  To take advantage of this, just copy one of the below Git URLs:

* **HTTPS:** https://github.com/reeseschultz/ReeseUnityDemos.git#spawning
* **SSH:** git@github.com:reeseschultz/ReeseUnityDemos.git#spawning

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

It'll take a little while for the import to work. It should install the required dependencies and appropriate versions for you. After it's done doing its thing, note that *Reese's DOTS Spawning*'s version will display as `0.0.0`, since all we care about is the most recent commit hash. [Semantic versioning](https://semver.org/) may be added later.

## Usage

```csharp
class SomeSpawner : MonoBehaviour
{
    EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

    void Start()
    {
        // First, get the entity associated with the prefab.
        var prefabEntity = entityManager.CreateEntityQuery(typeof(SomePrefab)).GetSingleton<SomePrefab>().Value;

        // Second and lastly, enqueue spawning.
        SpawnSystem.Enqueue(new Spawn()
            .WithPrefab(prefabEntity) //  Optional prefab entity.
            .WithComponentList( // Optional comma-delimited list of IComponentData.
                new Translation
                {
                    Value = new float3(100, 0, 100)
                },
                new SomeComponent
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
```

Spawning on `Update` is easy too, perhaps via button click. For an example of that, see the redundantly-named [SpawnDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/SpawnDemoSpawner.cs).
