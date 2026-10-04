# NoSharedMoney

How to Fish gives the whole crew one shared bank balance. This mod gives every player their own.

Sell a fish, the money goes to **you**. Buy a rod, it comes out of **your** wallet. Your balance follows your Steam account and survives rejoins and restarts.

## Everyone needs this mod

**All players must have NoSharedMoney installed, including the host.** Wallet balances are pushed to each client individually, and a client without the mod has no way to receive them.

If someone joins without it, the host gets a warning in the console and in chat. That player is not able to break anything, since the server still refuses any purchase they cannot afford, but their money display will be stuck on a stale number and they will not be able to buy anything until they install the mod.

## How it works

- Each player has their own balance, keyed by Steam ID.
- **Selling** credits whoever fed it to a quest NPC. If you catch a fish and hand it to a friend, they get paid for it, so loot can be passed around deliberately.
- **Buying** charges the player who interacted with the shop. That includes shared boat upgrades like the motor and the radar: the buyer pays the full price, and the whole crew gets the benefit.
- Every purchase is still checked on the server against the buyer's real balance, so nobody can spend money they do not have.

## Existing saves

The first time you load a save that still has a shared balance, that money is **split evenly** between the players recorded in the save, with any remainder going to the first of them. If the save has no player records yet, the whole amount goes to the first player who joins.

Balances live in `%USERPROFILE%\AppData\LocalLow\Dazed Games\How to Fish\NoSharedMoney\<save name>.json`, alongside the game's own saves. It is plain JSON keyed by Steam ID, so you can read or hand-edit it. The vanilla save file is left intact, and its single money field is kept up to date with the crew's combined total, so removing the mod leaves you with a sensible shared balance rather than a broken save.

## Backups

Your balances live in a file of their own, separate from the game's save. That means the two can drift apart: roll a save back and the wallets stay where they are, so the money recorded in the save no longer matches what people actually hold.

Install [SaveBackups](https://thunderstore.io/c/how-to-fish/p/hiccup/SaveBackups/) and this stops being possible. This mod registers with it automatically, so every snapshot captures the wallets alongside the save and restoring puts both back together. It is optional and everything works without it, but the wallet file is the only record of who owns what, and nothing else can recover it if it is lost.

## Config

`BepInEx/config/dazed.howtofish.nosharedmoney.cfg`

| Key | Default | Meaning |
| --- | --- | --- |
| `StartingMoney` | `0` | Balance for a player who has never played this save. |
| `WarnInChat` | `true` | Host only: announce in chat when a connected player is missing the mod. |
| `UnclaimedSalesGoToHost` | `true` | If something no player ever held is sold, credit the host instead of discarding the money. |
