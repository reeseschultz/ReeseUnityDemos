# Reese's DOTS Spawning User Guide

![Video of spawning prefabs with Unity ECS.](/Gifs/spawn-demo.gif)

Genericized and evil DOTS runtime spawning using reflection.

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
