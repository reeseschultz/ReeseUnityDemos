# ReeseUnityDemos

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)

![Flocking demo.](/preview.gif)

Unity packages and demos—emphasizing ECS, jobs and the Burst compiler—by me, Reese.

## Packages

This project is a [UPM](https://docs.unity3d.com/Manual/Packages.html) package [monorepo](https://en.wikipedia.org/wiki/Monorepo) that supports the included demos, featuring:

1. [Navigation](https://github.com/reeseschultz/ReeseUnityDemos/tree/nav#reeses-dots-navigation) - DOTS navigation with auto-jumping agents and dynamic surfaces; released as a package on the `nav` branch.
2. [Pathing](https://github.com/reeseschultz/ReeseUnityDemos/tree/path#reeses-dots-pathing) - DOTS pathing without any bells and whistles; released as a package on the `path` branch.
3. [Randomization](https://github.com/reeseschultz/ReeseUnityDemos/tree/random#reeses-dots-randomization) - `Unity.Mathematics.Random` number generators in jobs, including Burst-capable ones; released as a package on the `random` branch.
4. [Spatial Events](https://github.com/reeseschultz/ReeseUnityDemos/tree/spatial#reeses-dots-spatial-events) - Reactive entry and exit events in Burst-capable jobs; released as a package on the `spatial` branch.
5. [Utility Code](https://github.com/reeseschultz/ReeseUnityDemos/tree/utility#reeses-utility-code) - General utility code for Unity, mainly DOTS-oriented; released as a package on the `utility` branch.

## Demos

There are various demo scenes included in `Assets/Scenes`. Take a look!

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

Note that you may need to install `clang` on Ubuntu via:

```sh
sudo apt install clang
```

## Credits

* The `Stranded` demo extensively uses [Mini Mike's Metro Minis](https://mikelovesrobots.github.io/mmmm) (licensed with [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/?)) by [Mike Judge](https://github.com/mikelovesrobots). That project is embedded in this one by way of `Assets/MMMM/`. Its directory structure was modified, and new prefabs were generated for it rather than using the included ones.
* The sounds mixed in the `Stranded` demo are from [Freesound](https://freesound.org/); only ones licensed with [CC0](https://creativecommons.org/share-your-work/public-domain/cc0/) are used here.
* The `NavHybridDemo` leverages animations from [Mixamo](https://www.mixamo.com) by [Adobe](https://www.adobe.com/).
* The navigation package uses [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) (licensed with [MIT](https://opensource.org/licenses/MIT)) by [Unity Technologies](https://github.com/Unity-Technologies); this means, for example, that runtime baking is supported, but just from the main thread.
* The navigation package uses [PathUtils](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/PathUtils) (licensed with [zlib](https://opensource.org/licenses/Zlib)) by [Mikko Mononen](https://github.com/memononen), and modified by [Unity Technologies](https://github.com/Unity-Technologies). Did you know that Mikko is credited in [Death Stranding](https://en.wikipedia.org/wiki/Death_Stranding) for [Recast & Detour](https://github.com/recastnavigation/recastnavigation)?

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever. All that said, [GitHub Issues](https://github.com/reeseschultz/ReeseUnityDemos/issues/new/choose) is fine for constructive discussion.

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
