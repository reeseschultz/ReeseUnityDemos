# Reese's DOTS Spatial Events

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)

Reactive entry, overlap and exit events in Burst-capable jobs.

## Support

Need support or software customization? [Read more about consulting with the maintainer, Reese, and other services he provides...](https://reese.codes)

## Import

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#spatial`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#spatial`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Concepts

This package uses these concepts:

1. **Triggers** - Triggers react to overlapping activators. Entities with the `SpatialTrigger` component are processed as triggers. `SpatialTriggerAuthoring` is provided for convenience.
2. **Activators** - Activators activate overlapping triggers. Any activator that is currently overlapping a trigger is an **overlap**. Activators that enter a trigger are **entries**. Activators that exit a trigger are **exits**. Entities with the `SpatialActivator` component are processed as activators. `SpatialActivatorAuthoring` is provided for convenience.
3. **Tags** - Grouping of triggers and activators. Triggers and tags should both have a `DynamicBuffer` of the type `SpatialTag`. It's comprised of `FixedString128`s—these are the tags.

When at least one tag of an activator matches that of a trigger, that trigger is considered activated (as long as collision filters permit)! A `DynamicBuffer` of type `SpatialEntry` is populated upon entry, and a `DynamicBuffer` of type `SpatialExit` is populated upon exit. Current overlaps are in a `DynamicBuffer` of type `SpatialOverlap`. Entries, exits and overlaps contain a value of `SpatialEvent`, including the activating entity and its associated tag.

Be aware that `Unity.Physics` is used behind the scenes for efficiency. It uses a [bounding volume hierarchy](https://en.wikipedia.org/wiki/Bounding_volume_hierarchy) (BVH) to quickly detect collisions between collidable objects. What this means for you, the user, is that your activators and triggers **must** have colliders attached to them! This package will not work otherwise.

## Usage

For the sake of example, we'll start by creating a `CatSystem` that handles spatial events for entities with a `Cat` component, a `SpatialTrigger` component, and, finally, a `PhysicsCollider` component (since that's the only way the spatial events package can detect collisions).

```csharp
...
using Reese.Spatial;
...

namespace YourNamespace
{
    [UpdateAfter(typeof(SpatialStartSystem)), UpdateBefore(typeof(SpatialEndSystem))]
    class CatSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            ...
        }
    }
}
```

`[UpdateAfter(typeof(SpatialStartSystem)), UpdateBefore(typeof(SpatialEndSystem))]` is critically important! Your system needs to run between the `SpatialStartSystem` and `SpatialEndSystem`. Otherwise, things will not work!

### Overlaps

To handle what is currently overlapping, add this block to the `OnUpdate` method:

```csharp
Entities // Example handling of the overlap buffer.
    .WithAll<Cat, SpatialTrigger, PhysicsCollider>()
    .ForEach((in DynamicBuffer<SpatialOverlap> overlaps) => // Do NOT modify the buffer, hence the in keyword.
    {
        // There could be code here to process what currently overlaps in a given frame.
    })
    .WithName("CatOverlapJob")
    .ScheduleParallel();
```

### Entries

To handle entries, add this block to the `OnUpdate` method:

```csharp
Entities // Example handling of the spatial entry buffer.
    .WithAll<Cat, SpatialTrigger, PhysicsCollider>()
    .WithChangeFilter<SpatialEntry>() // Allows us to process (only) new entries once.
    .ForEach((in DynamicBuffer<SpatialEntry> entries) => // Do NOT modify the buffer, hence the in keyword.
    {
        for (var i = entries.Length - 1; i >= 0; --i) // Traversing from the end of the buffer for performance reasons.
        {
            Debug.Log($"Entity {entries[i].Value.Activator.Index} is making me purr! Purrrrrrrr!");
        }
    })
    .WithName("CatEntryJob")
    .ScheduleParallel();
```

If another entity enters the `Cat`'s bounds, purring ensues.

### Exits

To handle exits, add this block to the `OnUpdate` method:

```csharp
Entities // Example handling of the spatial exit buffer.
    .WithAll<Cat, SpatialTrigger, PhysicsCollider>()
    .WithChangeFilter<SpatialExit>() // Allows us to process (only) new exits once.
    .ForEach((in DynamicBuffer<SpatialExit> exits) => // Do NOT modify the buffer, hence the in keyword.
    {
        for (var i = exits.Length - 1; i >= 0; --i) // Traversing from the end of the buffer for performance reasons.
        {
            Debug.Log($"Entity {exits[i].Value.Activator.Index} making me meow for attention! MEEEOWWWWWWW!");
        }
    })
    .WithName("CatExitJob")
    .ScheduleParallel();
```

Upon exit, the `Cat` meows for attention.

## Tips

* See the working [CatSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Stranded/Cat/CatSystem.cs) in the [containing monorepo](https://github.com/reeseschultz/ReeseUnityDemos) that this example was modified from. That system is used in the `Stranded` demo, a concrete way to start learning and using this package and others!
* If you want to handle different kinds of events per trigger, you would most likely want to check for existence of a component per entry or exit. To that end, you could use `GetComponentFromEntity`.
* It is perfectly fine for an object to be *both* a trigger and activator (even though it may belong to the same tag as itself, self-activation is **not** possible).

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer, Reese, offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever, [unless you reach an agreement with him about support...](https://reese.codes)

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
