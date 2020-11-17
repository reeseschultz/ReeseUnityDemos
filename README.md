# ReeseUnityDemos

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)

Unity packages and demos—emphasizing ECS, jobs and the Burst compiler—by me, Reese.

## Packages

This project is a [UPM](https://docs.unity3d.com/Manual/Packages.html) package [monorepo](https://en.wikipedia.org/wiki/Monorepo) that supports the included demos, featuring:

1. [Nav](https://openupm.com/packages/com.reese.nav/) - DOTS navigation with auto-jumping agents and movable surfaces; released as a package on the `nav` branch.
2. [Randomization](https://openupm.com/packages/com.reese.random/) - `Unity.Mathematics.Random` number generators in jobs, including Burst-capable ones; released as a package on the `random` branch.

## Demos

![Gif of agents navigating complex terrain.](/Gifs/nav-terrain-demo.gif)

⇒ `Assets/Scenes/Nav/NavTerrainDemo.unity`.

---

![Gif of agents jumping across moving surfaces.](/Gifs/nav-moving-jump-demo.gif)

⇒ `Assets/Scenes/Nav/NavMovingJumpDemo.unity`.

---

![Gif of agents spawning and avoiding obstacles.](/Gifs/nav-performance-demo.gif)

⇒ `Assets/Scenes/Nav/NavPerformanceDemo.unity`.

---

![Gif of an agent moving to point-and-clicked destinations.](/Gifs/nav-point-and-click-demo.gif)

⇒ `Assets/Scenes/Nav/NavPointAndClickDemo.unity`.

---

![Gif of changing prefab colors with Unity ECS.](/Gifs/point-and-click-demo.gif)

⇒ `Assets/Scenes/PointAndClickDemo.unity`.

---

![Gif of projectile motion demonstration with Unity DOTS.](/Gifs/projectile-demo.gif)

⇒ `Assets/Scenes/ProjectileDemo.unity`

---

![Gif of spawning prefabs with Unity ECS.](/Gifs/spawn-demo.gif)

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

### IL2CPP Setup

If, despite prior warning, you still want to use IL2CPP, note that you need to install `clang` on Ubuntu via:

```sh
sudo apt install clang
```

### Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever. All that said, [GitHub Issues](https://github.com/reeseschultz/ReeseUnityDemos/issues/new/choose) is fine for constructive discussion.

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
