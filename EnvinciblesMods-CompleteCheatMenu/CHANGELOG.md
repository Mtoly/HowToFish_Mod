# Changelog

## 0.6.1 — 2026-08-23

Six fixes from the second play session. Every one had a real cause in the game's code.

**Boat handling did nothing, and flight wouldn't move forward.** Two separate causes.
`_maxVelocity` is copied into `HiddenPhysicsRig.maxLinearVelocity` once at startup and never read
again, so changing the field afterwards was ignored — the cap is now written directly. Actual
speed comes from `BoatMotor.Force`, which is what the thrust calculation uses, so that is now the
**Engine power** slider. Steering and throttle angle have been removed from the tab: they only
rotate the wheel and lever props and never affected handling, so presenting them as tuning was
wrong. Drag values that do matter were added in their place. Flight now holds `_propellerInWater`
true, because thrust is gated on it and an airborne propeller reads as out of water — that is why
the boat hovered but wouldn't drive.

**Aimbot did nothing.** It aimed the fire point during `Update`, but `PlayerToolMovement` re-poses
the whole weapon rig in `LateUpdate`, so the rotation was stale before any shot read it. Aiming is
now a prefix on `Weapon.Shoot`, which removes the ordering problem entirely.

**Zero recoil failed under rapid fire.** `Attachments` exposes recoil as read-only properties that
forward to the barrel attachment — there was no field to zero, so those writes did nothing and
only the self-knockback was ever suppressed. The three recoil calls `Shoot` makes are now
intercepted directly, which holds at any fire rate.

**Infinite ammo cut the gun's audio.** Clearing `_isReloading` and `_queueReload` every frame was
chopping the reload animation and its sound off mid-play. Ammo is now topped up only once it has
actually been spent and the reload flags are left alone, so the magazine never empties and the
audio runs normally.

**Show my character did nothing.** Enabling the renderer wasn't enough — those meshes sit on
GameObjects the local rig leaves deactivated. The objects are switched on too, shadow-only casting
is cleared, and an empty renderer field is now reported instead of silently skipped.

**Weapon reset restores real values.** It previously wrote invented defaults. Original values are
now captured per weapon before anything is changed.

## 0.6.0 — 2026-08-23

Two new tabs, a reworked skin machine, and the mod's first Harmony patches.

**Weapons tab.** Infinite ammo (also cancels queued reloads), perfect accuracy, zero recoil
(screen kick, weapon kick and the self-knockback), rapid fire, extra projectiles per shot, and
bullet speed. Plus **fire where you're looking**, which aligns the gun's fire point with the
camera so hipfire stops drifting off the crosshair.

**Aimbot.** `Weapon.Shoot` fires along `_attachments.FirePoint.forward`, so rather than teleporting
bullets or faking hits, this points that transform at the best target — your shots follow it while
your camera stays under your control. Configurable cone, range, aim height, creature/player
filters and an optional line-of-sight check that rejects targets behind terrain. Live readout of
what it's locked onto.

**Kill score tab.** Pick exactly which bonuses land on your kill banner. These are the game's own
`Bonus` entries pulled from `KillScoreCalculator.GetAllBonuses`, so the popup after a kill shows
them by name and the payout uses the same product — the multiplier you choose is the one you see
and the one you get. A flat multiplier can be added on top as its own labelled row rather than
appearing out of nowhere.

**Keep inventory.** Skips the death drop on both `PlayerInventory.ServerDropAll` and
`Server.DropAllItems`, so the client and host agree about where your loot is.

**Stacking.** The game has no stack count — each slot holds one real Item with its own worth and
skin — so this duplicates the held item into that many genuine copies rather than faking a number.

**Skin machine rebuilt.** It's now a three-step flow: choose what you're winning a skin for (boat,
held item, or any weapon by name), pick a rarity or take any, then arm a random skin of that
rarity or choose the exact one. Shows what's currently armed, and each skin can be unlocked
outright from the same row.

Harmony is used for exactly two things — the kill score bonus list and the death drop — because
both need to intercept a call the game makes to itself. Both carry `Prepare()` guards.

## 0.5.0 — 2026-08-23

Bug fixes from the first play session, plus the requested additions.

**Fixed — fly only moved you vertically.** `PlayerMovement.FixedUpdate` rebuilds horizontal
velocity from input every physics step and only reads `y` back off the rigidbody, so the
horizontal part of the old `SetVel` call was overwritten instantly. Fly now raises the game's own
`_curMoveSpeed` cap for horizontal motion and owns the vertical axis directly. **Separate
horizontal and vertical speed sliders**, and releasing both height keys hovers instead of falling.

**Fixed — killing a boss healed it.** `Creature.ServerChangeHp(int damage)` does
`_hp.Value -= damage`, so passing a negative added HP. It now passes positive damage, and clears
boss immortality first because that method early-returns while immortal.

**Fixed — motor tier could not be lowered.** `Boat.SetMotor` only assigns when the new tier is
higher — it is an upgrade path by design. Downgrades now write the SyncVar directly, which still
replicates and fires the change handler.

**Fixed — Teleport "Bring" did nothing.** Two classes in the assembly are named `Player`: the real
one and Unity's bundled InputSystem sample. Type lookup by name could resolve the wrong one and
silently return null for every `Player` member. The binder now disambiguates by looking for a
marker member. Bring also falls back to moving the target's `PlayerMovement` directly and logs
which path worked.

**Fixed — you kept walking in free camera.** The body is now pinned while the camera is detached.

**New — Entities tab.** A live count of everything loose in the world grouped by category, with
despawn by category, despawn within a radius of you, clear all creatures, or clear everything.
Uses `Item.DestroyItem` so objects despawn across the network properly. Held and stored items are
excluded and never touched.

**New — Spawn sorting and categories.** Sort A–Z, by category, or by item ID, and filter to Boss,
Creature, Fish, Bird, Weapon, Melee, Explosive, Tool or Other. Categories are derived from the
components each prefab carries, not from its name.

**New — boat flight and higher top speed.** Space and Ctrl control the boat's height with gravity
off; max velocity slider now reaches 600.

**New — show your own character.** The game only builds hand meshes for your own player, which is
why free camera showed floating hands. This runs the full-body setup it uses for everyone else.

## 0.4.0 — 2026-08-23

New **Gambling** tab, replacing Casino.

**The ball actually lands on the colour you win with.** The roulette is a real physics
simulation — `LocalCasino` spins a rigidbody and `EndRoulette` reads the ball's true angle
against the wheel to decide the result. Mods that force the payout leave the ball resting on a
pocket that didn't pay, which ruins the moment. This steers the *wheel* by a fraction of a pocket
while the ball settles, so the ball genuinely comes to rest on the target colour and the game's
own geometry check returns it. No payout is ever forged, and because the host streams wheel
rotation every tick, other players see the same result.

- **Win chance** slider, 0–100%, rolled once per spin. A win steers to the colour you bet; a loss
  steers to the other of red or black, never green, so a loss can't accidentally pay 35x.
- **Force colour** mode — every spin lands on the colour you pick.
- Live panel: bet colour, which pocket the ball is currently over, what it's steering toward,
  pot worth, last result, and a running win/loss tally.
- **Settle now** ends a spin on the pocket the ball already occupies.
- **Bet table** — multiply the worth of everything you've staked before the wheel resolves, and
  start a spin manually on any colour.
- **Skin machine** — arm any skin from your held item and the reel visibly stops on it. This uses
  `SlotMachineManager.SetCheatSkin`, the developers' own rig, which plants the skin in the reel
  and points the landing index at it. Searchable by name or rarity.

## 0.3.1 — 2026-08-23

- New **Keybinds** tab, moved out of Settings so it is actually findable.
- Master switch at the top of it turns every cheat hotkey on or off at once.
  **Off by default** — F1 to F5 do nothing until you enable them, so a stray keypress can
  never fire a cheat you did not intend.
- The menu key is deliberately outside the switch and always works. It stays config-only, so a
  mis-set key cannot lock you out of the menu.
- Binds can still be rebound and cleared while the switch is off.

## 0.3.0 — 2026-08-23

Phase 3, feature complete. Fifteen tabs.

- **Visuals** — ESP boxes with names, distances and tracers for creatures, items and players;
  free camera with mouse look and shift boost; field of view; damage number, blood and decal
  toggles. Entirely client-side, so this tab works in anyone's lobby.
- **Settings** — rebind every hotkey in-menu, and save or load named presets covering movement,
  ESP, camera and item values. Presets are plain text files you can edit by hand.
- Default hotkeys: F1 god mode, F2 fly, F3 ESP, F4 free camera, F5 one-shot.
- ESP renders whether or not the menu is open.
- Free camera restores the player camera on unload, so quitting with it active is safe.

## 0.2.0 — 2026-08-23

Phase 2. Eight new tabs; thirteen in total.

- **Boat** — unlock boat and radar, motor tier 0–4, bring the boat to you, and live sliders for
  max velocity, steering, throttle, anti-rollover and steer smoothing.
- **Teleport** — jump to any island, unlock all islands, go to the boat, teleport to any player or
  drag them to you, plus named waypoints that last the session.
- **Items** — edit base worth, weight, cookness, kill-score and betting multipliers on the item in
  your hand or on every item in the world at once.
- **Skins** — unlock or lock every weapon skin, wear any skin on the held item, switch boat skins,
  and unlock all fourteen NPC characters. Skin names and rarities are read from the game's own presets.
- **World** — complete or reset the normal and drip journals, boss immortality, heal and kill boss,
  unlock the grill.
- **Progression** — unlock everything in one press, inventory pockets, finish game, show kill
  scores, Steam achievements, manual save.
- **Casino** — force the roulette result to any colour, force which skin the slot machine awards.
- **Fishing** — grant any or all baits without paying, and equip any bait.
- Diagnostics now self-checks all thirty phase-2 bindings as well.

## 0.1.0 — 2026-08-23

First phase. Menu shell and the first four tabs.

- Custom-skinned window: sidebar tabs, search, drag, resize, toast notifications.
- **Player** — god mode, one-shot, friendly fire, heal, feed, vitals reset, dev-cheat switch.
- **Movement** — fly mode plus eight live movement sliders with per-field reset.
- **Money** — balance readout, add/remove/set with `k`/`m`/`b` shorthand, quick-add buttons.
- **Spawn** — searchable browser over the game's own spawnable catalogue, dead and drip variants.
- **Diagnostics** — per-binding self-check so game updates surface as named failures.
- Host and solo gating on every server-authoritative tab.
- Silent one-per-session save backup before the first destructive action.
