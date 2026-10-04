**Note: This description is bilingual. The Chinese section is provided below the English section.**  
**说明：本描述为中英双语版本，中文内容位于英文内容下方。**

---

# HowtoFishIdentityAPI (English)
HowtoFishIdentityAPI is a shared identity verification API for How to Fish mods. It gives server-authoritative mods a stable, verified player key.

## Main Features
- Verifies Steam players with the SteamID confirmed by the game's Steam transport.
- Verifies non-Steam players with a local RSA key and a server-issued challenge.
- Exposes a stable key such as `steam:<SteamID>` or `key:<public-key-hash>` after verification.
- Uses dedicated FishNet broadcasts for the handshake.
- Provides a verification event so dependent mods can initialize server-side player data.

## For Players
This mod is an API/dependency. It does not add gameplay, UI, commands, or currency by itself.  
Install it when another How to Fish mod lists HowtoFishIdentityAPI as a requirement. If the dependent mod is multiplayer, the host and participating clients should install the required dependency as instructed by that mod.

Non-Steam identity keys are stored locally in:

```text
Saves/HowtoFishIdentityAPI.identity
```

Do not share this file. A copied non-Steam identity file can be used to impersonate that identity. This API confirms possession of the key; it is not a permission, ban, or anti-cheat system.

## For Mod Authors
Reference `HowtoFishIdentityAPI.dll` and add a hard BepInEx dependency:

```csharp
using BepInEx;
using FishNet.Connection;
using HowtoFishIdentityAPI.Api;

[BepInPlugin(PluginInfo.PLUGIN_GUID, "My Mod", "1.0.0")]
[BepInDependency("IceBoxStudio.HowToFish.IdentityAPI", "1.0.1")]
public sealed class MyPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        IdentityApi.Verified += OnVerified;
    }

    private void OnVerified(NetworkConnection connection)
    {
        if (IdentityApi.TryGetKey(connection, out string key))
        {
            // Load or create server-side data using key.
        }
    }
}
```

Useful API methods:

- `IdentityApi.IsVerified(NetworkConnection)` checks whether a connection has completed verification.
- `IdentityApi.IsVerified(Player)` checks a player's current connection.
- `IdentityApi.TryGetKey(NetworkConnection, out string key)` returns the verified stable key.
- `IdentityApi.TryGetKey(Player, out string key)` returns the verified key for a player.
- `IdentityApi.TryGetLocalKey(Player, out string key)` resolves the local player's key on the client for UI filtering; it is not a server authorization check.
- `IdentityApi.IsSteam(NetworkConnection)` and `IdentityApi.IsSteam(Player)` identify Steam transport identities.
- `IdentityApi.Verified` fires on the server after a connection is verified.

The identity handshake uses `IdentityHelloBroadcast`, `IdentityChallengeBroadcast`, and `IdentityProofBroadcast`. Do not patch or read `Server.RpcReader___SendChatMessage___3264264606` for custom protocols.

## Compatibility
- Game version: 1.0.12+
- BepInEx: 5.4.23.5+

## Bug Reports & Feature Suggestions
If you have any questions or feature suggestions, please submit them through [GitHub Issues](https://github.com/ibox233/IceBox_Mods_Issues), contact me on Discord at `iceboxcool`, or email `764884112@qq.com` or `ibox2333@gmail.com`.

---

<div align="center">

If you enjoy my mods, feel free to support me! / 如果你喜欢我的模组，请支持我一下吧！

<a href="https://ko-fi.com/I3I1WNP4">
  <img src="https://storage.ko-fi.com/cdn/kofi3.png?v=6" width="320" alt="Ko-fi">
</a>
&nbsp;
<a href="https://www.ifdian.net/a/iceboxstudio">
  <img src="https://temp-rr-icebox.cn-nb1.rains3.com/ifdian.png" width="320" alt="爱发电">
</a>

</div>

---

# HowtoFishIdentityAPI（中文）
HowtoFishIdentityAPI 是供 How to Fish 模组使用的共享身份验证 API。它为需要服务器权威身份的模组提供稳定、经过验证的玩家标识。

## 主要功能
- 使用游戏 Steam 传输确认的 SteamID 验证 Steam 玩家。
- 使用客户端本地 RSA 密钥和服务器随机挑战验证非 Steam 玩家。
- 验证完成后提供稳定身份键，例如 `steam:<SteamID>` 或 `key:<公钥哈希>`。
- 使用独立 FishNet Broadcast 完成握手。
- 提供身份验证完成事件，方便其他模组在服务器端初始化玩家数据。

## 给玩家
这是一个 API/依赖模组，本身不会添加玩法、UI、命令或金钱功能。  
当其他 How to Fish 模组要求安装 HowtoFishIdentityAPI 时，请按该模组的说明安装。联机模组通常需要房主和参与游戏的客户端都安装所需依赖。

非 Steam 身份密钥保存在：

```text
Saves/HowtoFishIdentityAPI.identity
```

请不要分享这个文件。复制非 Steam 身份文件后，其他人可能冒用对应身份。本 API 只确认客户端是否持有对应密钥，不负责权限、封禁或反作弊。

## 给模组作者
在项目中引用 `HowtoFishIdentityAPI.dll`，并添加 BepInEx 硬依赖：

```csharp
using BepInEx;
using FishNet.Connection;
using HowtoFishIdentityAPI.Api;

[BepInPlugin(PluginInfo.PLUGIN_GUID, "My Mod", "1.0.0")]
[BepInDependency("IceBoxStudio.HowToFish.IdentityAPI", "1.0.1")]
public sealed class MyPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        IdentityApi.Verified += OnVerified;
    }

    private void OnVerified(NetworkConnection connection)
    {
        if (IdentityApi.TryGetKey(connection, out string key))
        {
            // 使用 key 加载或创建服务器端玩家数据。
        }
    }
}
```

常用 API：

- `IdentityApi.IsVerified(NetworkConnection)`：判断连接是否已完成验证。
- `IdentityApi.IsVerified(Player)`：判断玩家当前连接是否已完成验证。
- `IdentityApi.TryGetKey(NetworkConnection, out string key)`：读取已验证的稳定身份键。
- `IdentityApi.TryGetKey(Player, out string key)`：读取玩家已验证的身份键。
- `IdentityApi.TryGetLocalKey(Player, out string key)`：在客户端读取本地玩家身份键，用于界面筛选；不能替代服务器权限校验。
- `IdentityApi.IsSteam(NetworkConnection)` 和 `IdentityApi.IsSteam(Player)`：判断身份是否来自 Steam 传输。
- `IdentityApi.Verified`：服务器完成身份验证后触发。

身份握手使用 `IdentityHelloBroadcast`、`IdentityChallengeBroadcast` 和 `IdentityProofBroadcast`。不要为了实现自定义协议而补丁或读取 `Server.RpcReader___SendChatMessage___3264264606`。

## 兼容性
- 游戏版本：1.0.12+
- BepInEx：5.4.23.5

## Bug 提交 & 新功能建议
如果你有任何问题或新功能建议，请通过 [GitHub Issues](https://github.com/ibox233/IceBox_Mods_Issues) 提交，也可以通过 Discord：`iceboxcool`，或邮箱 `764884112@qq.com` 或 `ibox2333@gmail.com` 联系我。
