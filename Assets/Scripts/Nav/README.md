# Reese's DOTS Navigation User Guide

**DISCLAIMER:** So you want to use this hacky navigation solution at your own risk? Okay, cool. Just be aware that, as soon as Unity releases an *official* alternative, I (Reese, the maintainer) will ***immediately*** update or replace all the nav code by using their solution. Same goes for this guide itself. But all won't be lost if you're insistent on using old code: it will be archived via git commits. Commits are helpful in that regard.

## Introduction & Design Philosophy

[Read the blog post](https://reeseschultz.com/dots-navigation-with-auto-jumping-agents-and-movable-surfaces/) introducing the navigation scripts and demos. Or not. Assuming you didn't, here's what this code does:

1. **Supports auto-jumping** of agents between NavMeshSurfaces with artificial gravity relative to the normal of the surfaces' parent basis.
2. **Allows surfaces to move together** by parenting them to said basis, toggling either entity transforms to GameObject representations or even the other way around.
3. Has a glorified parent, the aformentioned basis, that can be parented to another basis (and so on) to **handle complex parenting scenarios**!
4. Is **multi-threaded**.
5. Can be **drag-and-dropped** into other Unity projects, specifically the `Assets/Scripts/Nav` directory, thanks to thoughtful file hierarchy and namespace conventions. Helper glue code for navigation can be wholesale drag-and-dropped from `Assets/Scripts/Demo/Nav`.
6. **Includes multiple demo scenes** in `Assets/Scenes/Nav` so I don't forget how it works.
7. Is **extensively documented** even though I'll eventually get tired of updating the docs and then they will exist solely to confuse myself and others.
8. And **still supports good-old obstacle avoidance**, only completing a "partial path" for agents who have a destination set inside an obstacle.

It should go without saying that I care more about usability than performance. The navigation code should be reasonably easy to use and work as expected. Performance is highly important, just not as much as delivering the jumping and transform parenting features.

## Prerequisites

It goes without saying that you should understand [Unity DOTS](https://unity.com/dots). If not, I know you'll just try to use this project anyway, so *at least* [follow this 'getting started' tutorial](https://reeseschultz.com/getting-started-with-unity-dots/), and consider familiarizing yourself with the abundance of information it links to.

Provided you understand DOTS, we can do this one of two ways: the easy way, and the hard way. Which way you choose is up to you. If you have problems getting the project working the hard way, then please refrain from creating a GitHub Issue about it unless you're absolutely certain there's something wrong with this project. In my experience, my problem is usually me.

Whether we take the easy or hard way which I'll cover below, you'll want to clone my project like so:

```sh
git clone https://github.com/reeseschultz/ReeseUnityDemos.git
```

And here's something to be aware of if you encounter issues opening the project with the Unity Editor: 

>**Linux & You:** We use the [Burst compiler](https://docs.unity3d.com/Packages/com.unity.burst@1.2/manual/index.html) in *this* household, so if you are on Debian or a Debian-based distro, you may need to manually install `libncurses5`. At least, this is what I did to solve my problem with Burst on Ubuntu: `sudo apt install libncurses5`.

You may have a different problem, so be sure to actually read the error message so you can troubleshoot it. And now let's move on to *The Easy Way* and *The Hard Way*.

### The Easy Way

With this method, you'll edit the project, add and delete stuff at your leisure. This means you have a greenfield project, or you're willing to copy your existing code and assets from another project into this one. That's ideal, but if it isn't in the cards for you, then please skip to the *The Hard Way*.

Now open the project with [Unity or Unity Hub](https://unity3d.com/get-unity/download). If it says you need to download a new version of Unity, do it. Refrain from upgrading or downgrading the project with a different version of Unity. I do that for you by keeping the project as current as possible. If you insist on changing the project version, then don't even think about asking anyone for help. That's on you. If you want to upgrade it for me and submit a PR, you're welcome to do so.

Assuming you listened and just used the version of Unity the project is intended to work with (which naturally changes in the `master` branch over time), now you should be able to open it.

Anyway, go open and run the demo scenes in `Assets/Scripts/Demo/Nav`. You might have guessed those are the demos specific to navigation. You can ignore the other demo scenes outside that directory if you don't care about them.

Explore. Look at the code if you want. Try to break stuff (and figure out why it broke so you can submit a PR and fix it). When you're done, proceed to the *Usage* section below.

### The Hard Way

#### Checking Versions

Ugh. Okay. If you want to be inconvenient, then we'll move the navigation code into a different Unity project. First we need to ensure that, ideally, versions of things match exactly, or are reasonably close. To check the editor versions between your project and mine for a quick sanity check, run this:

```sh
cat ProjectSettings/ProjectVersion.txt
```

You'll see the version in the output.

Next, as for the packages:

```sh
cat Packages/manifest.json
```

If they're close enough, you may be okay. It's on you to troubleshoot getting them right, though. If my project works assuming you're running it with the intended versions of everything, and yours doesn't, it's a you-problem.

#### Upgrading and Downgrading Packages

To upgrade or downgrade things, we'll need the editor. Go to `Window ⇒ Package Manager`, then click on `Advanced ⇒ Enable Preview Packages`. Install the following packages, ensuring their versions match exactly with those of this project if possible:

1. `Burst`
2. `Entities`
3. `Hybrid Renderer`
4. `Physics` (this project is only confirmed to work with *Unity* physics; it has not been tested with Havok)

Other packages, such as `Mathematics`, should be installed automatically along with `Entities`.

#### Drag-And-Drop

You'll want to drag and drop some directories from mine to yours:

* You **must** copy over the `Assets/Scripts/Nav` directory and all its contents. Where you put it in your project shouldn't matter so long as it's under `Assets`.
* I highly **recommend** copy over the code in `Assets/Scripts/Demo/Nav`, which include additional systems and code to support the navigation demos. You may think you don't need these, but they're helpful for reference, especially the `NavFallSystem` since how you want to handle falling is entirely up to you—it's not part of the core navigation code because it's too dependent on the game or simulation in question.
* I highly **recommend** you copy over the navigation demo scenes in `Scenes/Nav`. They're only here to help and show you how to set up your scenes. You can always modify them or delete them later.

You may have encountered issues moving things over. Take your time to figure out what's going if that's the case. Basically, when you copy these files over, you shouldn't be seeing any errors unless you did something wrong, or there's a version incompatibility between packages.

## Usage

For real, this is a final request for you to clone the project and explore it in code and via the Unity Editor. A little familiarity will go a long way. Okay? Okay.

So how's this thing work? The navigation code processes entities with **three key components**:

1. `NavAgent` - A component added to any entity you want to, well, navigate.
2. `NavSurface` - A component added to GameObjects during authoring that also have the `NavMeshSurface` attached. And make sure you bake your surfaces!
3. `NavBasis` - A glorified parent transform you may attach to any arbitrary GameObject. If you don't create a basis, then one will be created by default that all of your agents will become children of by way of their presently detected surface (but more on that later).

### The `NavAgent`

FYI, I personally made  *no* effort achieving parity between Unity's concept of the `NavMeshAgent` and this one. In fact, it's expected that you do *not* use the `NavMeshAgent` component. Thus, the only authoring script related to the `NavAgent` is the `NavAgentPrefab` which is used along with the `ConvertToEntity` script to simply designate your preferred prefab for your agents. It's assumed you'll spawn them whenever, i.e. at runtime.

All that said, if the `NavMeshAgent` has features you want, but that you don't see in the `NavAgent`, then please add them to this project and submit a PR.

#### Spawn Variables

The `NavAgent` has a ton of member variables. It's the most difficult to understand. But we're mainly concerned with the user-facing variables (meaning those intended for you to use in your project, not to help develop this one). Lets start with the variables we care about while spawning agents:

* `JumpDegrees`: `float` - It's the jump angle in degrees. `45` is a reasonable value to try.
* `JumpGravity`: `float` - It's artifical gravity used specifically for the projectile motion calculations during jumping. `200` is a reasonable value to try.
* `TranslationSpeed`: `float` - This is the translation (movement) speed of the agent. `20` is a reasonable value to try.
* `TypeID`: `int` - This is the type of agent, in terms of the NavMesh system. See examples of use in the demo spawners. There is also a helper method for setting the type from a `string` in `Assets/Scripts/Nav/NavUtil.cs` called `GetAgentType`.
* `Offset`: `float3` - This is the offset of the agent from the basis. It's a `float3` and not simply a float representing the y-component from the surface, which you may find odd. But the idea here is to provide flexibility. While you may usually only set the y-component, there could be situations where you want to set x or z.

See `NavMovingJumpDemoSpawner.cs` and `NavPointAndClickDemoSpawner.cs` in `Assets/Scripts/Demo/Nav` for examples of setting such data. There's also the redundantly named `Assets/Scripts/Demo/SpawnDemoSpawner.cs`.

#### Destination Variables

To move the agent, you would need to set *either* one of these:

* `WorldDestination`: `float3` - This is a destination in *world* space.
* `LocalDestination`: `float3` - This is a destination *local* to provided destination surface of your choosing, meaning you **must** also provide the `DestinationSurface` if you use the `LocalDestination`. The `DestinationSurface` is an `Entity` that is supposed to have a `NavSurface` component on it.

#### Status-Checking Variables

To check on the agent's progress from a system of your own, making sure only to read and not write, you'll be interested in these variables:

* `HasDestination`: `bool` - Whether the agent presently has a destination. It's automatically set for you, so be sure just to read it.
* `Surface`: `Entity` - This is the presently detected `NavSurface` underneath the agent. Keep in mind that there *are* circumstances where this could be equal to `EntityNull`, like if the agent is falling (and not simply jumping). The current surface could be useful to have for a number of reasons, one of which being that you can get all of the *jumpable* surfaces from a given surface like so: `jumpableBufferFromEntity[agent.Surface]`. We'll cover jumpable surfaces soon.
* `IsLerping`: `bool` - Whether or not the agent is interpolating. You probably want to wait for the agent to finish before issuing a new destination.
* `LastDestination`: `float3` - The last destination the agent had (which is only technically valid if the agent did indeed have a prior destination). This is useful for knowing if the destination changed.

See `NavDestinationSystem.cs` and `NavPointAndClickDestinationSystem.cs` in `Assets/Scripts/Demo/Nav` for examples of reading information out of agents to determine when and how to write their destinations.

### The `NavSurface`

The `NavSurface` is easier to understand than the `NavAgent`, since it's less complicated. Primarily what you'll be concerned with is the associated `NavSurfaceAuthoring` script. It has some important public variables:

* `HasGameObjectTransform`: `bool` - **Optional, but obviously false if not manually set.** If true the GameObject's transform will be used and applied to possible children via `CopyTransformFromGameObject`. If false the entity's transform will be used and applied conversely via `CopyTransformToGameObject`.
* `JumpableSurfaces`: `List<NavSurfaceAuthoring>` - **Optional.** This is a list of surfaces that are "jumpable" from *this* one. Immense thought went into this design, and it was determined that automating what's "jumpable" is probably out of scope for this project, but not automating jumping itself. Ultimately it largely depends on the design of one's game. This means it's *entirely on you* to figure out which surfaces are "jumpable" from one another. The agent will *not* automatically know that there is a surface in-between them; however, by using `Agent.Surface` and checking its `NavJumpableBufferElement` buffer, you can write code to deliberate which surface to jump to. As an example, `Assets/Scripts/Demo/Nav/NavPointAndClickDestinationSystem` already does that.
* `Basis`: `NavBasisAuthoring` - **Optional.** This is the basis for a given surface. Agents will automatically become children of the basis of their current surface. This is what permits navigation between surfaces that share a moving basis.

Other than that, you may have some reason to query `NavSurface`s, but generally speaking you'll work with surfaces via `NavAgent.Surface` and the `NavJumpableBufferElement` (not to be confused with the `NavJumpBufferElement`, which is what's used internally for the agent to mind "jumpable" points on surfaces, *not* surfaces themselves.)

### The `NavBasis`

This is our last component to cover. Whew. It's simple. Really. Like the `NavSurface`, you only need to know about the `NavBasisAuthoring` component. It has a couple public variables:

* `HasGameObjectTransform`: `bool` - Same deal as the `NavSurface`.
* `ParentBasis`: `NavBasisAuthoring` - You can probably guess what this is as well. In essence, a basis can have a basis.

What is the basis, exactly? It's abstract for a reason: it's a glorified parent transform. The logic is as follows: a basis can be the child of a basis; a surface can be a child of a basis; an agent can be a child of a *basis*. **Note** that agents' true parents are *not* surfaces, but rather the basis of their currently detected surface. That is why an agent's `Parent` component will differ from `Agent.Surface`. And as previously stated, the basis is optional. You don't have to set it on a `NavSurfaceAuthoring` script; a default basis will be created for you.

### Constants

There are a bunch of constants in `Assets/Scripts/Nav/NavConstants.cs`. You might be interested in changing the following constants, though be aware that they directly affect the internal workings of the navigation code:

* `OBSTACLE_RAYCAST_DISTANCE_MAX`: `float` - Default is `1000`. Upper limit on the raycast distance when searching for an obstacle in front of a given NavAgent.
* `SURFACE_RAYCAST_DISTANCE_MAX`: `float` - Default is `1000`. Upper limit on the raycast distance when searching for a surface below a given NavAgent.
* `BATCH_MAX`: `int` - Default is 50. Upper limit when manually batching jobs.
* `ITERATION_MAX`: `int` - Upper limit on the iterations performed in a `NavMeshQuery` to find a path in the `NavPlanSystem`.
* `JUMPABLE_SURFACE_MAX`: `int` - Default is `30`. Upper limit on a given jumpable surface buffer. Exceeding this will merely result in heap memory blocks being allocated.
* `PATH_NODE_MAX`: `int` - Default is `1000`. Upper limit on a given path buffer. Exceeding this will merely result in heap memory blocks being allocated.
* `PATH_SEARCH_MAX` - Default is `1000`. Upper limit on the search area size during path planning.
* `SURFACE_RAYCAST_MAX` - Default is `100`. Upper limit on the number of raycasts to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more raycasts will be performed.

Some of these constants may be absorbed into the `NavAgent` itself as individual settings later.

## Conclusion

That's it. The navigation code is subject to change at any time, and if it does, it's to help you, not confuse you. I'll keep the guide updated to ensure it's current and hopefully understandable. If you have any improvements for it, feel free to update it and submit a PR.

## Warnings Masquerading as Useful Tips

* Make sure you bake your `NavMeshSurfaces`! The weirdest problems have to do with not having the surfaces baked.
* Anything with an authoring script on it also needs an accompanying `ConvertToEntity` script as well. Don't forget! The Unity Editor should warn you about that if you forget.
* The compatible version of [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) is *already* in `Assets/Scripts/Nav/ThirdParty/NavMeshComponents`! Use that and nothing else, and I mean for your entire project. Do not try to mix and match it with other versions.
* Upon spawning `NavAgent`s, ensure you have their initial `Translation` component right, along with their `Offset`. Getting these things wrong may result in your agents being unable to raycast the surface below them, since they may be underneath it!
* Obstacles need the `NavMeshObstacle`, colliders, and the `ConvertToEntity` script on them. Otherwise obstacles will not be detected. By the way, `Carve` should be `true`.
