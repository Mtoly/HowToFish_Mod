# Mod Menu

Adds a native-style **Mod Settings** entry to both the main menu and the in-game pause screen, providing one place
to configure installed BepInEx mods.

## Features

- Uses the game's own Options button as the visual and interaction template.
- Opens directly from the normal in-game pause screen without leaving the current world.
- Shows installed mods in a scrollable list on the left and their settings on the right.
- Remembers the last selected mod and restores that tab the next time the menu is opened.
- Automatically discovers standard BepInEx `ConfigEntry` settings.
- Supports booleans, numbers, strings, enums, keyboard shortcuts, acceptable ranges, and acceptable-value lists.
- Includes `HowToFish.ModAPI.dll` for mods that want curated settings integration.

## Usage

1. Install with Thunderstore Mod Manager or r2modman.
2. Launch **How to Fish**.
3. Select **Mod Settings** from the main menu or pause screen.
4. Choose a mod on the left and change its settings on the right.

Settings backed by BepInEx config files are saved immediately.

Mod Menu does not enable, disable, move, rename, or delete mod files. Install, update, and remove mods only through your mod manager.

## Manual installation

Install BepInEx 5, then copy the package contents into the game directory so the included `BepInEx` folders merge with the existing ones.

## Compatibility

- How to Fish 1.0.5
- BepInEx 5.4.23.5

## For mod developers

Normal `Config.Bind(...)` entries appear automatically. Advanced integrations can reference `HowToFish.ModAPI.dll` and implement `IModSettingsProvider`. Live toggling should only be advertised when a plugin can completely undo and rebuild its runtime state.

## Development disclosure

This project was developed with substantial assistance from OpenAI Codex. The packaged code was reviewed, compiled, and tested against the game before release.
