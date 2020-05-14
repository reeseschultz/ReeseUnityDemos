[![openupm](https://img.shields.io/npm/v/com.reese.nav?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.reese.nav/)

# Reese's DOTS Navigation

![Video of navigation agents jumping across moving surfaces.](/Gifs/nav-moving-jump-demo.gif)

**DISCLAIMER:** So you want to use this hacky navigation solution at your own risk? Okay, cool. Just be aware that, as soon as Unity releases an *official* alternative ([and they're working on one!](https://forum.unity.com/threads/dots-navigation.758690/)), I, Reese, the maintainer, will ***immediately*** update or replace all the navigation code by leaning on their solution. Same goes for this guide itself. But all won't be lost if you're insistent on using old code: it will be archived via git commits.

## Introduction & Design Philosophy

[Read the blog post](https://reeseschultz.com/dots-navigation-with-auto-jumping-agents-and-movable-surfaces/) introducing the navigation scripts and demos. Or not. Assuming you didn't, here's what this code does:

1. **Supports auto-jumping** of agents between [NavMeshSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs) with artificial gravity relative to a shared normal.
2. **Allows surfaces to move together with agents on them** via parent transform.
3. Has a glorified parent, the so-called *basis*, that can be parented to another basis (and so on) to **handle complex parent transform scenarios** for surfaces and agents!
4. Is **multi-threaded**. 👈
5. Can be **imported** into other Unity projects.
6. **Includes multiple demo scenes** in [Assets/Scenes/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav) so I don't forget how it works.
7. Is **extensively documented** even though I'll eventually get tired of updating the docs and then they will exist solely to confuse myself and others.
8. And **still provides good-old obstacle avoidance**, only completing a "partial path" for agents who have a destination set inside an obstacle.

I care more about usability than performance. The navigation code should be reasonably easy to use and work as expected. Performance is highly important, just not as much as delivering the jumping and transform parenting features.

## Prerequisites

First, you should be familar with [Unity DOTS](https://unity.com/dots). If not, I know you'll just try to use this project anyway, so *at least* [follow this 'getting started' tutorial](https://reeseschultz.com/getting-started-with-unity-dots/). It will lead you to other tutorials and resources you may find useful.

Second, clone the demo project since it has glue code not part of `Reese.Nav` that you might want to copy later:

```sh
git clone https://github.com/reeseschultz/ReeseUnityDemos.git
```

Third, ideally that project should be opened with the version of Unity it's intended for. I recommend using the [Unity Hub](https://unity3d.com/get-unity/download) to manage various editor versions.

## Import

There are two ways to import this package, one being with [OpenUPM](https://openupm.com/), the preferred method, and the other via Git URL:

### OpenUPM

This requires [Node.js](https://nodejs.org/en/) `12` or greater. `cd` to your project's directory and run:

```sh
npx openupm-cli add com.reese.nav
```

### Git

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#nav`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#nav`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

#### Drag-And-Drop

I ***highly*** recommend that you drag and drop some files from the demo project to yours:

* [Assets/Scripts/Demo/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo/Nav) - Includes additional systems and code to support the navigation demos. You may think you don't need these, but they're helpful for reference, especially the [NavFallSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavFallSystem.cs) since how you want to handle falling is entirely up to you—it's not part of the core navigation code because it's too dependent on the game or simulation in question.
* [Assets/Scenes/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scenes/Nav) - The home of the nav demo scenes. It's easier to modify these than start from scratch.

## Usage

So how's this thing work? The navigation code processes entities with **three key components**:

1. [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) - A component added to any entity you want to, well, navigate.
2. [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) - A component added to [GameObjects](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html) during authoring that also have the [NavMeshSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs) attached. And make sure you bake your surfaces!
3. [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasis.cs) - A glorified parent transform you may attach to any arbitrary [GameObject](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/GameObject.html). If you don't create a basis, then one will be created by default that all of your agents will become children of by way of their presently detected surface (but more on that later).

### The [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs)

FYI, I personally made  *no* effort achieving parity between Unity's concept of the [NavMeshAgent](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/AI.NavMeshAgent.html) and this one. In fact, it's expected that you do *not* use the [NavMeshAgent](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/AI.NavMeshAgent.html) component. Thus, the only authoring script related to the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) is the [NavAgentPrefab](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentPrefab.cs), which is an optional helper that can be used along with [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent).

All that said, if the [NavMeshAgent](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/AI.NavMeshAgent.html) has features you want, but that you don't see in the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs), then please add them to this project and submit a PR.

#### Spawn Variables

The [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) has bunches of member variables. We're mainly concerned with the user-facing variables (meaning those intended for you to use in your project, not to help develop this one). Lets start with the variables we care about while spawning agents:

* `JumpDegrees`: `float` - It's the jump angle in degrees. `45` is a reasonable value to try.
* `JumpGravity`: `float` - It's artifical gravity used specifically for the projectile motion calculations during jumping. `200` is a reasonable value to try.
* `TranslationSpeed`: `float` - This is the translation (movement) speed of the agent. `20` is a reasonable value to try.
* `TypeID`: `int` - This is the type of agent, in terms of the NavMesh system. See examples of use in the demo spawners. There is also a helper method for setting the type from a `string` in the [NavUtil](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/NavUtil.cs) called `GetAgentType`.
* `Offset`: `float3` - This is the offset of the agent from the basis. It's a `float3` and not a mere float representing the y-component from the surface, which you may find odd. But the idea here is to provide flexibility. While you may usually only set the y-component, there could be situations where you want to set x or z.

See the [NavMovingJumpDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavMovingJumpDemoSpawner.cs) and [NavPointAndClickDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavPointAndClickDemoSpawner.cs) in [Assets/Scripts/Demo/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo/Nav) for examples of setting such data. The redundantly named [SpawnDemoSpawner](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/SpawnDemoSpawner.cs) in [Assets/Scripts/Demo](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo) would also be worth looking at, since it's piggybacked for navigation purposes.

#### Status Variables & Components

There are component tags (defined in [NavAgentStatus](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgentStatus.cs)) that the navigation code applies to [NavAgents](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs). Please do *not* write to these, just use them for optimizing your queries. All of this [IComponentData](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Entities.IComponentData.html?q=icomponent) can be applied throughout entire navigation lifecycle for a given [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs):

* `NavFalling` - Exists if the agent is falling.
* `NavJumping` - Exists if the agent is jumping.
* `NavLerping` - Exists if the agent is lerping.
* `NavNeedsSurface` - Exists if the agent needs a surface. This component should be added when spawning an agent. It's also automatically added after the agent jumps. When this component exists, the [NavSurfaceSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurfaceSystem.cs) will try to [raycast](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) for a new surface potentially underneath said agent.
* `NavPlanning` - Exists if the agent is planning paths or jumps.

See the [NavDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavDestinationSystem.cs) and [NavPointAndClickDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavPointAndClickDestinationSystem.cs) in [Assets/Scripts/Demo/Nav](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Assets/Scripts/Demo/Nav) for examples of querying and reading information out of agents to determine when and how to assign destinations.

### The [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs)

The [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs) is much less complicated than the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs). What you'll mainly be concerned with is the associated [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurfaceAuthoring.cs) script. It has some important public variables:

* `HasGameObjectTransform`: `bool` - **Optional, but obviously false if not manually set.** If true the GameObject's transform will be used and applied to possible children via [CopyTransformFromGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.CopyTransformFromGameObject.html?q=copytransform). If false the entity's transform will be used and applied conversely via [CopyTransformToGameObject](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.CopyTransformToGameObject.html).
* `JumpableSurfaces`: `List<NavSurfaceAuthoring>` - **Optional.** This is a list of surfaces that are "jumpable" from *this* one. Immense thought went into this design, and it was determined that automating what's "jumpable" is probably out of scope for this project, but not automating jumping itself. Ultimately it largely depends on the design of one's game. This means it's *entirely on you* to figure out which surfaces are "jumpable" from one another. The agent will *not* automatically know that there is a surface in-between them; however, by using an agent-associated `parent.Value` and checking its [NavJumpableBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavJumpableBufferElement.cs) buffer, you can write code to deliberate on which surface to jump to. As an example, the [NavPointAndClickDestinationSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Assets/Scripts/Demo/Nav/NavPointAndClickDestinationSystem.cs) already does that.
* `Basis`: `NavBasisAuthoring` - **Optional.** This is the basis for a given surface. Agents will automatically become children of the basis of their current surface. This is what permits navigation between surfaces that share a moving basis.

Other than that, you may have some reason to query [NavSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/Surface), but generally speaking you'll work with surfaces via an agent-associated `parent.Value` and the [NavJumpableBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavJumpableBufferElement.cs) (not to be confused with the [NavJumpBufferElement](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavJumpBufferElement.cs), which is what's used internally for the agent to mind "jumpable" points on surfaces, *not* surfaces themselves).

### The [NavBasis](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasis.cs)

This is our last component to cover. Whew. It's simple. Really. Like the [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs), you only need to know about the related [NavBasisAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasisAuthoring.cs) script. It has a couple public variables:

* `HasGameObjectTransform`: `bool` - Same deal as the [NavSurface](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Surface/NavSurface.cs).
* `ParentBasis`: `NavBasisAuthoring` - You can probably guess what this is as well. In essence, a basis can have a basis.

What is the basis, exactly? It's abstract for a reason: it's a glorified parent transform. The logic is as follows: a basis can be the child of a basis; a surface can be a child of a basis; an agent can be a child of a *basis*. **Note** that agents' true parents are *not* surfaces, but rather the basis of their currently detected surface. That is why an agent's [Parent](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.Parent.html?q=parent) component will differ from an agent-associated `parent.Value`. And as previously stated, the basis is optional. You don't have to set it on a [NavSurfaceAuthoring](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Basis/NavBasisAuthoring.cs) script; a default basis will be created for you.

### Constants

There are a bunch of constants in, well, [NavConstants](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/NavConstants.cs). You might be interested in changing the following constants, although be aware that they directly affect the internal workings of the navigation code:

* `JUMP_SECONDS_MAX`: `float` - Default is `5`. Upper limit on the *duration* spent jumping before the agent is actually considered falling. This limit can be reached when the agent tries to jump too close to the edge of a surface and misses.
* `OBSTACLE_RAYCAST_DISTANCE_MAX`: `float` - Default is `1000`. Upper limit on the [raycast](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) distance when searching for an obstacle in front of a given NavAgent.
* `SURFACE_RAYCAST_DISTANCE_MAX`: `float` - Default is `1000`. Upper limit on the [raycast](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) distance when searching for a surface below a given [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs).
* `BATCH_MAX`: `int` - Default is 50. Upper limit when manually batching jobs.
* `ITERATION_MAX`: `int` - Upper limit on the iterations performed in a [NavMeshQuery](https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Experimental.AI.NavMeshQuery.html) to find a path in the [NavPlanSystem](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavPlanSystem.cs).
* `JUMPABLE_SURFACE_MAX`: `int` - Default is `30`. Upper limit on a given jumpable surface buffer. Exceeding this will only result in heap memory blocks being allocated.
* `PATH_NODE_MAX`: `int` - Default is `1000`. Upper limit on a given path buffer. Exceeding this only result in heap memory blocks being allocated.
* `PATH_SEARCH_MAX` - Default is `1000`. Upper limit on the search area size during path planning.
* `SURFACE_RAYCAST_MAX` - Default is `100`. Upper limit on the number of [raycasts](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more [raycasts](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) will be performed.

Some of these constants may be absorbed into the [NavAgent](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs) itself as individual settings later.

## Conclusion

That's it. The navigation code is subject to change at any time, and if it does, it's to help, not confuse you. I'll keep the guide updated to ensure it's current and hopefully understandable. If you have any improvements for it, feel free to update it and submit a PR.

## Tips

* Make sure you bake your [NavMeshSurfaces](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/ThirdParty/NavMeshComponents/Scripts/NavMeshSurface.cs)! The weirdest problems have to do with not having the surfaces baked.
* Anything with an authoring script on it also needs an accompanying [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script as well. Don't forget! The Unity Editor should warn you about that.
* The compatible version of [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) is *already* in [Packages/com.reese.nav/ThirdParty](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/)! Use that and nothing else, and I mean for your entire project. Do not try to mix and match it with other versions.
* Upon spawning [NavAgents](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/Agent/NavAgent.cs), ensure you have their initial [Translation.Value](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Transforms.Translation.html?q=translation) right, along with their `Offset`. Getting these things wrong may result in your agents being unable to [raycast](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) the surface below them, since they may be [raycasting](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts) underneath it!
* Obstacles need the [NavMeshObstacle](https://docs.unity3d.com/2019.3/Documentation/Manual/class-NavMeshObstacle.html), colliders, and the [ConvertToEntity](https://docs.unity3d.com/Packages/com.unity.entities@0.5/api/Unity.Entities.ConvertToEntity.html?q=convert%20to%20ent) script on them. Otherwise obstacles will not be detected by [raycasts](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/collision_queries.html#ray-casts). By the way, `Carve` should be `true`.
