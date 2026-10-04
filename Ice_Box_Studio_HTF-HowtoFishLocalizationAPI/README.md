**Note: This description is bilingual. The Chinese section is provided below the English section.**  
**说明：本描述为中英双语版本，中文内容位于英文内容下方。**

---

# How to Fish Localization API (English)
How to Fish Localization API is a shared dependency API for How to Fish mods. It lets compatible mods load their own language files and update their UI when the game language changes.

## Main Features
- Loads an external JSON localization file for each compatible mod.
- Reads the active How to Fish language and raises `LanguageChanged` after a language change.
- Provides English fallback when a language or key is missing.
- Localizes compatible configuration labels in `HowToFish.ModMenu` and `HTFModManager`.

## For Players
This mod is an API/dependency. It does not add gameplay features by itself.  
Install it when another How to Fish mod lists How to Fish Localization API as a requirement.

The configuration-label integration is optional. Mods that do not register localization data are left unchanged.

## For Mod Authors
Reference `HowtoFishLocalizationAPI.dll` and add a hard BepInEx dependency:

```csharp
using System.IO;
using System.Reflection;
using BepInEx;
using HowtoFishLocalizationAPI.Api;

[BepInPlugin(PluginInfo.PLUGIN_GUID, "My Mod", "1.0.0")]
[BepInDependency(HowtoFishLocalizationAPI.PluginInfo.PLUGIN_GUID)]
public sealed class MyPlugin : BaseUnityPlugin
{
    private ModLocalizer _localizer;

    private void Awake()
    {
        string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        _localizer = LocalizationApi.For(PluginInfo.PLUGIN_GUID);
        _localizer.RegisterJson(Path.Combine(directory, "MyMod.Localization.json"));
        LocalizationApi.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string language)
    {
        // Refresh this mod's existing UI text here.
    }
}
```

Load the JSON file from the directory containing your mod DLL. This also works with generated mod-manager folder names:

```csharp
using System.IO;
using System.Reflection;
using HowtoFishLocalizationAPI.Api;

internal static class I18n
{
    private const string FileName = "MyMod.Localization.json";
    private static readonly ModLocalizer Localizer = Load();

    internal static string Text(string key, params object[] args)
    {
        return Localizer.GetLocalizedText(key, args);
    }

    private static ModLocalizer Load()
    {
        string directory = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location);
        ModLocalizer localizer = LocalizationApi.For(PluginInfo.PLUGIN_GUID);
        localizer.RegisterJson(Path.Combine(directory, FileName));
        return localizer;
    }
}
```

Use `I18n.Text("your.key")` for mod UI text. The `args` parameters are optional and use standard `string.Format` placeholders.

### Configuration Labels

Compatible configuration managers use the following key format:

```text
modmenu.config.<Section>.<Key>
```

For example, a `Controls / OpenWallet` entry uses `modmenu.config.Controls.OpenWallet`. The label is applied when the configuration row is created, and missing keys keep the original configuration label.

Example localization file:

```json
{
  "en": {
    "mod.name": "My Mod",
    "mod.description": "A short description of my mod.",
    "modmenu.config.General.Enabled": "General / Enabled"
  },
  "zh-CN": {
    "mod.name": "我的模组",
    "mod.description": "我的模组简介。",
    "modmenu.config.General.Enabled": "常规 / 启用"
  }
}
```

Every localization file must contain an `en` object. If the current language or requested key is missing, the API tries `en`; if the key is still missing, it returns the key itself.

## Supported Language Codes
- English: `en`
- Swedish: `sv`
- Simplified Chinese: `zh-CN`
- Traditional Chinese: `zh-TW`
- French: `fr`
- German: `de`
- Italian: `it`
- Japanese: `ja`
- Korean: `ko`
- Polish: `pl`
- Brazilian Portuguese: `pt-BR`
- Russian: `ru`
- Latin American Spanish: `es-MX`
- European Spanish: `es-ES`
- Turkish: `tr`
- Ukrainian: `uk`

## Compatibility
- Game: How to Fish 1.0.4+
- ModMenu: 0.3.3+
- HFModManager: 1.0.1

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

# How to Fish Localization API (中文)
How to Fish Localization API 是供 How to Fish 模组使用的共享依赖 API。它让兼容模组加载自己的语言文件，并在游戏语言切换后更新模组 UI。

## 主要功能
- 让每个兼容模组加载自己的外部 JSON 本地化文件。
- 读取 How to Fish 当前语言，并在语言切换后触发 `LanguageChanged` 事件。
- 当前语言或 key 缺失时回退到英语。
- 为 `HowToFish.ModMenu` 和 `HTFModManager` 中的兼容配置项提供标签本地化。

## 给玩家
这是一个 API/依赖模组，本身不会添加玩法内容。  
当其他 How to Fish 模组要求安装 How to Fish Localization API 时，请安装它。

配置项标签兼容属于可选功能。没有注册本地化数据的模组不会被修改。

## 给模组作者
在项目中引用 `HowtoFishLocalizationAPI.dll`，并添加 BepInEx 硬依赖：

```csharp
using System.IO;
using System.Reflection;
using BepInEx;
using HowtoFishLocalizationAPI.Api;

[BepInPlugin(PluginInfo.PLUGIN_GUID, "My Mod", "1.0.0")]
[BepInDependency(HowtoFishLocalizationAPI.PluginInfo.PLUGIN_GUID)]
public sealed class MyPlugin : BaseUnityPlugin
{
    private ModLocalizer _localizer;

    private void Awake()
    {
        string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        _localizer = LocalizationApi.For(PluginInfo.PLUGIN_GUID);
        _localizer.RegisterJson(Path.Combine(directory, "MyMod.Localization.json"));
        LocalizationApi.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string language)
    {
        // 在这里刷新模组已经创建的 UI 文本。
    }
}
```

从模组 DLL 自己所在的目录加载 JSON。这样即使模组管理器生成了不同的插件文件夹名，也能正确找到语言文件：

```csharp
using System.IO;
using System.Reflection;
using HowtoFishLocalizationAPI.Api;

internal static class I18n
{
    private const string FileName = "MyMod.Localization.json";
    private static readonly ModLocalizer Localizer = Load();

    internal static string Text(string key, params object[] args)
    {
        return Localizer.GetLocalizedText(key, args);
    }

    private static ModLocalizer Load()
    {
        string directory = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location);
        ModLocalizer localizer = LocalizationApi.For(PluginInfo.PLUGIN_GUID);
        localizer.RegisterJson(Path.Combine(directory, FileName));
        return localizer;
    }
}
```

在模组 UI 文本中使用 `I18n.Text("your.key")`。`args` 参数是可选的，使用标准 `string.Format` 占位符。

### 配置项标签

兼容的配置管理器使用以下 key 格式：

```text
modmenu.config.<Section>.<Key>
```

例如，`Controls / OpenWallet` 配置项使用 `modmenu.config.Controls.OpenWallet`。配置行创建时会应用对应标签；缺少 key 时保留原始配置项名称。

本地化文件示例：

```json
{
  "en": {
    "mod.name": "My Mod",
    "mod.description": "A short description of my mod.",
    "modmenu.config.General.Enabled": "General / Enabled"
  },
  "zh-CN": {
    "mod.name": "我的模组",
    "mod.description": "我的模组简介。",
    "modmenu.config.General.Enabled": "常规 / 启用"
  }
}
```

每个本地化文件都必须包含 `en` 对象。当前语言或目标 key 缺失时，API 会尝试 `en`；如果英语中仍缺少该 key，则直接返回 key 本身。

## 支持的语言代码
- 英语：`en`
- 瑞典语：`sv`
- 简体中文：`zh-CN`
- 繁体中文：`zh-TW`
- 法语：`fr`
- 德语：`de`
- 意大利语：`it`
- 日语：`ja`
- 韩语：`ko`
- 波兰语：`pl`
- 巴西葡萄牙语：`pt-BR`
- 俄语：`ru`
- 拉丁美洲西班牙语：`es-MX`
- 欧洲西班牙语：`es-ES`
- 土耳其语：`tr`
- 乌克兰语：`uk`

## 兼容性
- 游戏：How to Fish 1.0.4+
- ModMenu：0.3.3+
- HFModManager：1.0.1

## Bug 提交 & 新功能建议
如果你有任何问题或新功能建议，请通过 [GitHub Issues](https://github.com/ibox233/IceBox_Mods_Issues) 提交，也可以通过 Discord：`iceboxcool`，或邮箱 `764884112@qq.com`、`ibox2333@gmail.com` 联系我。
