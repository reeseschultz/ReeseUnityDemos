# Reese's Entity Prefab Groups

Create and reference groups of entity prefabs with ease.

## Import

This package depends on Chris Foulston's [Reorderable List](https://github.com/cfoulston/Unity-Reorderable-List).

First, import that dependency by copying one of the below Git URLs:

* **HTTPS:** `https://github.com/cfoulston/Unity-Reorderable-List.git#ac1f508240fb752b1f9d7c222c231d7f2a33df87`
* **SSH:** `git@github.com:cfoulston/Unity-Reorderable-List.git#ac1f508240fb752b1f9d7c222c231d7f2a33df87`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

Next, do the same for one of the Git URLs corresponding to *this* package:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#epg`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#epg`

Would you like the import process to be simpler? The maintainer of this package certainly would. Consider asking Unity to support Git URLs in package manifests [here](https://forum.unity.com/threads/custom-package-with-git-dependencies.628390/).

## Usage

You start by creating an entity prefab group—a group of prefabs converted into entities.

First, create an empty GameObject in the current scene. That GameObject's name will be the name of your entity prefab group, so choose wisely. A group is great for variants. Perhaps all weapons belong in the `Weapons` group. Maybe you'll have a `Characters` group for your players and NPCS. This package enables you to get a `NativeList` of all prefab entities belonging a given group at runtime, even in a Burst-compiled job, so knowing that may affect your prefab organizational structure. For example, you may want to randomize selection of prefabs from a group for spawning.

Next, add a `PrefabEntityGroup` component to said GameObject. Doing so will automatically add the `ConvertToEntity` script to this GameObject with the *Convert And Destroy* conversion mode, which is what you want.

Drag-and-drop prefabs into the group's dedicated list. Note: your prefabs and their children do *not* need `ConvertToEntity` scripts on them; however, you *can* supply authoring (`IConvertGameObjectToEntity`) scripts for them. This is recommended so that they have queryable, custom components attached to their converted entities. Unity's existing conversion will take care of references between prefab members and GameObjects that are converted into entities, making it easy to orchestrate multiple entities from one "controller" component in a system.

Anyway, when you add your prefabs to that list, that's where the automagic begins...

Why? Because now you can do this:

```csharp
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Reese.EntityPrefabGroups;

public class SomeSystem : SystemBase
{
    EntityPrefabSystem prefabSystem => World.GetOrCreateSystem<EntityPrefabSystem>();

    protected override void OnUpdate()
    {
        // Get an entity prefab by name:
        if (!prefabSystem.TryGet(SceneName.GroupName.PrefabName, out Entity prefab)) return;

        // Get a group of entity prefabs by group name:
        if (!prefabSystem.TryGet(nameof(SceneName.GroupName), out NativeList<Entity> prefabs))
        {
            prefabs.Dispose();
            return;
        }

        // TODO: Update API to get group names as FixedString128!!!

        ...

        prefabs.Dispose();
    } 
}
```

## Behind the Scenes

Whenever you add or remove prefabs from a group, or rename a group, this package will generate helper classes in `Assets/~EntityPrefabGroups`, further organized by scene and group name. You can largely ignore that directory, because this package manages it for you through editor scripts.

Still, if you create a group called `Buildings`, the output is `Assets/~EntityPrefabGroups/{ACTIVE_SCENE_NAME}/Buildings.cs`. In that file, a public static class will be defined called `Buildings`. If, in that group, you have a prefab called `Donut Shop` and another called `Haunted House`, there will be strings defined for the class, represented by the `FixedString128` type, of name *and* value `DonutShop` and `HauntedHouse`. (Whitespace is trimmed, among other things.)

Why `FixedString128`? Because it's big enough to store any ridiculous prefab name you should throw at it, small enough that thousands of them amount to mere kilobytes, and it's a [blittable](https://en.wikipedia.org/wiki/Blittable_types) type that can be used in Burst-compiled jobs.

## Invariants

TODO

## Having Problems?

TODO

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer, Reese, offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever, [unless you reach an agreement with him about support.](https://reese.codes)

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
