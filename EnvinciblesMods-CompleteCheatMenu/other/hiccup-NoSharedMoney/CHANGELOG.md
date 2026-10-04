# Changelog

## 1.1.0

* Wallets are now included in SaveBackups snapshots when that mod is installed, so restoring a save also restores everyone's money. Without it the save and the wallets could be rolled back independently, leaving the recorded total disagreeing with what people actually had.
* A wallet file that exists but cannot be read no longer causes the shared balance to be split evenly again. The mod now refuses to touch the file so it can still be recovered, and says so in the log.
* The wallet file is written through a temporary file and swapped into place, so a crash while saving can no longer leave it half written.
* An unclaimed balance, from migrating a save that had money but no player records, is now stored in the wallet file instead of only living in memory. It could previously be lost if the game saved before anyone joined.
* Money held by a player with no Steam ID is no longer counted into the total written to the save file, since it was never stored in the wallet file and vanished on the next load.
* Updated the BepInEx dependency to 5.4.2305.

## 1.0.0

- Initial release.
- Per-player wallets replace the shared balance, keyed by Steam ID and persisted per save.
- All eight purchase paths charge the buyer: items, bait, attachments, bullet upgrades, sharpness upgrades, boat motor, boat radar, and inventory pockets.
- Selling credits whoever sold the item.
- Existing shared balances are split evenly across the players recorded in the save on first load.
- The vanilla save's money field is kept at the crew total so uninstalling is non-destructive.
- Host is warned in console and chat when a connected player is missing the mod.
