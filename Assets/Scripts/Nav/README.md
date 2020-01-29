# Reese's DOTS Navigation User Guide

![Video of navigation agents jumping across moving surfaces.](/Gifs/nav-moving-jump-demo.gif)

**DISCLAIMER:** So you want to use this hacky navigation solution at your own risk? Okay, cool. Just be aware that, as soon as Unity releases an *official* alternative, I (Reese, the maintainer) will ***immediately*** update or replace all the navigation code by leaning on their solution. Same goes for this guide itself. But all won't be lost if you're insistent on using old code: it will be archived via git commits.

## Introduction & Design Philosophy

[Read the blog post](https://reeseschultz.com/dots-navigation-with-auto-jumping-agents-and-movable-surfaces/) introducing the navigation scripts and demos. Or not. Assuming you didn't, here's what this code does:

1. **Supports auto-jumping** of agents between [NavMeshSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs) with artificial gravity relative to a shared normal.
2. **Allows surfaces to move together with agents on them** via parent transform.
3. Has a glorified parent, the so-called *basis*, that can be parented to another basis (and so on) to **handle complex parent transform scenarios** for surfaces and agents!
4. Is **multi-threaded**. 👈
5. Can be **drag-and-dropped** into other Unity projects, specifically the [Assets/Scripts/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav) directory, thanks to thoughtful file hierarchy and namespace conventions. Helper glue code for navigation can be wholesale drag-and-dropped from [Assets/Scripts/Demo/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo/Nav).
6. **Includes multiple demo scenes** in [Assets/Scenes/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav) so I don't forget how it works.
7. Is **extensively documented** even though I'll eventually get tired of updating the docs and then they will exist solely to confuse myself and others.
8. And **still provides good-old obstacle avoidance**, only completing a "partial path" for agents who have a destination set inside an obstacle.

I care more about usability than performance. The navigation code should be reasonably easy to use and work as expected. Performance is highly important, just not as much as delivering the jumping and transform parenting features.

## Prerequisites

First, it goes without saying that you should be familar with [Unity DOTS](https://unity.com/dots). If not, I know you'll just try to use this project anyway, so *at least* [follow this 'getting started' tutorial](https://reeseschultz.com/getting-started-with-unity-dots/). It will lead you to other tutorials and resources you may find useful.

Second, there are two ways we can go about this: *The Easy Way*, meaning you will build your game or simulation from a clone of *this* project; or *The Hard Way*, meaning you want to copy this project's code (and possibly demos) into another. I'll cover these two different approaches below, but you'll want to clone my project regardless:

```sh
git clone https://github.com/reeseschultz/ReeseUnityDemos.git
```

Third, ideally the project should be opened with the version of Unity it's intended for. I recommend using the [Unity Hub](https://unity3d.com/get-unity/download) to manage various editor versions.

>**Linux & You:** Using Linux and having problems opening the project? On Ubuntu I couldn't use the [Burst compiler](https://docs.unity3d.com/Packages/com.unity.burst@1.2/manual/index.html) until I manually installed `libncurses5` via: `sudo apt install libncurses5`. But it's entirely possible you're missing another library. *Read* Unity's error message to be sure.

Now for *The Easy Way* and *The Hard Way*.

### The Easy Way

This means you've a greenfield project, or you're willing to copy your existing code and assets from another project into this one. If you've been following along, then feel free to add and delete stuff at your leisure. That's it then. Project-wise, you have everything you need. Skip ahead to the *Usage* section and pat yourself on the back for not being difficult.

### The Hard Way

#### Checking Versions

Ugh. Okay. If you want to be inconvenient, then we'll move the navigation code into a different Unity project. First we need to ensure that, ideally, versions of things match exactly, or are reasonably close. To check the editor versions between your project and mine for a quick sanity check, run this from either project directory:

```sh
cat ProjectSettings/ProjectVersion.txt | grep -Po '\d{4}\..*\..*' | head -n 1
```

You'll see the version in the output.

Next, as for the packages:

```sh
cat Packages/manifest.json
```

If they're close enough, you may be okay. It's on you to troubleshoot getting them right, though. If my project works assuming you're running it with the intended versions of everything, and yours doesn't, it's a You-Problem.

#### Upgrading and Downgrading Packages

To upgrade or downgrade things, which is probably what you'll need to do, you'll have to open your project in the Unity Editor. Go to `Window ⇒ Package Manager`, then click on `Advanced ⇒ Enable Preview Packages`. Install the following packages, ensuring their versions match exactly with those of my project if possible:

1. `Burst`
2. `Entities`
3. `Hybrid Renderer`
4. `Physics` (this project is only confirmed to work with *Unity* physics; it has not been tested with Havok)

Other packages, such as `Mathematics`, should be installed automatically along with `Entities`.

#### Drag-And-Drop

You'll want to drag and drop some directories from mine to yours:

* You **must** copy over the [Assets/Scripts/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav) directory and all its contents, including `.meta` files. Where you put it in your project shouldn't matter so long as it's under `Assets`.
* I highly **recommend** copying over the code in [Assets/Scripts/Demo/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo/Nav), which include additional systems and code to support the navigation demos. You may think you don't need these, but they're helpful for reference, especially the [NavFallSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavFallSystem.cs) since how you want to handle falling is entirely up to you—it's not part of the core navigation code because it's too dependent on the game or simulation in question.
* I highly **recommend** you copy over the navigation demo scenes in [Assets/Scenes/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scenes/Nav). They're only here to help and show you how to set up your scenes. You can always modify them or delete them later.

You may have encountered issues moving things over. Take your time to figure out what's going if that's the case. Basically, when you copy these files over, you shouldn't be seeing any errors unless you did something wrong, or there's a version incompatibility between packages.

#### Be Unsafe

The navigation scripts use `unsafe` code, meaning that the compiler is unable to guarantee its safety. Thus, you need to go to *Edit* ⇒ *Project Settings* ⇒ *Player* ⇒ *Allow 'unsafe' code*. The scripts won't work if you don't opt for this setting. FYI, just because something is marked `unsafe` doesn't mean there's automatically a race condition. It means, again, that the compiler is unwilling to make any judgment on code marked as such.

> **Implementation note:** The `unsafe` code is specifically needed to orchestrate [NavMeshQueries](https://docs.unity3d.com/ScriptReference/Experimental.AI.NavMeshQuery.html) in the most insane, performant way I've ever seen, which I also happen to have figured out myself ([Reese](https://github.com/reeseschultz) 👈). Effectively, the custom `NavMeshQuerySystem` creates structs with pointers to said queries in a [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html), ordered by thread index, only upon startup. The [UnsafeUtility](https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.UnsafeUtility.html) is used extensively there and in the [NavPlanSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavPlanSystem.cs) to use the queries. The only unfortunate side effect of this is that the built-in [DisposeSentinel](https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.DisposeSentinel.html) *appears* to mistakenly believe that the queries aren't disposed properly, even though they are via pointers to them.

## Usage

So how's this thing work? The navigation code processes entities with **three key components**:

1. [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs) - A component added to any entity you want to, well, navigate.
2. [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurface.cs) - A component added to [GameObjects](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html) during authoring that also have the [NavMeshSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs) attached. And make sure you bake your surfaces!
3. [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Basis/NavBasis.cs) - A glorified parent transform you may attach to any arbitrary [GameObject](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html). If you don't create a basis, then one will be created by default that all of your agents will become children of by way of their presently detected surface (but more on that later).

### The [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs)

FYI, I personally made  *no* effort achieving parity between Unity's concept of the [NavMeshAgent](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/AI.NavMeshAgent.html) and this one. In fact, it's expected that you do *not* use the [NavMeshAgent](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/AI.NavMeshAgent.html) component. Thus, the only authoring script related to the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs) is the [NavAgentPrefab](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgentPrefab.cs) which is used along with the [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script to simply designate your preferred prefab for your agents. It's assumed you'll spawn them whenever, i.e. at runtime.

All that said, if the [NavMeshAgent](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/AI.NavMeshAgent.html) has features you want, but that you don't see in the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs), then please add them to this project and submit a PR.

#### Spawn Variables

The [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs) has bunches of member variables. We're mainly concerned with the user-facing variables (meaning those intended for you to use in your project, not to help develop this one). Lets start with the variables we care about while spawning agents:

* `JumpDegrees`: `float` - It's the jump angle in degrees. `45` is a reasonable value to try.
* `JumpGravity`: `float` - It's artifical gravity used specifically for the projectile motion calculations during jumping. `200` is a reasonable value to try.
* `TranslationSpeed`: `float` - This is the translation (movement) speed of the agent. `20` is a reasonable value to try.
* `TypeID`: `int` - This is the type of agent, in terms of the NavMesh system. See examples of use in the demo spawners. There is also a helper method for setting the type from a `string` in the [NavUtil](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/NavUtil.cs) called `GetAgentType`.
* `Offset`: `float3` - This is the offset of the agent from the basis. It's a `float3` and not simply a float representing the y-component from the surface, which you may find odd. But the idea here is to provide flexibility. While you may usually only set the y-component, there could be situations where you want to set x or z.

See the [NavMovingJumpDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavMovingJumpDemoSpawner.cs) and [NavPointAndClickDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavPointAndClickDemoSpawner.cs) in [Assets/Scripts/Demo/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo/Nav) for examples of setting such data. The redundantly named [SpawnDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/SpawnDemoSpawner.cs) in [Assets/Scripts/Demo](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo) would also be worth looking at, since it's piggybacked for navigation purposes.

#### Destination Variables

To move the agent, you would need to set *either* one of these:

* `WorldDestination`: `float3` - This is a destination in *world* space.
* `LocalDestination`: `float3` - This is a destination *local* to provided destination surface of your choosing, meaning you **must** also provide the `DestinationSurface` if you use the `LocalDestination`. The `DestinationSurface` is an [Entity](https://docs.unity3d.com/Packages/com.unity.entities@0.5/manual/entities.html?q=entity) that is supposed to have a [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurface.cs) component on it.

#### Status-Checking Variables

To check on the agent's progress from a system of your own, making sure only to read and not write, you'll be interested in these variables:

* `HasDestination`: `bool` - Whether the agent presently has a destination. It's automatically set for you, so be sure just to read it.
* `Surface`: `Entity` - This is the presently detected [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurface.cs) underneath the agent. Keep in mind that there *are* circumstances where this could be equal to [Entity.Null](https://docs.unity3d.com/Packages/com.unity.entities@0.5/manual/entities.html?q=entity), like if the agent is falling (and not simply jumping). The current surface could be useful to have for a number of reasons, one of which being that you can get all of the *jumpable* surfaces from a given surface like so: `jumpableBufferFromEntity[agent.Surface]`. We'll cover jumpable surfaces soon.
* `LastDestination`: `float3` - The last destination the agent had (which is only technically valid if the agent did indeed have a prior destination). This is useful for knowing if the destination changed.

See the [NavDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavDestinationSystem.cs) and [NavPointAndClickDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavPointAndClickDestinationSystem.cs) in [Assets/Scripts/Demo/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo/Nav) for examples of reading information out of agents to determine when and how to write their destinations.

### The [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurface.cs)

The [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurface.cs) is easier to understand than the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs), since it's less complicated. Primarily what you'll be concerned with is the associated [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurfaceAuthoring.cs) script. It has some important public variables:

* `HasGameObjectTransform`: `bool` - **Optional, but obviously false if not manually set.** If true the GameObject's transform will be used and applied to possible children via [CopyTransformFromGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.CopyTransformFromGameObject.html?q=copytransform). If false the entity's transform will be used and applied conversely via [CopyTransformToGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.CopyTransformToGameObject.html).
* `JumpableSurfaces`: `List<NavSurfaceAuthoring>` - **Optional.** This is a list of surfaces that are "jumpable" from *this* one. Immense thought went into this design, and it was determined that automating what's "jumpable" is probably out of scope for this project, but not automating jumping itself. Ultimately it largely depends on the design of one's game. This means it's *entirely on you* to figure out which surfaces are "jumpable" from one another. The agent will *not* automatically know that there is a surface in-between them; however, by using `Agent.Surface` and checking its [NavJumpableBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavJumpableBufferElement.cs) buffer, you can write code to deliberate which surface to jump to. As an example, the [NavPointAndClickDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavPointAndClickDestinationSystem.cs) already does that.
* `Basis`: `NavBasisAuthoring` - **Optional.** This is the basis for a given surface. Agents will automatically become children of the basis of their current surface. This is what permits navigation between surfaces that share a moving basis.

Other than that, you may have some reason to query [NavSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/Surface), but generally speaking you'll work with surfaces via `NavAgent.Surface` and the [NavJumpableBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavJumpableBufferElement.cs) (not to be confused with the [NavJumpBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavJumpBufferElement.cs), which is what's used internally for the agent to mind "jumpable" points on surfaces, *not* surfaces themselves.)

### The [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Basis/NavBasis.cs)

This is our last component to cover. Whew. It's simple. Really. Like the [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurface.cs), you only need to know about the related [NavBasisAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Basis/NavBasisAuthoring.cs) script. It has a couple public variables:

* `HasGameObjectTransform`: `bool` - Same deal as the [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Surface/NavSurface.cs).
* `ParentBasis`: `NavBasisAuthoring` - You can probably guess what this is as well. In essence, a basis can have a basis.

What is the basis, exactly? It's abstract for a reason: it's a glorified parent transform. The logic is as follows: a basis can be the child of a basis; a surface can be a child of a basis; an agent can be a child of a *basis*. **Note** that agents' true parents are *not* surfaces, but rather the basis of their currently detected surface. That is why an agent's [Parent](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.Parent.html?q=parent) component will differ from `Agent.Surface`. And as previously stated, the basis is optional. You don't have to set it on a [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Basis/NavBasisAuthoring.cs) script; a default basis will be created for you.

### Constants

There are a bunch of constants in, well, [NavConstants](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/NavConstants.cs). You might be interested in changing the following constants, though be aware that they directly affect the internal workings of the navigation code:

* `OBSTACLE_RAYCAST_DISTANCE_MAX`: `float` - Default is `1000`. Upper limit on the [raycast](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) distance when searching for an obstacle in front of a given NavAgent.
* `SURFACE_RAYCAST_DISTANCE_MAX`: `float` - Default is `1000`. Upper limit on the [raycast](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) distance when searching for a surface below a given [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs).
* `BATCH_MAX`: `int` - Default is 50. Upper limit when manually batching jobs.
* `ITERATION_MAX`: `int` - Upper limit on the iterations performed in a [NavMeshQuery](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Experimental.AI.NavMeshQuery.html) to find a path in the [NavPlanSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavPlanSystem.cs).
* `JUMPABLE_SURFACE_MAX`: `int` - Default is `30`. Upper limit on a given jumpable surface buffer. Exceeding this will merely result in heap memory blocks being allocated.
* `PATH_NODE_MAX`: `int` - Default is `1000`. Upper limit on a given path buffer. Exceeding this will merely result in heap memory blocks being allocated.
* `PATH_SEARCH_MAX` - Default is `1000`. Upper limit on the search area size during path planning.
* `SURFACE_RAYCAST_MAX` - Default is `100`. Upper limit on the number of raycasts to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more raycasts will be performed.

Some of these constants may be absorbed into the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs) itself as individual settings later.

## Conclusion

That's it. The navigation code is subject to change at any time, and if it does, it's to help, not confuse you. I'll keep the guide updated to ensure it's current and hopefully understandable. If you have any improvements for it, feel free to update it and submit a PR.

## Tips

* Make sure you bake your [NavMeshSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs)! The weirdest problems have to do with not having the surfaces baked.
* Anything with an authoring script on it also needs an accompanying [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script as well. Don't forget! The Unity Editor should warn you about that.
* The compatible version of [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) is *already* in [Assets/Scripts/Nav/ThirdParty](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Nav/ThirdParty/)! Use that and nothing else, and I mean for your entire project. Do not try to mix and match it with other versions.
* Upon spawning [NavAgents](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Nav/Agent/NavAgent.cs), ensure you have their initial [Translation.Value](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.Translation.html?q=translation) right, along with their `Offset`. Getting these things wrong may result in your agents being unable to raycast the surface below them, since they may be [raycasting](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) underneath it!
* Obstacles need the [NavMeshObstacle](https://docs.unity3d.com/2019.3/Documentation/Manual/class-NavMeshObstacle.html), colliders, and the [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script on them. Otherwise obstacles will not be detected by [raycasts](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts). By the way, `Carve` should be `true`.
