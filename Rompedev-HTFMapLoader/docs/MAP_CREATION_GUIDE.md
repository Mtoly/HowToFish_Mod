# HTF Map Creation Guide

## Required editor

Use **Unity 6000.4.4f1** and create the project from **Universal 3D (URP)**.

Using the same Unity version and render pipeline as the target content avoids avoidable AssetBundle/material incompatibilities.

## 1. Create the scene

Create a scene, for example:

```text
Assets/Scenes/MyIsland.unity
```

The name in `map.json -> scene` must match the scene that is actually stored in the AssetBundle.

## 2. Create IslandHolder

Create an empty root GameObject named exactly:

```text
IslandHolder
```

Recommended transform:

```text
Position  0, 0, 0
Rotation  0, 0, 0
Scale     1, 1, 1
```

Build the island around the origin. This is the simplest workflow when using `"moveScene": true`.

## 3. Add your environment

Meshes alone do not necessarily provide gameplay collision.

Add appropriate:

- MeshCollider
- BoxCollider
- CapsuleCollider

to surfaces where the player or boat must collide.

The loader adapts normal static colliders to the game's Level collision setup at runtime.

## 4. Player spawn

Under `IslandHolder`, create an empty GameObject:

```text
PlayerSpawnPoint
```

Place it on safe ground, slightly above the floor if necessary.

Its position and rotation should represent where the player is expected to reappear.

## 5. Boat spawn

Create another empty GameObject:

```text
BoatSpawnPoint
```

Place it in open water with enough clearance for the boat.

Use the rotation to choose the desired boat orientation.

## 6. Build an AssetBundle

Copy:

```text
Unity/Assets/Editor/HTFMapBundleBuilder.cs
```

into your Unity project at:

```text
Assets/Editor/HTFMapBundleBuilder.cs
```

Select your `.unity` scene in the Project window, then use:

```text
Tools
└── HTF Map Loader
    └── Build Selected Scene AssetBundle
```

Give the bundle a simple name, for example:

```text
my_island
```

The builder outputs files to:

```text
HTFMapBuild/
```

You distribute the AssetBundle itself. You do not need its generated `.manifest` file.

## 7. Create map.json

The bundle filename and scene name must match the real files/content.

A simple map normally uses automatic placement:

```json
{
  "id": "my_island",
  "name": "My Island",
  "author": "YourName",
  "bundle": "my_island",
  "scene": "MyIsland",
  "positionMode": "auto",
  "position": { "x": 0, "y": 0, "z": 0 },
  "moveScene": true,
  "radarColor": "#FF4FA3",
  "islandSize": 55,
  "runtime": {
    "playerSpawn": "PlayerSpawnPoint",
    "boatSpawn": "BoatSpawnPoint",
    "defaultPlayerSpawn": { "x": 0, "y": 6, "z": 0 },
    "defaultBoatSpawn": { "x": 0, "y": 0.5, "z": -25 }
  },
  "wildlife": {
    "seagulls": false,
    "clams": false,
    "seagullMax": 3,
    "seagullDelay": 60,
    "clamMax": 3,
    "clamDelay": 15
  }
}
```

## 8. Test before publishing

At minimum test:

- map is discovered in the BepInEx log
- navigation trigger appears at the expected location
- entering the trigger loads the map
- radar marker appears
- radar color is correct
- island geometry is in the expected place
- player can walk on the island
- boat collides correctly
- `PlayerSpawnPoint` behaves correctly
- `BoatSpawnPoint` behaves correctly
- leaving for a native island works
- returning to the custom map works
- no repeated scene-load flicker occurs

Enable optional wildlife only after the base island works.

## 9. Publish as a separate Thunderstore package

A map package should depend on `Rompedev-HTFMapLoader-0.2.8`.

Its game files should ultimately be under a `Maps` directory in the package's BepInEx plugin installation.

See `THUNDERSTORE_MAP_PACKAGE.md`.
