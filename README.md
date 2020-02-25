# ReeseUnityDemos

Unity DOTS packages and samples—featuring ECS, jobs, and the Burst compiler—by me, Reese.

## Packages

This project is a [UPM](https://docs.unity3d.com/Manual/Packages.html) package [monorepo](https://en.wikipedia.org/wiki/Monorepo) that supports my demos, including:

1. [Nav](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav) - DOTS navigation with auto-jumping agents and movable surfaces.
2. [Spawning](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.spawning) - Generic DOTS runtime spawning for any combination of prefab, components, and buffers.
3. [Randomization](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.random) - `Unity.Mathematics.Random` number generators in jobs, including Burst-capable ones. 

Why a monorepo? Because juggling multiple Unity projects with different configurations is annoying. Plus, if I have to update one thing, it forces me to consider updating or removing other things. Centralizing configuration—while distributing stand-alone packages—works best for me personally. The alternative would be a sprawling mishmash of disproportionately maintained projects.

## Demos

Here's how my articles on [reeseschultz.com](https://reeseschultz.com) relate to samples in this project:

### [DOTS Navigation with Auto-Jumping Agents and Movable Surfaces](https://reeseschultz.com/dots-navigation-with-auto-jumping-agents-and-movable-surfaces/)

The DOTS navigation scripts and demos are self-contained so you can use them in *your* project.

![Video of navigation agents jumping across moving surfaces.](/Gifs/nav-moving-jump-demo.gif)

⇒ `Assets/Scenes/Nav/NavMovingJumpDemo.unity`.

![Video of agents spawning and avoiding obstacles.](/Gifs/nav-performance-demo.gif)

⇒ `Assets/Scenes/Nav/NavPerformanceDemo.unity`.

![Video of an agent moving to point-and-clicked destinations.](/Gifs/nav-point-and-click-demo.gif)

⇒ `Assets/Scenes/Nav/NavPointAndClickDemo.unity`.

---

### [Pointing and Clicking with Unity ECS](https://reeseschultz.com/pointing-and-clicking-with-unity-ecs/)


![Video of changing prefab colors with Unity ECS.](/Gifs/point-and-click-demo.gif)

⇒ `Assets/Scenes/PointAndClickDemo.unity`.

---

### [Projectile Motion with Unity DOTS](https://reeseschultz.com/projectile-motion-with-unity-dots/)


![Video of projectile motion demonstration with Unity DOTS.](/Gifs/projectile-demo.gif)

⇒ `Assets/Scenes/ProjectileDemo.unity`

---

### [Random Number Generation with Unity DOTS](https://reeseschultz.com/random-number-generation-with-unity-dots)

⇒ `Assets/Scenes/ProjectileDemo.unity`

⇒ `Assets/Scenes/SpawnDemo.unity`

⇒ `Assets/Scenes/Nav/NavPerformanceDemo.unity`

---

### [Selectively Running Systems in Scenes with Unity ECS](https://reeseschultz.com/selectively-running-systems-in-scenes-with-unity-ecs)

⇒ `Assets/Scenes/ProjectileDemo.unity`

---

### [Spawning Prefabs with Unity ECS](https://reeseschultz.com/spawning-prefabs-with-unity-ecs/)

![Video of spawning prefabs with Unity ECS.](/Gifs/spawn-demo.gif)

⇒ `Assets/Scenes/SpawnDemo.unity`

---

### Contributor Agreement

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
