# Reese's DOTS Navigation

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)
[![openupm](https://img.shields.io/npm/v/com.reese.nav?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.reese.nav/)

![Video of navigation agents jumping across moving surfaces.](https://raw.githubusercontent.com/reeseschultz/ReeseUnityDemos/master/Gifs/nav-moving-jump-demo.gif)

## Introduction

This is a multi-threaded navigation package using [Unity DOTS](https://unity.com/dots). It supports obstacle avoidance, terrain, agents automatically jumping between surfaces with artificial gravity, parenting of agents and surfaces for preserving local transformations, and even backward compatibility with GameObjects. It's maintained by me, [Reese Schultz](https://github.com/reeseschultz/).

## Clone (Optional)

You may want to *clone* the containing [monorepo](https://en.wikipedia.org/wiki/Monorepo) since it has demos and glue code not part of `Reese.Nav`:

```sh
git clone https://github.com/reeseschultz/ReeseUnityDemos.git
```

From the project, note, for instance, the [NavFallSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/NavFallSystem.cs) in `Assets/Scripts/Nav`, since how you want to handle falling is entirely up to you—it's not part of the core navigation code because it's too dependent on the game or simulation in question.

## Import

There are two ways to import this package into an existing Unity project, one being with [OpenUPM](https://openupm.com/) and the other via Git URL.

### OpenUPM

This requires [Node.js](https://nodejs.org/en/) `12` or greater. Just `cd` to your project's directory and run:

```sh
npx openupm-cli add com.reese.nav
```

### Git

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#nav`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#nav`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Usage at a Glance

For this navigation package, whether you're using it with GameObjects or entities, there are three concepts to be familiar with:

1. **Agent** - An actor or character that navigates. Agents are parented to surfaces.
2. **Surface** - A space for agents to navigate upon. Surfaces are parented to bases (if no explicit basis is provided, a default basis is used at the world origin).
3. **Basis** - A glorified parent transform that allows multiple surfaces to move as a whole.

### Authoring Components

1. [NavAgentAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Authoring/NavAgentAuthoring.cs) - Converts GameObjects into entities with the `NavAgent` component, and other needed components.
2. [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Authoring/NavSurfaceAuthoring.cs) - Converts GameObjects into entities with the `NavSurface` component, and other needed components.
3. [NavBasisAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Authoring/NavBasisAuthoring.cs) - Converts GameObjects into entities with the `NavBasis` component, and other needed components.

### Usage with GameObjects

To retain navigating agents *as* GameObjects, rather than converting them into entities, add the [NavAgentHybrid](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentHybrid.cs) to them *instead* of `NavAgentAuthoring`. Such hybrid agents are still able to interact with other objects with `NavSurfaceAuthoring` and `NavBasisAuthoring` components, so long as as the Conversion Mode for them is set to "Convert and Inject Game Object." FYI, `NavAgentAuthoring` works by creating an invisible entity with the `NavAgent` component in the background. The `NavAgentHybrid` updates its GameObject transform to match that of the background entity.

### Entity Components

1. [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) - A component for making entities into agents.
2. [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) - A component for making entities into entities into surfaces.
3. [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasis.cs) - A component for making entities into bases. 

## API

---

### [NavAgentHybrid](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentHybrid.cs) (exclusively for GameObjects)

#### Public Methods

| Method                     | Type      | Description                                                                 |
|----------------------------|-----------|-----------------------------------------------------------------------------|
| **`Stop`**                 | `void`    | Stops the agent from navigating (waits for jumping or falling to complete). |

#### Initialization Variables

| Variable                   | Type      | Description                                   | Default Value       |
|----------------------------|-----------|-----------------------------------------------|---------------------|
| **`JumpDegrees`**          | `float`   | The agent's jump angle in degrees.            | `45`                |
| **`JumpGravity`**          | `float`   | Artificial gravity applied to the agent.      | `200`               |
| **`JumpSpeedMultiplierX`** | `float`   | The agent's horizontal jump speed multiplier. | `1.5f`              |
| **`JumpSpeedMultiplierY`** | `float`   | The agent's vertical jump speed multiplier.   | `2`                 |
| **`TranslationSpeed`**     | `float`   | The agent's translation speed.                | `20`                |
| **`RotationSpeed`**        | `float`   | The agent's rotation speed.                   | `0.3f`              |
| **`TypeID`**               | `string`  | The agent's type.                             | `Humanoid`          |
| **`Offset`**               | `Vector3` | The agent's offset.                           | `(0, 0, 0)`         |

#### Status Variables

| Variable               | Type               | Description                                                                                                                                                                                                                                                | Default Value |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`IsWalking`**        | `bool`             | `true` if the agent is walking, `false` if not.                                                                                                                                                                                                            | `false`       |
| **`IsJumping`**        | `bool`             | `true` if the agent is jumping, `false` if not.                                                                                                                                                                                                            | `false`       |
| **`IsFalling`**        | `bool`             | `true` if the agent is falling, `false` if not.                                                                                                                                                                                                            | `false`       |
| **`IsPlanning`**       | `bool`             | `true` if the agent is planning, `false` if not.                                                                                                                                                                                                           | `false`       |
| **`IsTerrainCapable`** | `bool`             | `true` if the agent is terrain-capable, `false` if not.                                                                                                                                                                                                    | `false`       |
| **`NeedsSurface`**     | `bool`             | `true` if the agent needs a surface, `false` if not.                                                                                                                                                                                                       | `false`       |
| **`HasProblem`**       | `PathQueryStatus?` | Has a value of [PathQueryStatus](https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html) if the agent has a problem, `null` if not. Problems tend to arise to due incorrect values set in `NavConstants`, which is discussed later. | `null`        |

### Destination Variables

| Variable               | Type      | Description                                                          | Default Value |
|------------------------|-----------|----------------------------------------------------------------------|---------------|
| **`Teleport`**         | `bool`    | `true` if the agent should teleport to destinations, `false` if not. | `false`       |
| **`WorldDestination`** | `Vector3` | The agent's world destination.                                       | `(0, 0, 0)`   |

---

### [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) (exclusively for entities)

#### Initialization Variables

| Variable                   | Type     | Description                                                                                                                                                                                                                                                                                                   | Recommended Value                  |
|----------------------------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------|
| **`JumpDegrees`**          | `float`  | The agent's jump angle in degrees.                                                                                                                                                                                                                                                                            | `45`                               |
| **`JumpGravity`**          | `float`  | Artificial gravity applied to the agent.                                                                                                                                                                                                                                                                      | `200`                              |
| **`JumpSpeedMultiplierX`** | `float`  | The agent's horizontal jump speed multiplier.                                                                                                                                                                                                                                                                 | `1.5f`                             |
| **`JumpSpeedMultiplierY`** | `float`  | The agent's vertical jump speed multiplier.                                                                                                                                                                                                                                                                   | `2`                                |
| **`TranslationSpeed`**     | `float`  | The agent's translation speed.                                                                                                                                                                                                                                                                                | `20`                               |
| **`RotationSpeed`**        | `float`  | The agent's rotation speed.                                                                                                                                                                                                                                                                                   | `0.3f`                             |
| **`TypeID`**               | `int`    | This is the type of agent in terms of the NavMesh system. See examples of use in the demo spawners. There is also a helper method for setting the type from a `string` in the [NavUtil](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/NavUtil.cs) called `GetAgentType`. | `NavUtil.GetAgentType("Humanoid")` |
| **`Offset`**               | `float3` | The agent's offset.                                                                                                                                                                                                                                                                                           | `(0, 0, 0)`                        |

(See the [demo spawners](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/Spawner) for examples of initialization.)

#### Status Components & Variables

Here are the internally-managed components (defined in [NavAgentStatus](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentStatus.cs)) that are applied to [NavAgents](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) throughout the navigation lifecycle. Do **not** write to these, just query them to check existence:

| `IComponentData`      | Description                                                              |
|-----------------------|--------------------------------------------------------------------------|
| **`NavWalking`**      | Exists if the agent is walking.                                          |
| **`NavJumping`**      | Exists if the agent is jumping.                                          |
| **`NavFalling`**      | Exists if the agent is falling.                                          |
| **`NavPlanning`**     | Exists if the agent is planning.                                         |
| **`NavNeedsSurface`** | Exists if the agent needs a surface.                                     |

Other components you **may** add and write to:

| `IComponentData`          | Description                                                                                                                                                                                                                             |
|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **`NavNeedsDestination`** | Exists if the agent needs a destination. In this `struct`, there's a `float3` named `Destination` (relative to the world). There's also an optional `bool` named `Teleport`, which toggles teleportation to the provided `Destination`. |
| **`NavStop`**             | Exists if the agent needs to stop moving (waits for jumping or falling to complete).                                                                                                                                                    |
| **`NavTerrainCapable`**   | Only needed if your agents must navigate on terrain. Don't use it otherwise, since it may negatively impact performance.                                                                                                                |

(See the [demo destination systems](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/Destination) for examples of status component and variable usage.)

---

### [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs)

The [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) is much less complicated than the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs). What you'll mainly be concerned with is its associated [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurfaceAuthoring.cs) script. It has some important public variables:

| Variable                     | Type                        | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Default Value |
|------------------------------|-----------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`HasGameObjectTransform`** | `bool`                      | The GameObject's transform will be used and applied to possible children via [CopyTransformFromGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.CopyTransformFromGameObject.html?q=copytransform) if `true`, otherwise the entity's transform will be used and applied conversely via [CopyTransformToGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.CopyTransformToGameObject.html).                                                                                                                                                                                                               | `false`       |
| **`JumpableSurfaces`**       | `List<NavSurfaceAuthoring>` | A list of surfaces that are "jumpable" from *this* one. Automating what's "jumpable" is out of scope for this package, but automating jumping itself is not; thus, by using an agent-associated `parent.Value` and checking its [NavJumpableBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavJumpableBufferElement.cs) buffer, you can write code to deliberate on which surface to jump to. The [NavPointAndClickDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Destination/NavPointAndClickDestinationSystem.cs) already does this for demonstrational purposes.         | Empty         |
| **`Basis`**                  | `NavBasisAuthoring`         | The basis for a given surface. Surfaces parented to the same basis flock together.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | `null`        |

---

### [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasis.cs)

Like the [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs), you only need to know about the related [NavBasisAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasisAuthoring.cs) script. It has a couple public variables:

| Variable                           | Type                        | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Default Value |
|------------------------------------|-----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`HasGameObjectTransform`**       | `bool`                      | The GameObject's transform will be used and applied to possible children via [CopyTransformFromGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.CopyTransformFromGameObject.html?q=copytransform) if `true`, otherwise the entity's transform will be used and applied conversely via [CopyTransformToGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.CopyTransformToGameObject.html). | `false`       |
| **`ParentBasis`**                  | `NavBasisAuthoring`         | In essence, a basis can have a basis.                                                                                                                                                                                                                                                                                                                                                                                                                                       | `null`        |

---

### **IMPORTANT:** Layers & You

By default, [GameObjects](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html) with [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) components attached to them should be set to layer 28. All obstacles should be set to layer 29. Otherwise, things won't work because the navigation package depends on ray and collider casting. For more information on the layers and overriding them, see the section on settings below.

### Runtime Settings

The nav package has many settings you may override (at runtime), hence why the [NavSettingsOverrides](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/NavSettingsOverrides.cs) class is included for your convenience in the demo code. This class makes it easy to retain your changes even when you update the nav package via UPM.

If you cloned or forked the monorepo, then you already have the class. Otherwise, feel free to copy-paste its code and modify it for your preferred overrides.

Now, what settings are there to override, anyway?

Settings corresponding to the layers are as follows:

| Setting             | Type  | Description              | Default Value |
|---------------------|-------|--------------------------|---------------|
| **`SurfaceLayer`**  | `int` | The layer for surfaces.  | `28`          |
| **`ObstacleLayer`** | `int` | The layer for obstacles. | `29`          |
| **`ColliderLayer`** | `int` | The layer for colliders. | `30`          |

And then there's the rest of the settings you may want to change:

| Setting                                    | Type    | Description                                                                                                                                                                                                                                                                                                    | Default Value |
|--------------------------------------------|---------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`DestinationRateLimitSeconds`**          | `float` | Duration in seconds before a new destination will take effect after another. Prevents planning from being clogged with destinations which can then block interpolation of agents.                                                                                                                              | `0.8f`        |
| **`DestinationSurfaceColliderRadius`**     | `float` | A sphere collider of the specified radius is used to detect the destination surface.                                                                                                                                                                                                                           | `1`           |
| **`JumpSecondsMax`**                       | `float` | Upper limit on the *duration* spent jumping before the agent is actually considered falling. This limit can be reached when the agent tries to jump too close to the edge of a surface and misses.                                                                                                             | `5`           |
| **`ObstacleRaycastDistanceMax`**           | `float` | Upper limit on the raycast distance when searching for an obstacle in front of a given NavAgent.                                                                                                                                                                                                               | `1000`        |
| **`SurfaceRaycastDistanceMax`**            | `float` | Upper limit on the raycast distance when searching for a surface below a given [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs).                                                                                                               | `1000`        |
| **`PathSearchMax`**                        | `int`   | Upper limit on the search area size during path planning.                                                                                                                                                                                                                                                      | `1000`        |
| **`IterationMax`**                         | `int`   | Upper limit on the iterations performed in a [NavMeshQuery](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Experimental.AI.NavMeshQuery.html) to find a path in the [NavPlanSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavPlanSystem.cs). | `1000`        |
| **`PathNodeMax`**                          | `int`   | Upper limit on a given path buffer. Exceeding this merely results in allocation of heap memory.                                                                                                                                                                                                                | `1000`        |

### Compile-Time Constants

In addition to settings, there are also compile-time constants. You *can* change them directly in [NavConstants](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/NavConstants.cs), although that shouldn't be necessary. Plus, the constants will reset when you update via UPM.

| Constant                                   | Type     | Description                                                                                                                                                                                                                                                                                                    | Default Value |
|--------------------------------------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`SURFACE_RAYCAST_MAX`**                  | `int`    | Upper limit on the number of raycasts to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more raycasts will be performed.                                                        | `100`         |
| **`JUMPABLE_SURFACE_MAX`**                 | `int`    | Upper limit on a given jumpable surface buffer. Exceeding this merely results in allocation of heap memory.                                                                                                                                                                                                    | `30`          |
| **`HUMANOID`**                             | `string` | The 'Humanoid' NavMesh agent type as a string.                                                                                                                                                                                                                                                                 | `"Humanoid"`  |

## Tips

* Make sure you bake your [NavMeshSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs)!
* Anything with an authoring script on it also needs an accompanying [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script as well. Don't forget! The Unity Editor should warn you about that.
* The compatible version of [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) is *already* in [Packages/com.reese.nav/ThirdParty](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/)! Use that and nothing else, and I mean for your entire project. Do not try to mix and match it with other versions.
* Upon spawning [NavAgents](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs), ensure you have their initial [Translation.Value](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.Translation.html?q=translation) right, along with their `Offset`. Getting these things wrong may result in your agents being unable to raycast the surface below them, since they may be raycasting underneath it!
* Obstacles need the [NavMeshObstacle](https://docs.unity3d.com/2019.3/Documentation/Manual/class-NavMeshObstacle.html), colliders, and the [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script on them. Otherwise obstacles will not be detected by raycasts. By the way, `Carve` should be `true`.

## Credits

* The demos extensively use [Mini Mike's Metro Minis](https://mikelovesrobots.github.io/mmmm) (licensed with [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/?)) by [Mike Judge](https://github.com/mikelovesrobots). That project is embedded in this one by way of `Assets/MMMM/`. I modified its directory structure, and generated my own prefabs rather than using the included ones.
* One demo leverages animations from [Mixamo](https://www.mixamo.com) by [Adobe](https://www.adobe.com/).
* The sounds mixed in the demos are from [Freesound](https://freesound.org/); only ones licensed with [CC0](https://creativecommons.org/share-your-work/public-domain/cc0/) are used here.
* The navigation package uses [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) (licensed with [MIT](https://opensource.org/licenses/MIT)) by [Unity Technologies](https://github.com/Unity-Technologies).
* The navigation package also uses [PathUtils](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/PathUtils) (licensed with [zlib](https://opensource.org/licenses/Zlib)) by [Mikko Mononen](https://github.com/memononen), and modified by [Unity Technologies](https://github.com/Unity-Technologies). Did you know that Mikko is credited in [Death Stranding](https://en.wikipedia.org/wiki/Death_Stranding) for [Recast & Detour](https://github.com/recastnavigation/recastnavigation)?

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever. All that said, [GitHub Issues](https://github.com/reeseschultz/ReeseUnityDemos/issues/new/choose) is fine for constructive discussion.

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
