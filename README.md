# Reese's DOTS Spatial Events

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)

![Video of agents spawning and avoiding obstacles.](https://raw.githubusercontent.com/reeseschultz/ReeseUnityDemos/master/preview.gif)

Reactive entry and exit events in Burst-capable jobs.

## Import

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#spatial`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#spatial`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Concepts

This package uses these concepts:

1. **Triggers** - Triggers react to overlapping activators. Entities with the `SpatialTrigger` component are processed as triggers. `SpatialTriggerAuthoring` is provided for convenience.
2. **Activators** - Activators activate overlapping triggers. Activators that enter a trigger are **entries**. Activators that exit a trigger are **exits**. Entities with the `SpatialActivator` component are processed as activators. `SpatialActivatorAuthoring` is provided for convenience.
3. **Tags** - Grouping of triggers and activators. Triggers and tags should both have a `DynamicBuffer` of the type `SpatialTagBufferElement`. It's comprised of `FixString128`s—these are the tags. When at least one tag of an activator matches that of a trigger, that trigger is considered activated (as long as collision filters permit)! A `DynamicBuffer` of type `SpatialEntryBufferElement` is added upon entry, and a `DynamicBuffer` of type `SpatialExitBufferElement` is added upon exit.

Note that `Unity.Physics` is used behind the scenes for efficiency. It uses a [bounding volume hierarchy](https://en.wikipedia.org/wiki/Bounding_volume_hierarchy) (BVH) to quickly detect collisions between collidable objects. What this means for you, the user, is that your activators and triggers **must** have colliders attached to them! This package will not work otherwise. Also, remember that entities with a `PhysicsBody` or GameObjects with a `RigidBody` are considered to be dynamic! Ensure you indicate whether your objects are static or dynamic to Unity for accuracy and efficiency.

You may want to set the trigger entity as a child of a parent when said parent needs its immediate collider to be exact in the physical sense. This way the bounds of the trigger won't interfere with the parent's hitbox. Oftentimes the trigger bounds are entirely different from what we consider to be the physical bounds.

## Usage

```csharp
...
using Reese.Spatial;
...

[UpdateAfter(typeof(SpatialStartSystem)), UpdateBefore(typeof(SpatialEndSystem))]
class CatSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities // Example handling of the overlap buffer.
            .WithAll<SpatialTrigger, Cat>()
            .WithChangeFilter<SpatialOverlapBufferElement>() // Allows us to process what is currently overlapping.
            .ForEach((in DynamicBuffer<SpatialOverlapBufferElement> overlaps) => // Do NOT modify the buffer, hence the in keyword.
            {
                // There could be code here to process what currently overlaps in a given frame.
            })
            .WithoutBurst() // Can NOT use Burst when logging. Remove this line if you're not logging in the job!
            .WithName("CatOverlapJob")
            .ScheduleParallel();

        Entities // Example handling of the spatial entry buffer.
            .WithAll<SpatialTrigger, Cat>()
            .WithChangeFilter<SpatialEntryBufferElement>() // Allows us to process (only) new entries once.
            .ForEach((in DynamicBuffer<SpatialEntryBufferElement> entries) => // Do NOT modify the buffer, hence the in keyword.
            {
                for (var i = entries.Length - 1; i >= 0; --i) // Traversing from the end of the buffer for performance reasons.
                {
                    Debug.Log(entries[i].Value + " is making me purr! Purrrrrrrr!");
                }
            })
            .WithoutBurst() // Can NOT use Burst when logging. Remove this line if you're not logging in the job!
            .WithName("CatEntryJob")
            .ScheduleParallel();

        Entities // Example handling of the spatial exit buffer.
            .WithAll<SpatialTrigger, Cat>()
            .WithChangeFilter<SpatialExitBufferElement>() // Allows us to process (only) new exits once.
            .ForEach((in DynamicBuffer<SpatialExitBufferElement> exits) => // Do NOT modify the buffer, hence the in keyword.
            {
                for (var i = exits.Length - 1; i >= 0; --i) // Traversing from the end of the buffer for performance reasons.
                {
                    Debug.Log(exits[i].Value + " is making me meow for attention! MEEEOWWWWWWW!");
                }
            })
            .WithoutBurst() // Can NOT use Burst when logging. Remove this line if you're not logging in the job!
            .WithName("CatExitJob")
            .ScheduleParallel();
    }
}
```

Note that `[UpdateAfter(typeof(SpatialStartSystem)), UpdateBefore(typeof(SpatialEndSystem))]` is critically important! Your system needs to run between the `SpatialStartSystem` and `SpatialEndSystem`. Otherwise, things will not work!

In this example we have a `Cat` that happens to be a trigger. It is easy to tell which entities currently overlap with the `Cat` in the given frame with the first job. When entities enter and exit the `Cat`'s bounds, there are a couple more jobs defined to log about them appropriately (as long as the tags and collision filters permit). If another entity enters the `Cat`'s bounds, purring ensues. Otherwise, upon exit, the `Cat` meows for attention. See the working [CatSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Stranded/Cat/CatSystem.cs) in the containing monorepo that this example was modified from. That system is used in the `Stranded` demo, a concrete way to start learning and using this package and others!

If you want to handle different kinds of events per trigger, you would most likely want to check for existence of a component per entry or exit. To that end, you could use `GetComponentFromEntity`.

Also, it is perfectly fine for an object to be *both* a trigger and activator (even though it may belong to the same tag as itself, self-activation is **not** possible).

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever. All that said, [GitHub Issues](https://github.com/reeseschultz/ReeseUnityDemos/issues/new/choose) is fine for constructive discussion.

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
