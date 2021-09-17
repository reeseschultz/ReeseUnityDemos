# Reese's Entity Prefab Groups

Create and reference groups of entity prefabs with ease.

<details>
<summary>Import</summary>

Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#epg`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#epg`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.
</details>

<details>
<summary>Usage</summary>

This package lets you create entity prefab groups—groups of prefabs converted into entities.

A group is great for organizational purposes. Perhaps all weapons belong in the `Weapons` group, or maybe you'll have a `Characters` group for your players and NPCS. What's more, this package enables you to get a `NativeList` of all prefab entities belonging to a given group at runtime. One good reason to use this package is if you want to select a random prefab from a group of variants.

To get started, create an empty GameObject in a scene.

Next, add a `PrefabEntityGroup` component to said GameObject. Doing so will automatically add the `ConvertToEntity` script to this GameObject with the *Convert And Destroy* conversion mode, which is what you want.

Add prefabs to the group's prefab list. Your prefabs and their children do *not* need `ConvertToEntity` scripts on them; however, you *can* supply authoring (`IConvertGameObjectToEntity`) scripts for them. This is recommended so that they have queryable, custom components attached to their entity conversions. Unity's existing procedures will take care of references between prefab members and GameObjects that are converted into entities, making it easy to read/write multiple entities with one "controller" component in a system.

Anyway, after adding your prefabs to that list, you can now do something like this:

```csharp
using Reese.EntityPrefabGroups;
using Unity.Entities;
using Unity.Collections;

public class SomeSystem : SystemBase
{
    EntityPrefabSystem prefabSystem => World.GetOrCreateSystem<EntityPrefabSystem>();

    protected override void OnUpdate()
    {
        // Get a single entity prefab by a (presumably) unique, per-prefab component type:
        var donutShop = prefabSystem.GetPrefab(typeof(DonutShop));

        // The above assumes that a DonutShop authoring script is attached to a prefab.
        // Entity.Null is returned if no prefab is found.

        ...

        // Get a NativeList of entity prefabs by a (presumably) unique, per-group component type:
        var hairStyles = prefabSystem.GetGroup(typeof(HairStyle), Allocator.TempJob);

        // The above assumes that a HairStyle authoring script is attached to the group.
        // An empty NativeList is returned if the group cannot be found.
        // Keep in mind that the allocator is dependent on the calling context.

        ...

        hairStyles.Dispose(); // Don't forget to dispose the collection!
    } 
}
```
</details>

<details>
<summary>Why?</summary>

The Entity Prefab Groups package aims to improve your developer experience with improved simplicity, readability, and performance.

If you're familiar with the `Entities` package, then you know that a conventional way of getting a prefab would be like so:

```csharp
var somePrefab = EntityManager.CreateEntityQuery(
    typeof(SomeComponent),
    typeof(Prefab)
).GetSingleton<SomeComponent>().Value;
```

Of course, if you knew to do *that* specifically, then you would know that the `Prefab` component is a special one that is added to prefabs following conversion. You might also know that `GetSingletonEntity()` doesn't work in some situations, at least not in `Entities 0.17`. This is a roundabout way of saying that the convention is not idiomatic. Adding insult to injury, it's not especially readable, and it requires a query to be defined and executed.

As for groups of prefabs, the conventional way to get them is with a query similar to the above, returning a `NativeContainer` that may be passed into a job, and disposed of afterward. Each individual prefab may have a unique component in the group, in addition to being composed with a component tag symbolizing group membership—which would be used in the query. This is reasonable, but it requires manually adding the group authoring component to each prefab, in addition to any others.

This package departs from all of that aforementioned complexity.
</details>

<details>
<summary>Behind the Scenes</summary>

The `EntityPrefabSystem` stores and fetches all individual prefabs with a `NativeHashMap`. The `NativeMultiHashmap`, alternatively, is used to store and fetch groups. Theoretically, no querying means increased lookup speed. Both data structures use the `ComponentType` as keys. Since the system does not know for sure which components identify prefabs and groups, it may waste an insignificant amount of memory relating components to prefabs and groups that are not uniquely-identifying—this is the only downside, which is already mitigated to some extent, because various built-in components are culled.
</details>

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer, Reese, offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever, [unless you reach an agreement with him about support.](https://reese.codes)

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
