using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.UI;
using CompleteCheatMenu.UI.Tabs;
using HarmonyLib;
using UnityEngine;

namespace CompleteCheatMenu;

[BepInPlugin("envincible.howtofish.completecheatmenu", "Complete Cheat Menu", "0.7.0")]
[BepInProcess("How to Fish.exe")]
public class Plugin : BaseUnityPlugin
{
	public const string Guid = "envincible.howtofish.completecheatmenu";

	public const string Name = "Complete Cheat Menu";

	public const string Version = "0.7.0";

	internal static ManualLogSource Log;

	private Harmony _harmony;

	internal static ConfigEntry<KeyCode> ToggleKey;

	internal static ConfigEntry<bool> AutoEnableCheats;

	internal static ConfigEntry<bool> BackupSaves;

	internal static ConfigEntry<bool> KeybindsEnabled;

	private bool _checkedOnce;

	private float _nextGateCheck;

	private void Awake()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		Log = Logger;
		ToggleKey = ((BaseUnityPlugin)this).Config.Bind<KeyCode>("General", "ToggleKey", KeyCode.Insert, "Opens and closes the cheat menu.");
		AutoEnableCheats = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "AutoEnableCheats", true, "Unlock the game's own developer command suite automatically when you load in.");
		BackupSaves = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "BackupSaves", true, "Copy the save folder once per session before the first destructive action. Silent, no prompt.");
		KeybindsEnabled = ((BaseUnityPlugin)this).Config.Bind<bool>("Keybinds", "EnableAllKeybinds", false, "Master switch for every cheat hotkey. The menu key is unaffected and always works.");
		Keybinds.Register(((BaseUnityPlugin)this).Config);
		_harmony = new Harmony("envincible.howtofish.completecheatmenu");
		try
		{
			_harmony.PatchAll(Assembly.GetExecutingAssembly());
			int num = 0;
			foreach (MethodBase patchedMethod in _harmony.GetPatchedMethods())
			{
				_ = patchedMethod;
				num++;
			}
			Log.LogInfo((object)$"{num} method(s) patched.");
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"Harmony patching failed; the rest of the menu still works: {arg}");
		}
		Log.LogInfo((object)string.Format("{0} {1} loaded. Press {2} in game.", "Complete Cheat Menu", Version, ToggleKey.Value));
	}

	private void OnDestroy()
	{
		if (VisualCheats.FreeCamActive)
		{
			VisualCheats.ToggleFreeCam(on: false);
		}
		Harmony harmony = _harmony;
		if (harmony != null)
		{
			harmony.UnpatchSelf();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(ToggleKey.Value))
		{
			MenuWindow.Toggle();
			if (MenuWindow.Visible && !_checkedOnce)
			{
				_checkedOnce = true;
				SafeRun(DiagnosticsTab.RunChecks, "diagnostics");
			}
		}
		if (AutoEnableCheats.Value && Time.unscaledTime >= _nextGateCheck)
		{
			_nextGateCheck = Time.unscaledTime + 5f;
			if (Refs.InGame && Refs.IsHost)
			{
				SafeRun(CheatGate.EnsureEnabled, "cheat gate");
			}
		}
		SafeRun(Keybinds.Poll, "keybinds");
		SafeRun(WeaponCheats.Tick, "weapons");
		SafeRun(TickDriver.Tick, "tick");
	}

	private void FixedUpdate()
	{
		SafeRun(TickDriver.FixedTick, "fixed tick");
	}

	private void OnGUI()
	{
		SafeRun(EspRenderer.Draw, "esp");
		SafeRun(MenuWindow.Draw, "menu");
	}

	private static void SafeRun(Action action, string what)
	{
		try
		{
			action();
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"{what} failed: {arg}");
		}
	}
}
