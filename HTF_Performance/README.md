# 渔力全开锁帧优化mod

作者：镜桦izumik

这是为 Steam 游戏《How to Fish》制作的轻量性能 Mod。v1 只修复游戏原生 `MaxFPS` 限帧在启动、进入世界或场景切换后可能失效的问题，不改变画质、存档或联机数据。

## 当前状态

当前正式版本为 `v1.0.0`。它修复游戏原生限帧失效问题，并提供带作者署名、真实文件事务进度条和回滚保护的自动安装器；耗时写入在后台执行，窗口不会在安装期间失去响应。

正式版已通过 8/8 帧率策略测试、11/11 安装器夹具测试和 12/12 生产 ZIP 集成测试；安装器实际窗口检查确认署名、进度条、自动定位和只读清单显示正常。

用户在真实游戏 `Island1` 场景连续游玩 15 分钟，确认锁帧始终按设置的最高帧率运行；BepInEx 日志记录了插件启动和场景加载后的限帧修复，未出现本 Mod 的错误。该实测未覆盖完整联机矩阵，后续游戏更新仍需重新回归。

## 下载

正式版下载：[v1.0.0](https://github.com/MochizikuNanoka/how-to-fish-performance/releases/tag/v1.0.0)。GitHub 会清洗中文 Release 资产名，因此发布页提供 ASCII 名的 Windows 外层包；解压后仍是中文名的自动安装 EXE、手动安装 ZIP 和校验清单。

## 具体实现

运行时限帧策略、生命周期、安装事务、进度计算、打包和验证流程详见 [`docs/具体实现方法.md`](docs/具体实现方法.md)。

## 运行规则

- 只读取游戏自己的 `PlayerPrefs["MaxFPS"]` 和 `PlayerPrefs["VSync"]`。
- VSync 开启时不执行软件限帧。
- VSync 关闭时使用 `Application.targetFrameRate`。
- MaxFPS 缺失或不大于 0 时，本次运行使用当前显示器刷新率，不写回设置。
- 启动、场景加载后一帧以及每 1 秒检查一次；仅在值漂移时写入。

## 支持范围

- Windows 10/11 x64
- Steam 正版《How to Fish》
- Unity Mono 版游戏
- BepInEx 5.4.x；主要基于官方 BepInEx 5.4.23.5 验证

BepInEx 6、IL2CPP、Linux、Proton、Steam Deck 和非 Steam 版本不在正式支持范围内。

## 构建

### 其他 Mod 的开发环境

仓库已经包含可复用的 BepInEx 5 / Unity 6 Mono 开发层。首次使用时运行：

```powershell
.\tools\Initialize-Modding.ps1 -GameRoot 'E:\Program Files\Steam\steamapps\common\How to Fish\How to Fish'
```

它会验证游戏托管程序集、取得并校验开发用 BepInEx，然后生成仅供本机使用且不会提交的 `Directory.Build.local.props`。创建新 Mod：

```powershell
.\tools\New-Mod.ps1 -Name ViewDistance -DisplayName '渔力全开视野优化mod' -Description '调整远景显示距离。'
```

批量构建和显式部署分别使用：

```powershell
.\tools\Build-Mods.ps1
.\tools\Build-Mods.ps1 -Project 'src\HowToFish.ViewDistance\HowToFish.ViewDistance.csproj' -Deploy
```

不传 `-Deploy` 时不会写入游戏目录。共享引用、按需引用游戏程序集、测试与更新后的兼容检查详见 [`docs/Mod开发环境.md`](docs/Mod开发环境.md)。

### 锁帧 Mod 正式版构建

完整正式版打包还需要 .NET Framework 4.8 Targeting Pack。使用仓库根目录的 `build.ps1`；脚本会从 BepInEx 官方 GitHub Release 下载固定版本并验证 SHA-256。

自动安装 EXE 不内嵌载荷、不联网、不调用脚本，必须与同版本手动安装 ZIP 放在同一目录。当前构建没有 Authenticode 证书，运行时可能出现“未知发布者”或 SmartScreen 提示。

## 许可

本项目源码使用 MIT License。随手动包分发的 BepInEx 和其他第三方组件保留其各自许可，详见 `THIRD_PARTY_NOTICES.md`。
