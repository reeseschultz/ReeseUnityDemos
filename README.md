# ReeseUnityDemos

![Flocking demo.](/preview.gif)

Unity packages and demos—emphasizing ECS, jobs and the Burst compiler—by [Reese](https://github.com/reeseschultz) and others.

(This project is not associated with Unity Technologies.)

## Packages

This project is a [UPM](https://docs.unity3d.com/Manual/Packages.html) package [monorepo](https://en.wikipedia.org/wiki/Monorepo) that supports the included demos, including:

* [Navigation](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/README.md#reeses-dots-navigation) - DOTS navigation with flocking, auto-jumping agents and dynamic surfaces; released as a package on the `nav` branch.
* [Pathing](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.path/README.md#reeses-dots-pathing) - DOTS pathing without any bells and whistles; released as a package on the `path` branch.
* [Entity Prefab Groups](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.epg#reeses-entity-prefab-groups) - Create and reference groups of entity prefabs with ease; released as a package on the `epg` branch.
* [Math Extensions](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.math/README.md#reeses-dots-math-extensions) - Includes math functions missing from DOTS; released as a package on the `math` branch.
* [Randomization](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.random/README.md#reeses-dots-randomization) - `Unity.Mathematics.Random` number generators in jobs, including Burst-capable ones; released as a package on the `random` branch.
* [Spatial Events](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.spatial/README.md#reeses-dots-spatial-events) - Reactive entry and exit events in Burst-capable jobs; released as a package on the `spatial` branch.
* [Utility Code](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.utility/README.md#reeses-utility-code) - General utility code for Unity, mainly DOTS-oriented; released as a package on the `utility` branch.

## Demos

There are various demo scenes included in `Assets/Scenes`. Take a look!

## Acknowledgments

* The `Stranded` demo extensively uses [Mini Mike's Metro Minis](https://mikelovesrobots.github.io/mmmm) (licensed with [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/?)) by [Mike Judge](https://github.com/mikelovesrobots). That project is embedded in this one by way of `Assets/MMMM/`. Its directory structure was modified, and new prefabs were generated for it rather than using the included ones.
* The sounds mixed in the `Stranded` demo are from [Freesound](https://freesound.org/); only ones licensed with [CC0](https://creativecommons.org/share-your-work/public-domain/cc0/) are used here.
* The `NavHybridDemo` leverages animations from [Mixamo](https://www.mixamo.com) by [Adobe](https://www.adobe.com/).
* The navigation package uses [NavMeshComponents](https://github.com/Unity-Technologies/NavMeshComponents) (licensed with [MIT](https://opensource.org/licenses/MIT)) by [Unity Technologies](https://github.com/Unity-Technologies); this means, for example, that runtime baking is supported, but just from the main thread.
* The navigation package uses [PathUtils](https://github.com/reeseschultz/ReeseUnityDemos/tree/master/Packages/com.reese.nav/ThirdParty/PathUtils) (licensed with [zlib](https://opensource.org/licenses/Zlib)) by [Mikko Mononen](https://github.com/memononen), and modified by [Unity Technologies](https://github.com/Unity-Technologies). Did you know that Mikko is credited in [Death Stranding](https://en.wikipedia.org/wiki/Death_Stranding) for [Recast & Detour](https://github.com/recastnavigation/recastnavigation)?

## Contributing

All contributions to this repository are licensed under [MIT](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
