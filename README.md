# ReeseUnityDemos

Unity packages and demos—emphasizing ECS, jobs, and the Burst compiler—by me, Reese.

## Packages

This project is a [UPM](https://docs.unity3d.com/Manual/Packages.html) package [monorepo](https://en.wikipedia.org/wiki/Monorepo) that supports the included demos, featuring:

1. [Nav](https://openupm.com/packages/com.reese.nav/) - DOTS navigation with auto-jumping agents and movable surfaces.
2. [Spawning](https://openupm.com/packages/com.reese.spawning/) - Generic DOTS runtime spawning for any combination of prefab, components, and buffers.
3. [Randomization](https://openupm.com/packages/com.reese.random/) - `Unity.Mathematics.Random` number generators in jobs, including Burst-capable ones.

All of my packages are available on [OpenUPM](https://openupm.com/). Please [support](https://www.patreon.com/openupm) it and its maintainer, Favo. We depend on dedicated people like him.

### `ubump`

My packages benefit from [ubump](https://github.com/reeseschultz/ubump), automating their SemVer-bumping needs, including committing, pushing, tagging, changelog generation and subtree splitting so each package can be released stand-alone and imported with OpenUPM or Git.

![Video of using ubump's interactive CLI mode.](Gifs/ubump.gif)

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

## Linux & You

Linux users may need to do some extra work to get the project and/or packages up and running.

### Mono Setup

Install Mono by following [these directions](https://www.mono-project.com/download/stable/).

### Burst Prerequisite Setup

Avoid sandboxing Unity Hub and Unity with Flatpak or Snap, otherwise `libdl.so` may be inaccessible to the editor.

Also, on Ubuntu, you may need to manually install `gcc-multilib` and `libncurses5` with:

```sh
sudo apt install gcc-multilib libncurses5
```

### Contributor Agreement

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
