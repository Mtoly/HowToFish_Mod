# Complete Cheat Menu

A cheat menu for **How to Fish** that covers everything the game can be made to do, behind an
interface that doesn't look like a debug panel. Press **Insert** in game.

Nineteen tabs, everything the game can be made to do.

## Host and solo only

The game's own developer commands are gated behind a host check, and so is this menu. Anything
server-authoritative — money, spawning, god mode — greys out with a reason when you've joined
someone else's lobby. Client-local features stay available everywhere. This isn't a griefing tool.

## Tabs

**Weapons** — infinite ammo, perfect accuracy, zero recoil, rapid fire, extra projectiles, bullet speed, and an aimbot that steers the gun rather than yanking your camera.

**Kill score** — choose which bonuses appear on your kill banner and what they multiply to.

**Player** — god mode, one-shot kills, friendly fire, heal, feed, full vitals reset, and a switch
for the game's built-in developer command suite.

**Movement** — fly mode with separate horizontal and vertical speed sliders (WASD, Space up, Ctrl down; hovers when you release), plus live sliders for walk
speed, sprint speed, crouch multiplier, jump force, extra gravity, acceleration, deceleration and
sink speed. Every slider remembers its original value and has a one-click reset.

**Boat** — unlock boat and radar, motor tier 0–4 in either direction, flight mode, bring the boat
to you, and sliders for max velocity, steering, throttle, anti-rollover and steer smoothing.

**Teleport** — jump to any island, unlock all islands, go to the boat, teleport to any player or
drag them to you, and named waypoints.

**Spawn** — searchable, sortable list of every spawnable prefab, filterable by boss, creature, fish, weapon and more, read straight from the game's own
catalogue so it can't fall out of date. Quantity up to 25, plus dead and drip variants for creatures.

**Items** — edit base worth, weight, cookness, kill-score and betting multipliers, on the item in
your hand or on every item in the world at once.

**Skins** — unlock or lock every weapon skin, wear any skin on the held item, switch boat skins, and
unlock all fourteen NPC characters. Names and rarities come from the game's own skin presets.

**Money** — balance readout, add/remove/set with `50k` / `2.5m` / `1b` shorthand, and quick-add buttons.

**World** — complete or reset the normal and drip journals, boss immortality, heal or kill the boss,
unlock the grill.

**Progression** — unlock everything at once, inventory pockets, finish game, kill scores, Steam
achievements, manual save.

**Entities** — live count of everything loose in the world, despawn by category, by radius, or all at once.

**Gambling** — a win-chance slider or a forced colour for roulette, live readouts, a bet-table
value multiplier, and a skin machine rig. See below for why the ball behaves properly.

**Fishing** — grant any or all baits for free, and equip any of them.

**Visuals** — ESP boxes, names, distances and tracers for creatures, items and players; free camera;
field of view; damage number, blood and decal toggles. All client-side, so this tab works anywhere.

**Keybinds** — a master switch for every cheat hotkey, off by default, plus in-menu rebinding.

**Settings** — save or load named presets of your movement, ESP, camera and item values.

**Diagnostics** — a live self-check of every game member the mod binds to. If a game update breaks
something, this tells you exactly what by name.

## Hotkeys

**Cheat hotkeys are off by default.** Turn them on with the master switch at the top of the
**Keybinds** tab. Until then only the menu key does anything, so a stray F-key can never trigger
a cheat you didn't intend.

| Key | Action | Affected by the master switch |
|---|---|---|
| `Insert` | Open or close the menu | No — always works |
| `F1` | God mode | Yes |
| `F2` | Fly | Yes |
| `F3` | ESP | Yes |
| `F4` | Free camera | Yes |
| `F5` | One-shot kills | Yes |

All five are rebindable in the Keybinds tab. The menu key is config-only, so a mis-set key can
never lock you out of the menu.

## The roulette actually works

Other roulette mods force the payout. The ball then rolls into black while the game hands you a
red win, and the moment is dead.

How to Fish runs the wheel as a genuine physics simulation on the host. `LocalCasino` launches a
rigidbody, waits until it has stayed in one pocket for two seconds, then calls
`GetRouletteColorFromBall()` — which computes the slot as
`((ballAngle − wheelAngle) mod 360) / 9.72973°` and passes that colour to the payout.

Because the slot is a *relative* angle, rotating the wheel changes which pocket the ball sits
over. So instead of touching the result, this mod nudges the wheel a few degrees at a time while
the ball settles, and stops the moment the ball is genuinely over a pocket of the colour you
want. Red and black alternate, so that's usually less than one pocket of movement. The game's own
check then returns your colour because it really is your colour — and since the host streams
wheel rotation to clients every tick, everyone watching sees the same thing.

The skin machine works on the same principle: `SlotMachineManager.SetCheatSkin` is a developer
hook that plants a skin into the reel and points the landing index at it, so the machine stops on
the skin it awards.

## How it works

The game ships a full developer cheat suite (`DazedCommands`) gated behind
`ClientSettings.CheatsEnabled` — a plain static bool with a public setter. This mod flips that
switch and drives the developers' own code paths wherever they exist, rather than re-implementing
them. Networked actions go through `Server.Instance`, which is the same route the game uses, so
everything replicates correctly instead of desyncing.

All game access is funnelled through one binding layer (`src/Game/GameBinder.cs`). A rename in a
future game update means editing one file, and until then the Diagnostics tab reports it as a
named failure instead of a dead button.

Nothing in the menu can throw into the game's own callbacks — every tab renders inside a
try/catch, and every `Update`, `FixedUpdate` and `OnGUI` entry point is wrapped.

## Save safety

The first time a destructive action runs in a session, the save folder is copied to a timestamped
backup next to it. It's silent and never prompts. Turn it off with `BackupSaves` in the config.

Steam achievements are not covered — those are written server-side and can't be undone from here.

## Config

`BepInEx/config/envincible.howtofish.completecheatmenu.cfg`

| Key | Default | Purpose |
|---|---|---|
| `ToggleKey` | `Insert` | Opens and closes the menu |
| `EnableAllKeybinds` | `false` | Master switch for every cheat hotkey |
| `AutoEnableCheats` | `true` | Unlock the dev command suite on load |
| `BackupSaves` | `true` | One silent save copy per session |

## Building

```
dotnet build -c Release -p:Deploy=true
```

Needs the .NET SDK at `C:\Users\schol\.dotnet` — set `DOTNET_ROOT` and prepend it to `PATH` first,
it isn't registered globally.
