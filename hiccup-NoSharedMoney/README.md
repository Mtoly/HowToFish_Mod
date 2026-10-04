# NoSharedMoney

How to Fish gives the whole crew one shared bank balance. This mod gives every player their own.

Sell a fish, the money goes to **you**. Buy a rod, it comes out of **your** wallet. Your balance follows your Steam account and survives rejoins and restarts.

## Everyone needs this mod

**All players must have NoSharedMoney installed, including the host.** Wallet balances are pushed to each client individually, and a client without the mod has no way to receive them.

If someone joins without it, the host gets a warning in the console and in chat. That player is not able to break anything, since the server still refuses any purchase they cannot afford, but their money display will be stuck on a stale number and they will not be able to buy anything until they install the mod.

## How it works

- Each player has their own balance, keyed by Steam ID.
- **Selling** credits whoever put the item in the sell box (or fed it to a quest NPC). If you catch a fish and hand it to a friend, they get paid for it, so loot can be passed around deliberately.
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
| `ShareBossMoney` | `true` | Split the money from anything a boss or mini boss drops, including its meat, evenly between everyone in the session. |
| `DuplicateBossMoney` | `false` | Pay every player the full sale price instead of a share of it. Ignored when `ShareBossMoney` is off. |
| `SplitBoatCosts` | `false` | Split the cost of the boat motor and radar between everyone instead of one player paying it all. |
| `ShowBalancesOnNameplates` | `true` | Show each player's balance under their name. Your own setting, only changes what you see. |
| `EnablePayCommand` | `true` | Allow `/pay` in chat. |

### Seeing what everyone has

Each player's balance appears under their name above their head, so you can tell at a glance who is flush and who is broke. This is a local setting: turn it off and only you stop seeing it.

### Sending money

`/pay <name or steam id> <amount>` in chat sends money to another player.

```
/pay Steve 500
!pay Steve 500
/pay 76561198000000000 500
```

Both `/pay` and `!pay` work. The game normally swallows anything starting with `/` unless you are the developer, so this mod lets its own command through and leaves every other slash command alone.

Type part of a name and press **Tab** to complete it. Press Tab again to cycle through everyone who matches, or on an empty `/pay ` to cycle through everyone.

Partial names still work without Tab, as long as they match only one person. You cannot send more than you have, and you cannot send zero or a negative amount. Every payment is announced in chat so nobody has to take your word for it.

### Sharing the cost of the boat

The motor and the radar benefit the whole crew but normally land on whoever clicks buy. Turn on `SplitBoatCosts` and the price is divided evenly instead. Everyone has to be able to afford their share, otherwise the purchase is refused and the log says who came up short.

### Boss money

Bosses are a group effort, so by default their drops pay everyone. When a boss or mini boss drop is sold, the money is split evenly between every player in the session rather than going to whoever carried it to the sell box.

This covers the body itself and the meat chunks a boss scatters on death. Everything else you catch or find still pays only you.

Whoever made the sale gets any remainder that does not divide evenly, so nothing is lost to rounding. Turn `ShareBossMoney` off to have boss drops pay the seller like everything else.

Set `DuplicateBossMoney` to `true` if you would rather everyone got the whole amount. A 1000 corpse then pays 1000 to each of four players instead of 250 each. This creates money rather than dividing it, so it makes bosses far more rewarding as the group grows. Use it if you find the split makes bosses feel worse the more friends you bring.

Both of these are read from the host only. Guests do not need matching settings, and changing yours has no effect unless you are hosting.

## Compatibility

Requires BepInEx 5. Built against How to Fish on Unity 6000.4 with FishNet networking.

The mod hooks the game's money functions by name and reports what it found on startup. If a game update moves something, the BepInEx log says exactly which hook failed instead of failing silently.
