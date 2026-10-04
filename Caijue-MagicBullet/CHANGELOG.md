# Changelog

## 1.2.2 - 2026-08-25

### Changed
- Moved gameplay preview GIFs to external hosting to reduce the Thunderstore package size.
- Improved README preview presentation.

### Notes
- No gameplay behavior changes in this release.

## 1.2.1 - 2026-08-25

- Verified compatibility with the latest *How to Fish* update.
- Added gameplay preview documentation.
- Superseded by v1.2.2 for package-size cleanup.

## 1.2.0 - 2026-08-24

### Added
- Added movement prediction and projectile gravity compensation.
- Added Sticky Target.
- Added Crosshair, Nearest, LowestHealth, and BossPriority target modes.
- Added independent Fish, Bird, and Albatross target filters.
- Added optional Line-of-Sight checks and Wall Penetration.
- Added FOV circle and target tracking line options.
- Added configurable target distance and debug logging.

### Improved
- Improved head/weak-point targeting.
- Improved target caching and performance.
- Improved projectile tracking and cleanup.
- Improved rapid-fire and multi-projectile weapon support.
- Improved Harmony startup resilience.

### Fixed
- Fixed Albatross and other NPC/Boss projectiles being incorrectly redirected by Magic Bullet.
- Fixed local-player projectile ownership detection.
- Fixed target lifecycle issues after creature death or despawn.
- Fixed startup failures caused by invalid lifecycle patches.

## 1.1.0 - 2026-08-24

- Initial public release.
- Added Silent Aim projectile redirection.
- Added forced headshots.
- Added FOV circle and target tracking line.
- Added Bowhead Whale targeting support.
