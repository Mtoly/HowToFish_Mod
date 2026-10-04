# 第三方组件说明

## 发布载荷来源

- 项目：https://github.com/BepInEx/BepInEx
- 固定版本：https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5
- Windows x64 官方资产：`BepInEx_win_x64_5.4.23.5.zip`
- 官方资产 SHA-256：`82f9878551030f54657792c0740d9d51a09500eeae1fba21106b0c441e6732c4`
- 该 v5 标签源码许可：MIT License

发布包中的 BepInEx 载荷从上述官方资产逐字节提取，不从本地范例复制，也不修改官方文件。外部文件的固定 URL、长度和 SHA-256 记录在 `packaging/third-party-assets.tsv`；打包脚本会逐项校验。

## 官方包内组件

| 组件 | 固定版本 | 许可 | 来源 |
| --- | --- | --- | --- |
| BepInEx | 5.4.23.5 | MIT | https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5 |
| BepInEx.Harmony / HarmonyXInterop | `d4cdcb4cdeac14a0b77012165f5f5a9f5032a9fa` | MIT | https://github.com/BepInEx/BepInEx.Harmony/commit/d4cdcb4cdeac14a0b77012165f5f5a9f5032a9fa |
| HarmonyX | 2.9.0 | MIT；保留原 Harmony MIT 声明 | https://github.com/BepInEx/HarmonyX/tree/v2.9.0 |
| Mono.Cecil | 0.10.4 | MIT | https://github.com/jbevain/cecil/tree/0.10.4 |
| MonoMod.RuntimeDetour / MonoMod.Utils | 22.1.29.1 | MIT | https://github.com/MonoMod/MonoMod/tree/v22.01.29.01 |
| UnityDoorstop | 4.5.0 | LGPL-2.1 | https://github.com/NeighTools/UnityDoorstop/tree/v4.5.0 |

虽然本 Mod 自身不使用 Harmony，官方 BepInEx ZIP 仍包含上表程序集，因此发布包保留对应许可文本。

UnityDoorstop 4.5.0 的 `winhttp.dll` 使用 LGPL-2.1。手动安装 ZIP 的 `licenses/source` 目录同时提供固定标签的完整机器可读源码归档，以满足未修改二进制再分发时的源码提供要求。

本文件是组件来源与许可汇总，不替代发布包 `licenses` 目录中的完整许可正文。
