# Reese's DOTS Navigation

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)
[![openupm](https://img.shields.io/npm/v/com.reese.nav?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.reese.nav/)

## Introduction

This is a multi-threaded navigation package using [Unity DOTS](https://unity.com/dots). It supports obstacle avoidance, terrain, agents automatically jumping between surfaces with artificial gravity, parenting of agents and surfaces for preserving local transformations, flocking behaviors (cohesion, alignment & separation) and even backward compatibility with GameObjects. It's maintained by me, [Reese](https://github.com/reeseschultz/). [0x6c23](https://github.com/0x6c23), Dennis, contributed the flocking feature.

If you don't want all the extra bells and whistles, such as surface management and jumping, please see the [pathing package](https://github.com/reeseschultz/ReeseUnityDemos/tree/path#reeses-dots-pathing) instead.

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

1. **Agent** - An actor or character that navigates. The package automatically parents agents to surfaces.
2. **Surface** - A space for agents to navigate upon. Surfaces are parented to bases (if no explicit basis is provided, a default basis is used at the world origin).
3. **Basis** - A glorified parent transform that allows multiple surfaces to move as a whole.

### Authoring Components

1. `NavAgentAuthoring` - Converts GameObjects into entities with the `NavAgent` component, and other needed components.
2. `NavFlocking` - Optional component one may add to agents to achieve flocking.
3. `NavSurfaceAuthoring` - Converts GameObjects into entities with the `NavSurface` component, and other needed components.
4. `NavBasisAuthoring` - Converts GameObjects into entities with the `NavBasis` component, and other needed components.

### Usage with GameObjects

To retain navigating agents *as* GameObjects, rather than converting them into entities, add the `NavAgentHybrid` to them *instead* of `NavAgentAuthoring`. Such hybrid agents are still able to interact with other objects with `NavSurfaceAuthoring` and `NavBasisAuthoring` components, so long as as the Conversion Mode for them is set to "Convert and Inject Game Object." FYI, `NavAgentAuthoring` works by creating an invisible entity with the `NavAgent` component in the background. The `NavAgentHybrid` updates its GameObject transform to match that of the background entity.

### Entity Components

1. `NavAgent` - A component for making entities into agents.
2. `NavSurface` - A component for making entities into surfaces.
3. `NavBasis` - A component for making entities into bases. 

## API

---

### `NavAgentHybrid` (exclusively for GameObjects)

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
| **`ShouldFlock`**      | `bool`             | `true` if the agent should flock, `false` if not.                                                                                                                                                                                                          | `false`       |
| **`NeedsSurface`**     | `bool`             | `true` if the agent needs a surface, `false` if not.                                                                                                                                                                                                       | `false`       |
| **`HasProblem`**       | `PathQueryStatus?` | Has a value of [PathQueryStatus](https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html) if the agent has a problem, `null` if not. Problems tend to arise to due incorrect values set in `NavConstants`, which is discussed later. | `null`        |

### Destination Variables

| Variable                | Type         | Description                                                                                                                                                                     | Default Value |
|-------------------------|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`Teleport`**          | `bool`       | `true` if the agent should teleport to destinations, `false` if not.                                                                                                            | `false`       |
| **`WorldDestination`**  | `Vector3`    | The agent's world destination.                                                                                                                                                  | `(0, 0, 0)`   |
| **`FollowTarget`**      | `GameObject` | Set if this agent should follow another GameObject with a NavAgentHybrid component.                                                                                             | `null`        |
| **`FollowMaxDistance`** | `float`      | Maximum distance before this agent will stop following the target Entity. If less than or equal to zero, this agent will follow the target Entity no matter how far it is away. | `0`           |
| **`FollowMinDistance`** | `float`      | Minimum distance this agent maintains between itself and the target Entity it follows.                                                                                          | `0`           |

---

### `NavAgent` (exclusively for entities)

#### Initialization Variables

| Variable                   | Type     | Description                                                                                                                                                                                                    | Recommended Value                  |
|----------------------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------|
| **`JumpDegrees`**          | `float`  | The agent's jump angle in degrees.                                                                                                                                                                             | `45`                               |
| **`JumpGravity`**          | `float`  | Artificial gravity applied to the agent.                                                                                                                                                                       | `200`                              |
| **`JumpSpeedMultiplierX`** | `float`  | The agent's horizontal jump speed multiplier.                                                                                                                                                                  | `1.5f`                             |
| **`JumpSpeedMultiplierY`** | `float`  | The agent's vertical jump speed multiplier.                                                                                                                                                                    | `2`                                |
| **`TranslationSpeed`**     | `float`  | The agent's translation speed.                                                                                                                                                                                 | `20`                               |
| **`RotationSpeed`**        | `float`  | The agent's rotation speed.                                                                                                                                                                                    | `0.3f`                             |
| **`TypeID`**               | `int`    | This is the type of agent in terms of the NavMesh system. See examples of use in the demo spawners. There is also a helper method for setting the type from a `string` in the `NavUtil` called `GetAgentType`. | `NavUtil.GetAgentType("Humanoid")` |
| **`Offset`**               | `float3` | The agent's offset.                                                                                                                                                                                            | `(0, 0, 0)`                        |
| **`CohesionPerceptionRadius`**  | `float` | The perception radius for the cohesion flocking behavior.                                                                                                                                                  | `1.5f or radius of agent plus some`|
| **`AlignmentPerceptionRadius`** | `float` | The perception radius for the alignment flocking behavior.                                                                                                                                                 | `1.5f or radius of agent plus some`|
| **`SeparationPerceptionRadius`**| `float` | The perception radius for the separation flocking behavior.                                                                                                                                                | `1.5f or radius of agent plus some`|
| **`ObstacleAversionDistance`**  | `float` | Distance from which agents start to steer away from obstacles..                                                                                                                                            | `3.0f or double/ triple the radius of the agent`|
| **`AgentAversionDistance`**     | `float` | Distance from which agents start to steer away from other agents.                                                                                                                                          | `3.0f or double/ triple the radius of the agent`|

(See the [demo spawners](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/Spawner) for examples of initialization.)

#### Status Components & Variables

Here are the internally-managed components (defined in `NavAgentStatus`) that are applied to `NavAgent`s throughout the navigation lifecycle. Do **not** write to these, just query them to check existence:

| `IComponentData`      | Description                                                              |
|-----------------------|--------------------------------------------------------------------------|
| **`NavWalking`**      | Exists if the agent is walking.                                          |
| **`NavJumping`**      | Exists if the agent is jumping.                                          |
| **`NavFalling`**      | Exists if the agent is falling.                                          |
| **`NavPlanning`**     | Exists if the agent is planning.                                         |
| **`NavNeedsSurface`** | Exists if the agent needs a surface.                                     |
| **`NavSteering`**     | Holds the steering data for the agent.                                   |

Other components you **may** add and write to:

| `IComponentData`          | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **`NavDestination`** | Exists if the agent needs a destination. In this `struct`, there's a `float3` named `Destination` (relative to the world). There's also an optional `bool` named `Teleport`, which toggles teleportation to the provided `Destination`. Additionally, the `CustomLerp` property, if `true`, disables lerping by the navigation package so that it can be handled by user code (the `NavCustomLerping` component will exist on the agent if planning is done and custom lerping is required).             |
| **`NavFollow`**           | Exists if the agent is following an entity. One important property is the `Entity` `Target`, which is self-explanatory. There's also the `float` `MaxDistance`, which is the maximum distance before this agent will stop following the target entity. If `MaxDistance` is less than or equal to zero, this agent will follow the target entity no matter how far it is away. Finally, the `float` `MinDistance` is that which the agent maintains between itself and the target entity it follows. |
| **`NavStop`**             | Exists if the agent needs to stop moving (waits for jumping or falling to complete).                                                                                                                                                                                                                                                                                                                                                                                                                |
| **`NavTerrainCapable`**   | Only needed if your agents must navigate on terrain. Don't use it otherwise, since it may negatively impact performance.                                                                                                                                                                                                                                                                                                                                                                            |
| **`NavFlocking`**         | Exists if flocking behaviours should be applied to the agent.            |

(See the [demo destination systems](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/Destination) for examples of status component and variable usage.)

---

### `NavSurface`

The `NavSurface` is much less complicated than the `NavAgent`. What you'll mainly be concerned with is its associated `NavSurfaceAuthoring` script. It has some important public variables:

| Variable                     | Type                        | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Default Value |
|------------------------------|-----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`HasGameObjectTransform`** | `bool`                      | The GameObject's transform will be used and applied to possible children via [CopyTransformFromGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Transforms.CopyTransformFromGameObject.html?q=copytransform) if `true`, otherwise the entity's transform will be used and applied conversely via [CopyTransformToGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Transforms.CopyTransformToGameObject.html).                                                                               | `false`       |
| **`JumpableSurfaces`**       | `List<NavSurfaceAuthoring>` | A list of surfaces that are "jumpable" from *this* one. Automating what's "jumpable" is out of scope for this package, but automating jumping itself is not; thus, by using an agent-associated `parent.Value` and checking its `NavJumpableBufferElement` buffer, you can write code to deliberate on which surface to jump to. The [NavPointAndClickDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Destination/NavPointAndClickDestinationSystem.cs) already does this for demonstrational purposes. | Empty         |
| **`Basis`**                  | `NavBasisAuthoring`         | The basis for a given surface. Surfaces parented to the same basis flock together.                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | `null`        |

---

### `NavBasis`

Like the `NavSurface`, you only need to know about the related `NavBasisAuthoring` script. It has a couple public variables:

| Variable                           | Type                        | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Default Value |
|------------------------------------|-----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`HasGameObjectTransform`**       | `bool`                      | The GameObject's transform will be used and applied to possible children via [CopyTransformFromGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Transforms.CopyTransformFromGameObject.html?q=copytransform) if `true`, otherwise the entity's transform will be used and applied conversely via [CopyTransformToGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Transforms.CopyTransformToGameObject.html). | `false`       |
| **`ParentBasis`**                  | `NavBasisAuthoring`         | In essence, a basis can have a basis.                                                                                                                                                                                                                                                                                                                                                                                                                                       | `null`        |

---

### **IMPORTANT:** Layers & You

By default, GameObjects with `NavSurface` components attached to them should be set to layer 28. All obstacles should be set to layer 29. Otherwise, things won't work because the navigation package depends on ray and collider casting. For more information on the layers and overriding them, see the section on settings below.

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

| Setting                                    | Type    | Description                                                                                                                                                                                         | Default Value |
|--------------------------------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`DestinationRateLimitSeconds`**          | `float` | Duration in seconds before a new destination will take effect after another. Prevents planning from being clogged with destinations which can then block interpolation of agents.                   | `0.8f`        |
| **`DestinationSurfaceColliderRadius`**     | `float` | A sphere collider of the specified radius is used to detect the destination surface.                                                                                                                | `1`           |
| **`JumpSecondsMax`**                       | `float` | Upper limit on the *duration* spent jumping before the agent is actually considered falling. This limit can be reached when the agent tries to jump too close to the edge of a surface and misses.  | `5`           |
| **`ObstacleRaycastDistanceMax`**           | `float` | Upper limit on the raycast distance when searching for an obstacle in front of a given NavAgent.                                                                                                    | `1000`        |
| **`SurfaceRaycastDistanceMax`**            | `float` | Upper limit on the raycast distance when searching for a surface below a given `NavAgent`.                                                                                                          | `1000`        |
| **`StoppingDistance`**                     | `float` | Stopping distance of an agent from its destination.                                                                                                                                                 | `1`           |
| **`PathSearchMax`**                        | `int`   | Upper limit on the search area size during path planning.                                                                                                                                           | `1000`        |
| **`IterationMax`**                         | `int`   | Upper limit on the iterations performed in a [NavMeshQuery](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Experimental.AI.NavMeshQuery.html) to find a path in the `NavPlanSystem`. | `1000`        |
| **`PathNodeMax`**                          | `int`   | Upper limit on a given path buffer. Exceeding this merely results in allocation of heap memory.                                                                                                     | `1000`        |
| **`SeparationWeight`**                     | `float` | The weight of separation in the flocking system. Pushes agents back once they get too close to another.                                                                                             | `2f`          |
| **`AlignmentWeight`**                      | `float` | The weight of alignment in the flocking system.                                                                                                                                                     | `1f`          |
| **`CohesionWeight`**                       | `float` | The weight of cohesion in the flocking system.                                                                                                                                                      | `1f`          |
| **`AgentCollisionAvoidanceStrength`**      | `float` | The strength of steering applied when agents steer away from each other.                                                                                                                            | `0.5f`        |
| **`ObstacleCollisionAvoidanceStrength`**   | `float` | The strength of steering applied when agents steer away from obstacles.                                                                                                                             | `5f`          |
| **`CollisionCastingAngle`**                | `float` | The (half) angle in which raycasts are being projected for the collision system. The direction is the entities forward vector.                                                                      | `65f`         |



### Compile-Time Constants

In addition to settings, there are also compile-time constants. You *can* change them directly in `NavConstants`, although that *usually* shouldn't be necessary. Plus, the constants will reset when you update via UPM.

| Constant                                   | Type     | Description                                                                                                                                                                                                                                             | Default Value |
|--------------------------------------------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`SURFACE_RAYCAST_MAX`**                  | `int`    | Upper limit on the number of raycasts to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more raycasts will be performed. | `100`         |
| **`JUMPABLE_SURFACE_MAX`**                 | `int`    | Upper limit on a given jumpable surface buffer. Exceeding this merely results in allocation of heap memory.                                                                                                                                             | `30`          |
| **`HUMANOID`**                             | `string` | The 'Humanoid' NavMesh agent type as a string.                                                                                                                                                                                                          | `"Humanoid"`  |

## Tips

* Make sure you bake your `NavMeshSurfaces`!
* Anything with an authoring script on it also needs an accompanying [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script as well. Don't forget! The Unity Editor should warn you about that.
* The compatible version of [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) is *already* in [Packages/com.reese.nav/ThirdParty](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/)! Use that and nothing else, and I mean for your entire project. Do not try to mix and match it with other versions.
* Upon spawning `NavAgents`, ensure you have their initial [Translation.Value](https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Transforms.Translation.html?q=translation) right, along with their `Offset`. Getting these things wrong may result in your agents being unable to raycast the surface below them, since they may be raycasting underneath it!
* Obstacles need [NavMeshObstacle](https://docs.unity3d.com/2019.3/Documentation/Manual/class-NavMeshObstacle.html) components, colliders, and the [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script on them. Otherwise obstacles will not be detected by raycasts.
* If you want to use the flocking system, you need to add the `NavFlocking` component to your agents.

## Credits

* The `Stranded` demo extensively uses [Mini Mike's Metro Minis](https://mikelovesrobots.github.io/mmmm) (licensed with [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/?)) by [Mike Judge](https://github.com/mikelovesrobots). That project is embedded in this one by way of `Assets/MMMM/`. Its directory structure was modified, and new prefabs were generated for it rather than using the included ones.
* The sounds mixed in the `Stranded` demo are from [Freesound](https://freesound.org/); only ones licensed with [CC0](https://creativecommons.org/share-your-work/public-domain/cc0/) are used here.
* The `NavHybridDemo` leverages animations from [Mixamo](https://www.mixamo.com) by [Adobe](https://www.adobe.com/).
* The navigation package uses [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) (licensed with [MIT](https://opensource.org/licenses/MIT)) by [Unity Technologies](https://github.com/Unity-Technologies); this means, for example, that runtime baking is supported, but just from the main thread.
* The navigation package uses [PathUtils](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/PathUtils) (licensed with [zlib](https://opensource.org/licenses/Zlib)) by [Mikko Mononen](https://github.com/memononen), and modified by [Unity Technologies](https://github.com/Unity-Technologies). Did you know that Mikko is credited in [Death Stranding](https://en.wikipedia.org/wiki/Death_Stranding) for [Recast & Detour](https://github.com/recastnavigation/recastnavigation)?

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever. All that said, [GitHub Issues](https://github.com/reeseschultz/ReeseUnityDemos/issues/new/choose) is fine for constructive discussion.

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
