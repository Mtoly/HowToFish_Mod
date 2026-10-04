# MorePlayers current-version compatibility task

## Status

- [x] Module 1: Establish the current game assembly baseline
- [x] Module 2: Disassemble and compare `SteamManager.CreateLobby`
- [x] Module 3: Repair the Transpiler and verify Harmony patch application
- [ ] Module 4: Install into the game and verify live lobby host/join behavior (startup complete; multiplayer session pending)
- [x] Module 5: Produce diff, verification record, and tested rollback

## Module 1: Baseline (complete)

- Game root: `E:\SteamLibrary\steamapps\common\How to Fish\How to Fish`
- Assembly located and copied to `analysis\current-game\Assembly-CSharp.dll`
- Assembly SHA-256 recorded in `analysis\current-game\Assembly-CSharp.sha256`
- Original plugin preserved unchanged at `MorePlayers.dll`

## Module 2: Disassembly and comparison (complete)

- Exported current `SteamManager` IL to `analysis\current-game\SteamManager.il.txt`
- Exported original plugin IL to `analysis\current-game\MorePlayers.il.txt`
- Confirmed current `CreateLobby()` calls `SteamMatchmaking.CreateLobby(ELobbyType, Int32)`
- Confirmed the lobby size constant is `ldc.i4.8` at `IL_0017`
- Confirmed `IL_0017` carries a branch target from `IL_0014`
- Root cause documented in `analysis\current-game\compatibility-findings.md`

## Module 3: Transpiler repair (complete)

- Replaced new-instruction assignment with in-place `opcode` and `operand` writes
- Preserved labels attached to the original lobby-size instruction
- Original runtime result: Harmony patch fails with invalid/unmarked label, exit `1`
- Fixed runtime result: Harmony `PatchAll` succeeds, exit `0`
- Fixed plugin: `build\MorePlayers.fixed.dll`

## Module 4: Live game validation (startup complete; multiplayer session pending)

The fixed build is installed at `BepInEx\plugins\Kalamies-MorePlayers\MorePlayers.dll` with SHA-256 `1CF59B23A4288D880B1179CFBB32E7AB381F1AC9E4F72479746C18C5CF1188E5`.

- [x] Game startup without a Harmony/MonoMod IL exception
- [x] BepInEx log reports `Loading [MorePlayers 0.1.0]`
- [x] Plugin log reports `MorePlayers 0.1.0 loaded. MaxPlayers=32`
- [x] No `Label #2`, `ArgumentException`, `HarmonyException`, or `IL Compile Error` in the startup log
- [ ] Creation of a Steam lobby
- [ ] Joining the lobby from a second client
- [ ] A session with more than eight members

The test process exited on its own within 20 seconds. Startup compatibility is verified; the remaining checks require an interactive Steam multiplayer session.

## Module 5: Delivery and rollback (complete)

- Modified file: `build\MorePlayers.fixed.dll`
- IL diff: `build\MorePlayers.patch.diff`
- Verification: `build\VERIFICATION.txt`
- Rollback: `build\ROLLBACK.ps1`
- Rollback was tested on `build\rollback-test\MorePlayers.dll`; restored SHA-256 matches the original.
- The baseline-absent rollback mode was tested on an independent directory; it removed the installed test directory and exited `0`.
