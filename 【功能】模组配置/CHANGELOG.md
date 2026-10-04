# Changelog

## 0.3.6

- Mod Settings now remembers the last selected mod and restores that tab the next time the menu is opened.
- The remembered tab works across both main-menu and pause-screen entry points and across game restarts.

## 0.3.5

- Removed the one-second UI discovery interval so `Mod Settings` appears with the native menu buttons immediately.

## 0.3.4

- Added the native-style `Mod Settings` entry to the in-game pause screen.
- Closing the mod page returns to the pause screen while the world remains paused.

## 0.3.3

- Replaced the package icon with a real native-style `Mod Settings` menu preview and a readable `MOD MENU` title.
- No menu or configuration behavior changed in this release.

## 0.3.2

- Shortened the public plugin and package name to `Mod Menu`.
- Removed the plugin on/off control and all plugin file-management behavior.
- Removed the Bootstrap preloader and private enabled-state registry.
- Removed the lifecycle toggle API; Mod Menu is now settings-only.
- Added packaging guards against preloader and file-management components.

## 0.3.1

- Added a native-style Mod Settings entry based on the game's Options button behavior.
- Added a two-column installed-mod list and settings layout.
- Added automatic BepInEx configuration discovery and validated controls.
- Added the optional Mod API assembly for advanced integrations.
