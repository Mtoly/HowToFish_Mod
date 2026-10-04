# HowToFish 综合性能优化

**模组名称（中文）**：HowToFish 综合性能优化
**Mod Name (English)**：HowToFish Comprehensive Performance Optimization
**当前版本（Version）**：v1.5.4

> 说明：本文件上半部分为中文说明，下方另附完整英文版说明。
> Note: This document has a Chinese version (below the header) followed by a full English version at the bottom.

---

# 一、简介

该模组是《How to Fish》的纯性能优化插件。不修改任何游戏文件、不接触原版模组、不影响掉落物与存档、不改变画质，通过几项相互独立的优化提升运行流畅度，尤其针对「跨岛卡顿」与「首次使用卡顿」。

---

# 二、功能特性

| 功能 | 作用 |
|------|------|
| JIT 预热 | 启动时预编译所有模组方法（只编译、不执行），消除每个模组首次使用时的卡顿；自动覆盖之后新装的模组 |
| 日志削减 | 关闭每帧日志的栈生成，降低 GC 频率与 CPU 占用 |
| 温和垃圾清理 | 定期吸收临时内存垃圾，平滑 GC 停顿，让游戏更流畅 |
| 岛屿早加载 | 加大跨岛触发距离，靠岸前提前流式加载新岛，减轻跨岛卡顿 |

---

# 三、安装

1. 打开游戏目录 `...\How to Fish\How to Fish\BepInEx\plugins`
2. 将 `HowToFish.ComprehensivePerformanceOptimization.dll` 放入其中
3. 启动游戏，模组自动加载

环境要求：BepInEx 5，游戏运行于 D3D12。

---

# 四、配置文件

文件路径：`BepInEx\config\HowToFish.ComprehensivePerformanceOptimization.cfg`

| 节点 | 选项 | 默认 | 说明 |
|------|------|:---:|------|
| Logging | ReduceLogSpam | true | 削减每帧日志栈生成 |
| GC | IdleCollect | true | 温和垃圾清理 |
| GC | CollectIntervalSeconds | 20 | 清理间隔（秒） |
| Warmup | EnableJitWarmup | true | JIT 预热 |
| IslandEarlyLoad | EnableEarlyLoadRange | true | 岛屿早加载 |
| IslandEarlyLoad | RangeMultiplier | 2.0 | 触发距离放大倍数 |

所有选项均可独立开关，改完重启生效。

---

# 五、注意事项

- 该模组不删除掉落物、不清理存档、不影响任何持久数据。「垃圾清理/GC」只回收程序临时内存。
- 岛屿早加载改变了岛屿触发行为，属实验性；如有异常可将 `EnableEarlyLoadRange` 设为 `false` 还原。
- 低风险、可一键还原。

---

# 六、兼容与日志

- 启动日志 `LogOutput.log` 会输出 `Optimize: loaded`，开启的优化各有对应条目。
- 兼容其他所有插件，不修改原版 `ArceDev.FishEffects.dll` 等。

---

---

# HowToFish Comprehensive Performance Optimization

English version (full documentation).

---

## 1. Overview

This is a pure-performance plugin for "How to Fish". It does not modify any game files, does not touch vanilla mods, does not affect loot or saves, and does not change visual quality. It improves smoothness through several independent optimizations, especially targeting island-crossing stutter and first-use hitch.

---

## 2. Features

| Feature | What it does |
|---------|-------------|
| JIT Warmup | Precompiles every installed mod's methods at startup (compile-only, never run) to remove first-use stutter. Automatically covers mods added later. |
| Log Trim | Strips per-frame stack-trace population to cut GC rate and CPU usage. |
| Gentle GC | Periodically drains garbage at low pressure to smooth out GC pauses. |
| Island Early-Load | Enlarges the island load trigger distance so the next island streams earlier, reducing crossing stutter. |

---

## 3. Installation

1. Open the game folder `...\How to Fish\How to Fish\BepInEx\plugins`
2. Copy `HowToFish.ComprehensivePerformanceOptimization.dll` into it
3. Launch the game; the mod loads automatically

Requirements: BepInEx 5, game running on D3D12.

---

## 4. Configuration

File: `BepInEx\config\HowToFish.ComprehensivePerformanceOptimization.cfg`

| Section | Key | Default | Description |
|---------|-----|:---:|-------------|
| Logging | ReduceLogSpam | true | Cut per-frame stack-trace population |
| GC | IdleCollect | true | Gentle garbage collection |
| GC | CollectIntervalSeconds | 20 | Collect interval in seconds |
| Warmup | EnableJitWarmup | true | JIT warmup |
| IslandEarlyLoad | EnableEarlyLoadRange | true | Island early-load range |
| IslandEarlyLoad | RangeMultiplier | 2.0 | Range multiplier |

All options are independent and take effect after restart.

---

## 5. Notes

- This mod never removes loot, never cleans saves, and never affects persistent data. "GC" only reclaims transient memory.
- Island early-load changes island trigger behaviour (experimental). Set `EnableEarlyLoadRange=false` to restore defaults if needed.
- Low risk and fully revertible.

---

## 6. Compatibility & Logs

- On startup, `LogOutput.log` prints `Optimize: loaded`, with one entry per enabled optimization.
- Compatible with all other mods; it does not modify vanilla DLLs such as `ArceDev.FishEffects.dll`.