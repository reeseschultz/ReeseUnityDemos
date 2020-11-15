[![openupm](https://img.shields.io/npm/v/com.reese.nav?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.reese.nav/)

# Reese's DOTS Navigation

![Video of navigation agents jumping across moving surfaces.](https://raw.githubusercontent.com/reeseschultz/ReeseUnityDemos/master/Gifs/nav-moving-jump-demo.gif)

## Introduction

This is a multi-threaded navigation package using [Unity DOTS](https://unity.com/dots). It supports obstacle avoidance, terrain, agents automatically jumping between surfaces with artificial gravity, parenting of agents and surfaces for preserving local transformations, and even backward compatibility with GameObjects. It's maintained by me, [Reese Schultz](https://github.com/reeseschultz/).

You may want to *clone* the containing [monorepo](https://en.wikipedia.org/wiki/Monorepo) since it has demos and glue code not part of `Reese.Nav`:

## Clone (Optional)

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

For this navigation package, whether you're using it with GameObjects or entities, there are **three key components** you should be familiar with:

1. [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) - A component added to any entity you want to, well, navigate. There is a facade for this component called the [NavAgentHybrid](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentHybrid.cs) to be used exclusively with GameObjects. It works by creating an entity with the `NavAgent` component in the background. The `NavAgentHybrid` updates its GameObject transform to match that of the invisible "background" entity which drives the navigation.
2. [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) - A component added to [GameObjects](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html) during authoring that also have the [NavMeshSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs) attached. Make sure you bake your surfaces!
3. [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasis.cs) - A glorified parent transform you may attach to any arbitrary [GameObject](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html). If you don't assign your surfaces to a basis, then they will be parented to a shared default basis. A basis is normally used to translate multiple surfaces as a whole.

### A Note on Usage with GameObjects

If you want to use this package with standard GameObjects, you're in luck. Alternatively, if you want to use a skinned mesh renderer (bone-based animation), using GameObjects may be your only option until the official DOTS animation package is more mature.

Either way, the `NavHybridDemo` in `Assets/Scenes/Nav` of the containing project will illuminate the GameObject workflow. The prefab used in that scene has the `NavAgentHybrid` component attached to it. It also features sample animation controllers in [Assets/Scripts/Nav/NavHybridDemo](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/), one of which is a script that shows you how to interface with the `NavAgentHybrid` component to play animations.

Consider that, even when using GameObjects, you would still want to attach the DOTS-based surface, and ,optionally, basis code as well. For more on that, see the API section to follow.

## API

### [NavAgentHybrid](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentHybrid.cs) (simple, exclusively for GameObjects)

#### Initialization Variables

* `JumpDegrees`: `float` - The agent's jump angle in degrees. `45` is the default.
* `JumpGravity`: `float` - Artificial gravity applied to the agent. `200` is the default.
* `JumpSpeedMultiplierX`: `float` - The agent's horizontal jump speed multiplier. `1.5f` is the default.
* `JumpSpeedMultiplierY`: `float` - The agent's vertical jump speed multiplier. `2` is the default.
* `TranslationSpeed`: `float` - The agent's translation speed. `20` is the default.
* `RotationSpeed`: `float` - The agent's rotation speed. `0.3f` is the default.
* `Type`: `string` - The agent's type. `Humanoid` is the default.
* `Offset`: `Vector3` - The agent's offset. `(0, 0, 0)` is the default.

#### Status Variables

* `IsLerping`: `bool` - `true` if the agent is lerping, `false` if not.
* `IsJumping`: `bool` - `true` if the agent is jumping, `false` if not.
* `IsFalling`: `bool` - `true` if the agent is falling, `false` if not.
* `IsPlanning`: `bool` - `true` if the agent is planning, `false` if not.
* `IsTerrainCapable`: `bool` - `true` if the agent is terrain-capable, `false` if not.
* `NeedsSurface`: `bool` - `true` if the agent needs a surface, `false` if not.
* `HasProblem`: `PathQueryStatus` - Has a value of [PathQueryStatus](https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html) if the agent has a problem, `null` if not. Problems tend to arise to due incorrect values set in `NavConstants`, which is discussed later.

### Destination Variables

* `Teleport`: `bool` - `true` if the agent should teleport to destinations, `false` if not.
* `WorldDestination`: `Vector3` - The agent's world destination.

### [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) (advanced, exclusively for entities)

#### Initialization Variables

* `JumpDegrees`: `float` - The agent's jump angle in degrees. `45` is a reasonable value to try.
* `JumpGravity`: `float` - Artificial gravity applied to the agent. `200` is a reasonable value to try.
* `JumpSpeedMultiplierX`: `float` - The agent's horizontal jump speed multiplier. `1.5f` is a reasonable value to try.
* `JumpSpeedMultiplierY`: `float` - The agent's vertical jump speed multiplier. `2` is a reasonable value to try.
* `TranslationSpeed`: `float` - The agent's translation speed. `20` is a reasonable value to try.
* `RotationSpeed`: `float` - The agent's rotation speed. `0.3f` is a reasonable value to try.
* `TypeID`: `int` - This is the type of agent in terms of the NavMesh system. See examples of use in the demo spawners. There is also a helper method for setting the type from a `string` in the [NavUtil](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/NavUtil.cs) called `GetAgentType`.
* `Offset`: `float3` - The agent's offset. `(0, 0, 0)` is a reasonable value to try.

(See the [demo spawners](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/Spawner) for examples of initialization.)

#### Status Components & Variables

Here are the internally-managed components (defined in [NavAgentStatus](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentStatus.cs)) that are applied to [NavAgents](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) throughout the navigation lifecycle. Do **not** write to these, just use them for querying:

* `NavFalling` - Exists if the agent is falling.
* `NavHasProblem` - Exists if the agent has a problematic [PathQueryStatus](https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html) preventing further path planning. The navigation package will not process agents with this component. You need to handle and remove the component as needed. Problems tend to arise to due incorrect values set in `NavConstants`, which is discussed later.
* `NavJumping` - Exists if the agent is jumping.
* `NavLerping` - Exists if the agent is lerping.
* `NavNeedsSurface` - Exists if the agent needs a surface. This component should be added when spawning an agent. It's also automatically added after the agent jumps. When this component exists, the [NavSurfaceSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurfaceSystem.cs) will try to raycast for a new surface potentially underneath said agent.
* `NavPlanning` - Exists if the agent is planning paths or jumps.

You should, however, write to the following component:

* `NavNeedsDestination` - Exists if the agent needs a destination. In this `struct`, there's a `float3` named `Destination` (relative to the world). There's also an optional `bool` named `Teleport`, which toggles teleportation to the provided `Destination`.

You may *optionally* add another component to your agents when spawning:

* `NavTerrainCapable` - Only needed if your agentsmust navigate on terrain. Don't use it otherwise, since it may negatively impact performance.

(See the [demo destination systems](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/Destination) for examples of status component and variable usage.)

### [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs)

The [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) is much less complicated than the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs). What you'll mainly be concerned with is its associated [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurfaceAuthoring.cs) script. It has some important public variables:

* `HasGameObjectTransform`: `bool` - If true the GameObject's transform will be used and applied to possible children via [CopyTransformFromGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.CopyTransformFromGameObject.html?q=copytransform). If false the entity's transform will be used and applied conversely via [CopyTransformToGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.CopyTransformToGameObject.html). False by default.
* `JumpableSurfaces`: `List<NavSurfaceAuthoring>` - This is a list of surfaces that are "jumpable" from *this* one. Automating what's "jumpable" is out of scope for this package, but automating jumping itself is not; thus, by using an agent-associated `parent.Value` and checking its [NavJumpableBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavJumpableBufferElement.cs) buffer, you can write code to deliberate on which surface to jump to. The [NavPointAndClickDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Destination/NavPointAndClickDestinationSystem.cs) already does this for demonstrational purposes. Optional.
* `Basis`: `NavBasisAuthoring` - This is the basis for a given surface. Surfaces that parented to the same basis can be transformed together by said basis. Optional.

Other than that, you may have some reason to query [NavSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/Surface), but that's about it.

### [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasis.cs)

Like the [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs), you only need to know about the related [NavBasisAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasisAuthoring.cs) script. It has a couple public variables:

* `HasGameObjectTransform`: `bool` - Same deal as the [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs).
* `ParentBasis`: `NavBasisAuthoring` - In essence, a basis can have a basis.

What is the basis, exactly? It's abstract for a reason: it's a glorified parent transform. The logic is as follows: a basis can be the child of a basis; a surface can be a child of a basis; and finally, an agent can be a child of a surface. And as previously stated, the basis is optional. You don't have to set it on a [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasisAuthoring.cs) script; a default basis will be created for you.

### **IMPORTANT:** Layers & You

By default, [GameObjects](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html) with [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) components attached to them should be set to layer 28. All obstacles should be set to layer 29. Otherwise, things won't work because the navigation package depends on ray and collider casting. For more information on the layers and overriding them, see the section on constants below.

### Constants

Constants corresponding to the layers are as follows (feel free to change them as needed):

* `SURFACE_LAYER`: `int` - Default is `28`.
* `OBSTACLE_LAYER`: `int` - Default is `29`.
* `COLLIDER_LAYER`: `int` - Default is `30`.

Additionally, there are other user-facing constants you may need to change:

* `DESTINATION_SURFACE_COLLIDER_RADIUS`: `float` - A sphere collider of the specified radius is used to detect the destination surface.
* `JUMP_SECONDS_MAX`: `float` - Upper limit on the *duration* spent jumping before the agent is actually considered falling. This limit can be reached when the agent tries to jump too close to the edge of a surface and misses.
* `OBSTACLE_RAYCAST_DISTANCE_MAX`: `float` - Upper limit on the raycast distance when searching for an obstacle in front of a given NavAgent.
* `SURFACE_RAYCAST_DISTANCE_MAX`: `float` - Upper limit on the raycast distance when searching for a surface below a given [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs).
* `PATH_SEARCH_MAX`: `float` - Upper limit on the search area size during path planning.
* `SURFACE_RAYCAST_MAX`: `float` - Upper limit on the number of raycasts to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more raycasts will be performed.
* `ITERATION_MAX`: `int` - Upper limit on the iterations performed in a [NavMeshQuery](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Experimental.AI.NavMeshQuery.html) to find a path in the [NavPlanSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavPlanSystem.cs).
* `JUMPABLE_SURFACE_MAX`: `int` - Upper limit on a given jumpable surface buffer. Exceeding this will only result in heap memory blocks being allocated.
* `PATH_NODE_MAX`: `int` - Upper limit on a given path buffer. Exceeding this only result in heap memory blocks being allocated.

## Tips

* Make sure you bake your [NavMeshSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs)!
* Anything with an authoring script on it also needs an accompanying [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script as well. Don't forget! The Unity Editor should warn you about that.
* The compatible version of [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) is *already* in [Packages/com.reese.nav/ThirdParty](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/)! Use that and nothing else, and I mean for your entire project. Do not try to mix and match it with other versions.
* Upon spawning [NavAgents](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs), ensure you have their initial [Translation.Value](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Transforms.Translation.html?q=translation) right, along with their `Offset`. Getting these things wrong may result in your agents being unable to raycast the surface below them, since they may be raycasting underneath it!
* Obstacles need the [NavMeshObstacle](https://docs.unity3d.com/2019.3/Documentation/Manual/class-NavMeshObstacle.html), colliders, and the [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.11/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script on them. Otherwise obstacles will not be detected by raycasts. By the way, `Carve` should be `true`.
