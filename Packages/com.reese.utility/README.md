# Reese's Utility Code

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)

General utility code for Unity, mainly DOTS-oriented.

## Support

Need support or software customization? [Read more about consulting with the maintainer, Reese, and other services he provides...](https://reese.codes)

## Import

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#utility`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#utility`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Root

Just by adding the utility package to your project, any entity instantiated from a prefab will possess a `Root` component. Its singular property is the root entity of said prefab. This is extremely useful when you use complex prefabs, because component changes on the root entity tend to have implications for children in game logic. While it is possible to check for parents and their components repeatedly from the children, that is time-consuming and error-prone. Plus, the `Root` ends up being a standard expectation of *any* child entity. `Root`s are added by the `RootSystem` when a parent with a `LinkedEntityGroup` changes.

Note that a root entity is its own root.

## Transform Extensions

### FixTranslation

`FixTranslation` is a component added to entities as a hack to correct their translation when they are converted from GameObjects during authoring, specifically when 1) one of these entities will become a child of another in the process, and 2) the parent is not at the world origin. Otherwise the `Translation` is wrong. The `FixTranslationSystem` processes entities with the `FixTranslation` component, removing it after the correction is made.

### Sticky

The `Sticky` component is used to "stick" entities to others via collider-casting and subsequent parenting. This component has these properties:

| Property             | Type              | Description                                                                                                                         |
|----------------------|-------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| **`Filter`**         | `CollisionFilter` | The collision filter to use.                                                                                                        |
| **`WorldDirection`** | `float3`          | The world direction unit vector in which collider-casting occurs to stick the attached entity.                                      |
| **`Radius`**         | `float`           | Radius of collider-casting `SphereGeometry` used to stick this entity to another.                                                   |
| **`StickAttempts`**  | `int`             | Number of attempts the `StickySystem` has to stick the object. The `StickyFailed` component will be added to it in case of failure. |

The properties in the `Sticky` component directly correspond to what you'll find in `StickyAuthoring`, which is included for convenience.

As the above table alludes, the `StickySystem` collider-casts a `SphereGeometry` with the provided `Radius` in the given `WorldDirection`; that is to attempt a stick. This is tried for `StickAttempts` before the `StickyFailed` component is added to the entity. User code must handle failure if needed. Otherwise, a successful stick will result in `Sticky` simply being removed from the entity, and its `Parent` being set appropriately.

`StickyAuthoring` conveniently adds `FixTranslation` by default.

### Parent & Child

`ParentAuthoring` and `ChildAuthoring` are provided to explicity set parent-child relationships, while in authoring, that will transfer to the entity representation at runtime.

Generally speaking, you should prefer a subscene approach, but unfortunately that is sometimes impossible when `GameObjectConversionSystem.GetPrimaryEntity` is used in other authoring scripts. The demos in the [containing monorepo](https://github.com/reeseschultz/ReeseUnityDemos) suffer from this problem since lists of GameObjects must be converted into buffers of entities during authoring. All that said, `Sticky` and `StickyAuthoring` seems to work better in the demos, so you may prefer it over these authoring scripts.

`ChildAuthoring` conveniently adds `FixTranslation` by default.

## Other Utility Code

### RemoveRenderer

The `RemoveRendererAuthoring` script simply removes the `Renderer` from a converted *and* injected GameObject, in case the GameObject needs to continue to exist at runtime, but you want it to be invisible.

### Util

Ah, yes, the redundantly-named `Utility.Util` static class of, you guessed it, static grab-bag functions. These include:

| Function             | Type              | Description                                                                                                                                                                                                                                                                                                             |
|----------------------|-------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **MultiplyPoint3x4** | `float3`          | Technically this is an extension method that transforms a point (equivalent to `Matrix4x4.MultiplyPoint3x4`, but uses Unity.Mathematics).                                                                                                                                                                               |
| **ToBitMask**        | `uint`            | Converts the layer to a bit mask. Valid layers range from 8 to 30, inclusive. All other layers are invalid, and will always result in layer 8, since they are used by Unity internally. See https://docs.unity3d.com/Manual/class-TagManager.html and https://docs.unity3d.com/Manual/Layers.html for more information. |
| **InvertBitMask**    | `uint`            | Inverts a bit mask, meaning that it applies to all layers *except* for the one expressed in said mask.                                                                                                                                                                                                                  |

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer, Reese, offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever, [unless you reach an agreement with him about support...](https://reese.codes)

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
