# v1.4.0

New:

- Balances now show under each player's name above their head. Local setting, `ShowBalancesOnNameplates`.
- `/pay <name or steam id> <amount>` in chat to send money to another player. `!pay` works too. Press Tab to complete a partial name, and again to cycle through matches. You cannot overdraw or send a negative amount, and every payment is announced in chat.
- Tab no longer opens the journal while you are typing in chat.
- `DuplicateBossMoney`, off by default, is an alternative to splitting a boss drop. Instead of dividing the sale between everyone, every player is paid the full amount. A 1000 corpse pays 1000 to each of four players rather than 250 each. This creates money rather than dividing it, so bosses get more rewarding as the group grows instead of less.
- `SplitBoatCosts`, off by default, splits the cost of the boat motor and radar between everyone instead of one player paying it all.
- Both boss money settings and `SplitBoatCosts` are read from the host only, so guests do not need matching configs.

Fixed:

- Wallets did nothing in non Steam sessions. The game gives every player the host's Steam id in LAN and direct games, so all wallets collapsed into one shared pot with no warning.
- Rejoining a session left you at $0 and unable to buy anything until your balance next changed.
- The mod could write a zero balance over the vanilla save's money if the wallet file failed to load, destroying the last copy of the shared pot.
- An error while saving could leave the host holding the total of everyone's wallets.
- A purchase whose buyer could not be identified was handed over free instead of being refused.
- A boss share paid to a player who was disconnecting vanished instead of being paid.
- A player who joined on a reused connection id could be mistaken for a modded client and never warned that they were missing the mod.

# v1.3.0

- Boss and mini boss drops now split their money evenly between everyone in the session when sold, including the meat chunks a boss scatters on death. Controlled by the new `ShareBossMoney` setting, on by default.
- The remainder from an uneven split goes to whoever made the sale, so nothing is lost to rounding.

# Changelog

## 1.2.0

* Fixed sales by anyone other than the host being credited to the host. The seller was identified using the game's `LastHolder`, which is not synchronised, so on the host a fish held by another player often looked like it had never been held by anyone. That fell through to the unclaimed sale rule and paid the host instead. The mod now tracks holders itself on the server, and only falls back to `LastHolder` as a last resort.
* Fixed a player who was slow to be recognised as having the mod never receiving their balance afterwards. The server recorded a wallet value as sent before it had actually sent it, so once that happened it would never resend the same value and the player was stuck on an empty wallet.
* Broadcast handlers are now registered before the client announces itself, closing a window where the first reply from the server could arrive with nothing listening for it.

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
