# map.json Reference

## Core identity

`id` — Stable unique technical identifier for the map.

`name` — Human-readable display name.

`author` — Map creator.

`bundle` — Exact AssetBundle filename beside `map.json`.

`scene` — Exact Unity scene name stored in the AssetBundle.

## Placement

`positionMode`

- `auto`: loader chooses a navigation position.
- `manual`: use the requested `position`, unless it conflicts with an already registered island, in which case the loader may move it to a safe free position.

`position`

```json
{ "x": 0, "y": 0, "z": 0 }
```

`moveScene`

- `true`: move `IslandHolder` to the resolved navigation position.
- `false`: preserve authored world coordinates.

For normal creator maps authored around `(0,0,0)`, use `true`.

## Radar

`radarColor`

HTML hex:

```text
#RRGGBB
#RRGGBBAA
```

Example:

```text
#FF4FA3
```

If empty, the loader selects a palette color.

## Runtime

`islandSize` — Value applied to the game's `Island._islandSize`.

`runtime.playerSpawn` — GameObject name to find recursively. Recommended: `PlayerSpawnPoint`.

`runtime.boatSpawn` — Recommended: `BoatSpawnPoint`.

`runtime.defaultPlayerSpawn` / `runtime.defaultBoatSpawn` — fallback local positions if marker objects do not exist.

Published maps should normally include the actual marker GameObjects rather than relying on fallback positions.

## Wildlife

`wildlife.seagulls` and `wildlife.clams` opt into built-in spawners.

Related values:

- `seagullMax`
- `seagullDelay`
- `clamMax`
- `clamDelay`

Wildlife is disabled by default.
