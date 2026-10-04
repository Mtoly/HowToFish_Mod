# Publishing a Map on Thunderstore

A custom map is a normal Thunderstore package that depends on HTF Map Loader.

## manifest.json

Example:

```json
{
  "name": "MyIsland",
  "version_number": "1.0.0",
  "website_url": "",
  "description": "A custom island for How to Fish.",
  "dependencies": [
    "Rompedev-HTFMapLoader-0.2.8",
    "BepInEx-BepInExPack-5.4.2305"
  ]
}
```

## ZIP structure

Required Thunderstore files are at ZIP root.

The map payload goes under the BepInEx plugin route:

```text
manifest.json
README.md
CHANGELOG.md
icon.png

BepInEx/
└── plugins/
    └── Maps/
        └── MyIsland/
            ├── map.json
            └── my_island
```

HTF Map Loader 0.2.8 recursively discovers `Maps` directories throughout installed BepInEx plugin packages.

## Dependencies

The map should depend on HTF Map Loader.

The loader itself depends on BepInEx. Keeping BepInEx as an explicit direct map dependency is also acceptable and makes the requirement visible on the package page.

## Before upload

Make sure:

- icon.png is exactly 256x256.
- `manifest.json`, `README.md`, and `icon.png` are at ZIP root.
- dependency strings match already-published package names/versions.
- the AssetBundle filename exactly matches `map.json -> bundle`.
- the AssetBundle's scene exactly matches `map.json -> scene`.
