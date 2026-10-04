# MorePlayers compatibility findings

## Inputs

- Game assembly: `E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\How to Fish_Data\Managed\Assembly-CSharp.dll`
- Game assembly SHA-256: `1A5B77E661992399E99D65E96037C06D18836AF16DD7A63FB843C514FFF76508`
- Original plugin: `D:\Code\How2fish\Kalamies-MorePlayers\MorePlayers.dll`
- Original plugin SHA-256: `082648207F272DC7EFBFFCA69933A5D48ABC903A30A711CA2E1399490C5AFC72`

## Current game method

`SteamManager.CreateLobby()` still exists with signature `System.Void SteamManager::CreateLobby()`.
Its relevant IL is:

```text
IL_0013: ldc.i4.0
IL_0014: br.s         IL_0017
IL_0016: ldc.i4.1
IL_0017: ldc.i4.8
IL_0018: call         Steamworks.SteamAPICall_t Steamworks.SteamMatchmaking::CreateLobby(Steamworks.ELobbyType,System.Int32)
```

`IL_0017` loads the original lobby limit (`8`) and is also a branch target.

## Root cause

`MorePlayers.CreateLobby_Patch.Transpiler` correctly locates the `SteamMatchmaking.CreateLobby` call and the preceding constant, but replaces the preceding `CodeInstruction` with a newly constructed instance. Harmony stores the branch label targeting `IL_0017` on the original instruction. Replacing that instruction drops the label, so the generated dynamic method contains a branch to an unmarked label.

The original plugin reproduces the failure during `Harmony.PatchAll`:

```text
RESULT=FAIL_HARMONY_PATCHALL
ERROR_TYPE=HarmonyLib.HarmonyException
ERROR_MESSAGE=IL Compile Error (unknown location)
System.ArgumentException: ILGenerator 中的错误标签内容。
```

This is the same failure class as `ArgumentException: Label #2 is not marked`.

## Fix

The patched Transpiler keeps the original `CodeInstruction` object and mutates only:

- `CodeInstruction.opcode` to `OpCodes.Call`
- `CodeInstruction.operand` to the `MorePlayers.Plugin.Limit` getter

The original labels and exception blocks therefore remain attached.

## Verification

- Original plugin: runtime Harmony patch fails, exit `1`.
- Patched plugin: runtime Harmony patch succeeds, exit `0`.
- Patched structure: no replacement `new CodeInstruction`; one in-place opcode write and one in-place operand write.
- Rollback test: restored file SHA-256 equals the original plugin SHA-256.

## Installed startup verification

The fixed DLL was installed at `BepInEx\plugins\Kalamies-MorePlayers\MorePlayers.dll`. A game startup produced:

```text
[Info   :   BepInEx] Loading [MorePlayers 0.1.0]
[Info   :MorePlayers] MorePlayers 0.1.0 loaded. MaxPlayers=32
[Message:   BepInEx] Chainloader startup complete
```

The startup log contains none of `Label #2`, `ArgumentException`, `HarmonyException`, or `IL Compile Error`.

## Remaining runtime coverage

A live Steam multiplayer session is still needed to verify lobby creation, joining from a second client, and membership beyond eight players.
