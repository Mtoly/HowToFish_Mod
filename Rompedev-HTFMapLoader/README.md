# HTF Map Loader

A generic custom-island loader for **How to Fish**.

## Requirements

- How to Fish 1.0.5
- BepInEx 5 / Mono x64
- Windows x64

For map creators:

- Unity **6000.4.4f1**
- Project template: **Universal 3D (URP)**

## What it does

HTF Map Loader allows custom maps to be distributed as independent Thunderstore packages instead of requiring every map to ship its own BepInEx plugin.

It supports:

- Unity AssetBundle scenes
- automatic or manual island placement
- conflict avoidance with existing island navigation positions
- native-style island navigation triggers
- radar markers and per-map radar colors
- `IslandHolder`
- `PlayerSpawnPoint`
- `BoatSpawnPoint`
- runtime repair of common island objects
- optional built-in Seagull and Clam spawners
- independent map packages installed anywhere below `BepInEx/plugins`

## Installing

Use **Install with Mod Manager**.

BepInEx is declared as a dependency and will be installed automatically by a compatible Thunderstore mod manager.

Manual installation is also possible by placing `HTFMapLoader.dll` inside `BepInEx/plugins`.

## Where maps are discovered

Version **0.2.8** scans installed plugin packages recursively for:

```text
BepInEx/plugins/**/Maps/**/map.json
```

It also remains compatible with the old layout where a `Maps` directory sits beside `HTFMapLoader.dll`.

That means a third-party map can have its own Thunderstore package:

```text
BepInEx/
└── plugins/
    └── SomeMapPackage/
        └── Maps/
            └── MyMap/
                ├── map.json
                └── my_assetbundle
```

## Creating your first map

1. Install Unity **6000.4.4f1**.
2. Create a **Universal 3D (URP)** project.
3. Create your island scene.
4. Add an `IslandHolder` root.
5. Add colliders to walkable geometry.
6. Add `PlayerSpawnPoint` and `BoatSpawnPoint`.
7. Build the scene as a Windows x64 AssetBundle using the included builder.
8. Put the bundle beside `map.json`.
9. Test navigation, radar, collisions and respawning in-game.

Full documentation is included in `docs/`.

## Recommended Unity hierarchy

```text
IslandHolder
├── Environment
│   ├── Terrain
│   ├── Rocks
│   ├── Buildings
│   └── Colliders
├── PlayerSpawnPoint
└── BoatSpawnPoint
```

Keep `IslandHolder` at:

```text
Position: 0, 0, 0
Rotation: 0, 0, 0
Scale:    1, 1, 1
```

For most maps use:

```json
"positionMode": "auto",
"moveScene": true
```

The loader will choose a free world/navigation position.

## Included creator files

```text
docs/
├── MAP_CREATION_GUIDE.md
├── MAP_JSON_REFERENCE.md
└── THUNDERSTORE_MAP_PACKAGE.md

examples/
├── beginner_map.json
└── advanced_map.json

Unity/
└── Assets/
    └── Editor/
        └── HTFMapBundleBuilder.cs
```

## Test map

**Rival Isles** is the reference/test package for the loader and can be installed separately.
