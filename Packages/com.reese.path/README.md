# Reese's DOTS Pathing

## Introduction

This was forked from the [navigation package](https://github.com/reeseschultz/ReeseUnityDemos/tree/nav#reeses-dots-navigation), and stripped of everything except what's required for setting destinations and pathing. Thus, there is **no** physics-related code (which includes surface management and obstacle avoidance), lerping, terrain support, etc. You must supply world positions and anticipate world positions as output—there is no support for local positioning, unlike the aforementioned navigation package.

Furthermore, because physics is ripped out entirely from this package, you may have to do more to validate correctness of the positions you supply. Remember, unlike the navigation package, this one does not raycast underneath agents to check for a potential surface.

And unlike the navigation package, which includes [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) by default, you must install it yourself for this package to work properly.

By the way, this package is maintained by me, [Reese](https://github.com/reeseschultz/).

## Import

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#path`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#path`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Usage at a Glance

For a working example of how to use this package, see these [demo scripts](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Path), which include flocking!

In this package, there's only one concept you need to be familiar with: the **agent** (technically the `PathAgent` component), which is an actor or character who is in need of path planning.

There is also an associated authoring component you may want to use: `PathAgentAuthoring`.

The flow of usage works like this: you supply a `PathDestination` component to a `PathAgent`. This package will automatically supply a `PathPlanning` component to said agent when ready. After that, the `PathPlanning` and `PathDestination` components will be removed when a path is found, or if there's a problem (if there's a problem, the `PathProblem` component will be added to the agent for you to inspect). Assuming planning is successful, the agent's path buffer (which you can access by way of the `PathBufferElement`) will be populated for you to consume. That is the buffer you may use for interpolation.

### Usage with GameObjects

To retain path-planning agents *as* GameObjects, rather than converting them into entities, add the `PathAgentHybrid` to them *instead* of `PathAgentAuthoring`. FYI, this component works by creating an invisible entity with the `PathAgent` component in the background. Even though the pathing package does not feature interpolation, the `PathAgentHybrid` updates its GameObject translation and rotation to match that of the associated entity, for your convenience.

## API

---

### `PathAgentHybrid` (exclusively for GameObjects)

#### Public Methods

#### Initialization Variables

| Variable                   | Type      | Description                                   | Default Value       |
|----------------------------|-----------|-----------------------------------------------|---------------------|
| **`TypeID`**               | `string`  | The agent's type.                             | `Humanoid`          |
| **`Offset`**               | `Vector3` | The agent's offset.                           | `(0, 0, 0)`         |

#### Status Variables

| Variable               | Type               | Description                                                                                                                                                                                                                                                | Default Value |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`IsPlanning`**       | `bool`             | `true` if the agent is planning, `false` if not.                                                                                                                                                                                                           | `false`       |
| **`HasProblem`**       | `PathQueryStatus?` | Has a value of [PathQueryStatus](https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html) if the agent has a problem, `null` if not. Problems tend to arise to due incorrect values set in `NavConstants`, which is discussed later. | `null`        |

### Destination Variables

| Variable                | Type         | Description                                                                                                                                                                     | Default Value |
|-------------------------|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`WorldDestination`**  | `Vector3`    | The agent's world destination.                                                                                                                                                  | `(0, 0, 0)`   |
| **`FollowTarget`**      | `GameObject` | Set if this agent should follow another GameObject with a PathAgentHybrid component.                                                                                             | `null`        |
| **`FollowMaxDistance`** | `float`      | Maximum distance before this agent will stop following the target Entity. If less than or equal to zero, this agent will follow the target Entity no matter how far it is away. | `0`           |
| **`FollowMinDistance`** | `float`      | Minimum distance this agent maintains between itself and the target Entity it follows.                                                                                          | `0`           |

---

### `PathAgent` (exclusively for entities)

#### Initialization Variables

| Variable                   | Type     | Description                                                                                                                                                                                                    | Recommended Value                   |
|----------------------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------|
| **`TypeID`**               | `int`    | This is the type of agent in terms of the NavMesh system.                                                                                                                                                      | `PathUtil.GetAgentType("Humanoid")` |
| **`Offset`**               | `float3` | The agent's offset.                                                                                                                                                                                            | `(0, 0, 0)`                         |

#### Status Components & Variables

Here are the internally-managed components (defined in `PathAgentStatus`) that are applied to `PathAgent`s throughout the navigation lifecycle. Do **not** write to these, just query them to check existence:

| `IComponentData`      | Description                                                              |
|-----------------------|--------------------------------------------------------------------------|
| **`PathPlanning`**     | Exists if the agent is planning.                                        |

Other components you **may** add and write to:

| `IComponentData`          | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **`PathDestination`**     | Exists if the agent needs a destination. In this `struct`, there's a self-explanatory `float3` named `WorldPoint`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| **`PathFollow`**          | Exists if the agent should follow an entity. Remember, the pathing package does not interpolate, so you're still responsible for interpolating a "follower," but this will generate destinations for said follower. One important property is the `Entity` `Target`, which is self-explanatory. There's also the `float` `MaxDistance`, which is the maximum distance before this agent will stop following the target entity. If `MaxDistance` is less than or equal to zero, this agent will follow the target entity no matter how far it is away. Finally, the `float` `MinDistance` is that which the agent maintains between itself and the target entity it follows. |

### Runtime Settings

The pathing package has many settings you may override (at runtime), hence why the [NavSettingsOverrides](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/NavSettingsOverrides.cs) class is included for your convenience in the demo code (keep in mind that while these overrides are for the navigation package, you can do something similar for pathing). This class makes it easy to retain your changes even when you update the package via UPM.

Now, what settings are there to override, anyway?

| Setting                                    | Type    | Description                                                                                                                                                                                         | Default Value |
|--------------------------------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`DestinationRateLimitSeconds`**          | `float` | Duration in seconds before a new destination will take effect after another. Prevents planning from being clogged with destinations which can then block interpolation of agents.                   | `0.8f`        |
| **`PathSearchMax`**                        | `int`   | Upper limit on the search area size during path planning.                                                                                                                                           | `1000`        |
| **`IterationMax`**                         | `int`   | Upper limit on the iterations performed in a [NavMeshQuery](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Experimental.AI.NavMeshQuery.html) to find a path in the `NavPlanSystem`. | `1000`        |

### Compile-Time Constants

In addition to settings, there are also compile-time constants. You *can* change them directly in `PathConstants`, although that *usually* shouldn't be necessary. Plus, the constants will reset when you update via UPM.

| Constant                                   | Type     | Description                                                                                                                                                                                                                                             | Default Value |
|--------------------------------------------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| **`PATH_NODE_MAX`**                        | `int`    | Upper limit on the possible path nodes available for the buffer.                                                                                                                                                                                        | `1000`        |
| **`HUMANOID`**                             | `string` | The 'Humanoid' NavMesh agent type as a string.                                                                                                                                                                                                          | `"Humanoid"`  |

## Credits

This package uses [PathUtils](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/PathUtils) (licensed with [zlib](https://opensource.org/licenses/Zlib)) by [Mikko Mononen](https://github.com/memononen), and modified by [Unity Technologies](https://github.com/Unity-Technologies). Did you know that Mikko is credited in [Death Stranding](https://en.wikipedia.org/wiki/Death_Stranding) for [Recast & Detour](https://github.com/recastnavigation/recastnavigation)?

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer, Reese, offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever, [unless you reach an agreement with him about support...](https://reese.codes)

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
