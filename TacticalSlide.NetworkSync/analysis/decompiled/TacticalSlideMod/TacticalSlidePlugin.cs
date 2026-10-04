using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TacticalSlideMod;

[BepInPlugin("com.atomic.tacticalslide", "Tactical Slide & Slide-Hop Mod", "1.0.0")]
public class TacticalSlidePlugin : BaseUnityPlugin
{
	public const string PluginGuid = "com.atomic.tacticalslide";

	public const string PluginName = "Tactical Slide & Slide-Hop Mod";

	public const string PluginVersion = "1.0.0";

	private Harmony _harmony;

	internal static TacticalSlidePlugin Instance { get; private set; }

	internal static ManualLogSource Log
	{
		get
		{
			TacticalSlidePlugin instance = Instance;
			if (instance == null)
			{
				return null;
			}
			return ((BaseUnityPlugin)instance).Logger;
		}
	}

	public static ConfigEntry<bool> ModEnabled { get; private set; }

	public static ConfigEntry<KeyCode> SlideKey { get; private set; }

	public static ConfigEntry<KeyCode> AltSlideKey { get; private set; }

	public static ConfigEntry<float> SlideBoostMulti { get; private set; }

	public static ConfigEntry<float> SlideDuration { get; private set; }

	public static ConfigEntry<float> SlideHopBoost { get; private set; }

	public static ConfigEntry<bool> EnableFovWarp { get; private set; }

	public static ConfigEntry<bool> EnableCameraTilt { get; private set; }

	public static ConfigEntry<bool> EnableSlopeBoost { get; private set; }

	public static ConfigEntry<bool> EnableFirstPersonLegs { get; private set; }

	private void Awake()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Expected O, but got Unknown
		Instance = this;
		ModEnabled = ((BaseUnityPlugin)this).Config.Bind<bool>("01 - 常规设置", "Enabled", true, "启用战术滑铲与滑铲跳功能");
		SlideKey = ((BaseUnityPlugin)this).Config.Bind<KeyCode>("02 - 按键配置", "SlideKey", KeyCode.LeftControl, "滑铲主按键 (冲刺奔跑时按住或点按触发)");
		AltSlideKey = ((BaseUnityPlugin)this).Config.Bind<KeyCode>("02 - 按键配置", "AltSlideKey", KeyCode.C, "滑铲备用快捷键");
		SlideBoostMulti = ((BaseUnityPlugin)this).Config.Bind<float>("03 - 动作与动力学", "SlideBoostMulti", 1.6f, new ConfigDescription("起铲初始爆发速度倍率 (相对于冲刺速度)", (AcceptableValueBase)(object)new AcceptableValueRange<float>(1f, 2.5f), Array.Empty<object>()));
		SlideDuration = ((BaseUnityPlugin)this).Config.Bind<float>("03 - 动作与动力学", "SlideDuration", 0.85f, new ConfigDescription("单次滑铲最大持续秒数", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.4f, 1.5f), Array.Empty<object>()));
		SlideHopBoost = ((BaseUnityPlugin)this).Config.Bind<float>("03 - 动作与动力学", "SlideHopBoost", 1.15f, new ConfigDescription("滑铲跳 (Slide-Hop) 起跳水平初速继承加成", (AcceptableValueBase)(object)new AcceptableValueRange<float>(1f, 1.5f), Array.Empty<object>()));
		EnableSlopeBoost = ((BaseUnityPlugin)this).Config.Bind<bool>("03 - 动作与动力学", "EnableSlopeBoost", true, "启用下坡地形重力顺向滑行加速");
		EnableFovWarp = ((BaseUnityPlugin)this).Config.Bind<bool>("04 - 视觉与视效", "EnableFovWarp", true, "启用滑铲瞬间动态 FOV 广角推背风阻拉伸");
		EnableCameraTilt = ((BaseUnityPlugin)this).Config.Bind<bool>("04 - 视觉与视效", "EnableCameraTilt", true, "启用第一人称后仰/微侧倾与第三人称程序化 IK 滑行姿态");
		EnableFirstPersonLegs = ((BaseUnityPlugin)this).Config.Bind<bool>("04 - 视觉与视效", "EnableFirstPersonLegs", true, "启用第一人称下可见身体与腿部动作 (低头与滑铲时均可见双腿)");
		SlideController.Initialize();
		_harmony = new Harmony("com.atomic.tacticalslide");
		_harmony.PatchAll();
		ManualLogSource log = Log;
		if (log != null)
		{
			log.LogInfo((object)"================================================================================");
		}
		ManualLogSource log2 = Log;
		if (log2 != null)
		{
			log2.LogInfo((object)"  【渔力全开】战术滑铲与滑铲跳模组 (TacticalSlide) v1.0.0 已成功加载！");
		}
		ManualLogSource log3 = Log;
		if (log3 != null)
		{
			log3.LogInfo((object)"  >> 操作指南：冲刺奔跑中按下 [LeftControl] 或 [C] 触发贴地飞驰滑铲");
		}
		ManualLogSource log4 = Log;
		if (log4 != null)
		{
			log4.LogInfo((object)"  >> 进阶身法：滑铲中按 [Space] 触发【滑铲跳 (Slide-Hop)】，惯性 100% 继承至空中！");
		}
		ManualLogSource log5 = Log;
		if (log5 != null)
		{
			log5.LogInfo((object)"  >> 出品作者：哔哩哔哩-华丽的小柠檬");
		}
		ManualLogSource log6 = Log;
		if (log6 != null)
		{
			log6.LogInfo((object)"================================================================================");
		}
	}

	private void OnDestroy()
	{
		Harmony harmony = _harmony;
		if (harmony != null)
		{
			harmony.UnpatchSelf();
		}
	}
}
