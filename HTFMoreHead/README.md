# HTF MoreHead

[中文](#中文说明) · [English](#english)

`HTF MoreHead` is a BepInEx cosmetic framework for **How to Fish**. Content creators build independent `.htfhhh` AssetBundles with the Unity SDK; players select them through the game's original Hat, Accessory, and Outfit arrows.

Current release: **0.6.1**, network protocol **3**, tested against How to Fish **1.0.11**.

## 中文说明

### 功能

- 支持三个原生外观分类：`帽子 Hat`、`配饰 Accessory`、`衣服 Outfit`。
- 自定义物品直接接入各分类的原版左右箭头，不修改原版 `SkinManager` 列表。
- 递归扫描 `BepInEx/plugins/**/*.htfhhh`；一个装扮包就是一个文件。
- 游戏内显示彩色的 `{Display Name} By {Author Name}`。
- 三个槽位分别本地持久化，重新启动游戏后恢复。
- 基于 FishNet Broadcast 的 Host 权威同步，支持换装增量、断线清理与晚加入快照。
- 联机仅传稳定物品 ID，不传模型。缺少内容包不会断线，只在该客户端隐藏对应装扮。
- `0.6.1` 使用网络协议 `3`，所有联机成员必须使用相同 Core 版本。

### 玩家安装

1. 安装 BepInEx `5.4.23.5`。
2. 下载 Release，将 `HTFMoreHead.dll` 放入：

   ```text
   How to Fish/BepInEx/plugins/HTFMoreHead/
   ```

3. 将任意 `.htfhhh` 放在 `BepInEx/plugins/` 的任意子目录，例如：

   ```text
   How to Fish/BepInEx/plugins/HTFMoreHead/Content/猪猪眼罩.htfhhh
   ```

4. 完整重启游戏，进入角色自定义界面。
5. 打开对应的帽子、配饰或衣服页面，继续使用原版 `< >` 按钮。越过原版列表边界后会进入该分类的自定义物品。
6. 点击原版“返回”保存。选择记录写入：

   ```text
   BepInEx/config/HTFMoreHead/loadout.json
   ```

### 内容作者 SDK

不再使用 `.unitypackage`。请在 Unity Package Manager 选择 `Install package from git URL...` 并粘贴：

```text
https://github.com/XiaoHaiiOvO/HTFMoreHeadSDK.git#v0.6.0
```

完整的中英文制作教程、三个分类的准确挂点和构建限制见 [HTF MoreHead SDK](https://github.com/XiaoHaiiOvO/HTFMoreHeadSDK)。Builder 会自动生成联机/存档所需的稳定内部 ID，并且每次只输出一个 `{DisplayName}.htfhhh` 文件。

### 三分类工作方式

| 自定义分类 | SDK 校对挂点 | 游戏内选择页 | 远端运行时挂点 |
|---|---|---|---|
| Hat | `Armature/Body/Head/HTFMoreHead_HeadAnchor` | 帽子 | `Player.Body.Head` |
| Accessory | 参考角色根节点 `Accessory` | 配饰 | 原版 Accessory renderer Transform |
| Outfit | 参考角色根节点 `Outfit` | 衣服/外观 | 原版 Outfit renderer Transform |

当前三个分类都使用刚性 `MeshFilter + MeshRenderer` 装扮。需要随骨骼变形的衣服和长发仍需后续增加 `SkinnedMeshRenderer` 骨骼重定向，不能直接用本版 Builder 打包。

### 联机说明

- 所有玩家必须安装同一协议版本的 Core；`0.6.1` 不与协议 2 混用。
- 内容包可以不同。没有某件 `.htfhhh` 的客户端不会显示它，但仍可正常游戏。
- Host 从 FishNet 的真实连接确定玩家身份，维护 Hat/Accessory/Outfit 三槽快照。
- 第一次进入和晚加入都会请求完整 Snapshot；换装使用 Reliable 增量消息。
- 自定义模型只实例化为本地纯视觉对象，不注册 NetworkObject，不修改游戏 DLL。

### 从源码构建

工程目标为 `netstandard2.1`，引用当前游戏、FishNet、BepInEx、Harmony 和 Unity Managed DLL；宿主引用全部为 `Private=false`。

```powershell
dotnet build .\HTFMoreHead.csproj -c Release --nologo `
  -p:GameRoot='D:\SteamLibrary\steamapps\common\How to Fish\How to Fish'
```

输出：

```text
bin/Release/netstandard2.1/HTFMoreHead.dll
```

### 当前限制

- 本机第一人称不在摄像机前挂载头部装扮；本机外观通过角色自定义预览显示。
- 尸体和结算画面的独立角色目前不会复制自定义装扮。
- 当前仅支持刚性模型，不支持蒙皮骨骼重定向。
- 游戏更新后如果 Hook 或运行时指纹改变，应先通过日志重新验证兼容性。

### 特别鸣谢与授权

本项目在获得 **Masaicker** 明确许可的前提下，参考了其 R.E.P.O. Mod「MoreHead」的架构与开发思路。感谢 Masaicker 的授权、开源贡献以及对 Mod 社区的帮助。

- 原版作者：Masaicker
- 原版仓库：[Masaicker/repo-MoreHead](https://github.com/Masaicker/repo-MoreHead)
- 原仓库源码许可证：[MIT License](https://github.com/Masaicker/repo-MoreHead/blob/main/LICENSE)
- 原仓库资源说明：[ASSET_LICENSE.txt](https://github.com/Masaicker/repo-MoreHead/blob/main/ASSET_LICENSE.txt)

上述鸣谢和授权记录不替代原仓库许可证。若复用其 MIT 源码，应继续保留相应版权与许可文本。

## English

### Features

- Integrates custom `Hat`, `Accessory`, and `Outfit` items with the matching native arrow selectors.
- Recursively loads single-file `.htfhhh` packages from `BepInEx/plugins/`.
- Displays `{Display Name} By {Author Name}` in the customization preview.
- Persists all three custom slots across restarts.
- Uses a host-authoritative FishNet Broadcast protocol with reliable deltas, disconnect cleanup, and late-join snapshots.
- Synchronizes stable cosmetic IDs only. Missing local packages are tolerated.

### Player installation

1. Install BepInEx `5.4.23.5`.
2. Put `HTFMoreHead.dll` in `BepInEx/plugins/HTFMoreHead/`.
3. Put `.htfhhh` files anywhere below `BepInEx/plugins/`.
4. Restart the game. In character customization, use the original arrows on the matching Hat, Accessory, or Outfit tab.
5. Use the game's Back button to save the current custom loadout.

Every multiplayer participant must use the same Core protocol. Content packages may differ; a missing package only hides that item on the affected client.

### Creator SDK

Install the SDK from Unity Package Manager with:

```text
https://github.com/XiaoHaiiOvO/HTFMoreHeadSDK.git#v0.6.0
```

See [HTF MoreHead SDK](https://github.com/XiaoHaiiOvO/HTFMoreHeadSDK) for the bilingual step-by-step tutorial. The builder automatically creates stable internal IDs, bakes anchor-relative transforms, and outputs exactly one `{DisplayName}.htfhhh` file.

### Build from source

```powershell
dotnet build .\HTFMoreHead.csproj -c Release --nologo `
  -p:GameRoot='D:\SteamLibrary\steamapps\common\How to Fish\How to Fish'
```

### Credits

This project references the design of Masaicker's R.E.P.O. mod [MoreHead](https://github.com/Masaicker/repo-MoreHead) with **Masaicker's explicit permission**. Thank you to Masaicker for the authorization and open-source contribution.
