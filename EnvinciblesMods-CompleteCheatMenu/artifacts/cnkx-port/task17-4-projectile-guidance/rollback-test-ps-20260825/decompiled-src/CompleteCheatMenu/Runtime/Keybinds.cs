using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using CompleteCheatMenu.Cheats;
using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal static class Keybinds
{
	internal class Bind
	{
		internal string Name;

		internal ConfigEntry<KeyCode> Key;

		internal Action Invoke;

		internal Func<bool> IsOn;
	}

	internal static readonly List<Bind> All = new List<Bind>();

	internal static Bind Capturing;

	internal static bool Enabled
	{
		get
		{
			if (Plugin.KeybindsEnabled != null)
			{
				return Plugin.KeybindsEnabled.Value;
			}
			return false;
		}
	}

	internal static void Register(ConfigFile config)
	{
		All.Clear();
		Add(config, "God mode", KeyCode.F1, delegate
		{
			CombatCheats.SetGodMode(!CombatCheats.GodMode);
		}, () => CombatCheats.GodMode);
		Add(config, "Fly", KeyCode.F2, delegate
		{
			CheatState.Fly = !CheatState.Fly;
		}, () => CheatState.Fly);
		Add(config, "ESP", KeyCode.F3, delegate
		{
			CheatState.EspEnabled = !CheatState.EspEnabled;
		}, () => CheatState.EspEnabled);
		Add(config, "Free camera", KeyCode.F4, delegate
		{
			VisualCheats.ToggleFreeCam(!VisualCheats.FreeCamActive);
		}, () => VisualCheats.FreeCamActive);
		Add(config, "One-shot kills", KeyCode.F5, delegate
		{
			CombatCheats.SetOneShot(!CombatCheats.OneShot);
		}, () => CombatCheats.OneShot);
	}

	private static void Add(ConfigFile config, string name, KeyCode fallback, Action action, Func<bool> isOn)
	{
		All.Add(new Bind
		{
			Name = name,
			Key = config.Bind<KeyCode>("Keybinds", name, fallback, "Hotkey for " + name.ToLowerInvariant() + "."),
			Invoke = action,
			IsOn = isOn
		});
	}

	internal static void Poll()
	{
		if (Capturing != null || !Enabled)
		{
			return;
		}
		foreach (Bind item in All)
		{
			if (item.Key != null && item.Key.Value != KeyCode.None && Input.GetKeyDown(item.Key.Value))
			{
				try
				{
					item.Invoke();
					Notifications.Show(item.Name + ": " + (item.IsOn() ? "on" : "off"));
				}
				catch (Exception arg)
				{
					Plugin.Log.LogError((object)$"Keybind '{item.Name}' failed: {arg}");
				}
			}
		}
	}

	internal static void HandleCapture(Event e)
	{
		if (Capturing != null && e != null && e.type == EventType.KeyDown)
		{
			if (e.keyCode == KeyCode.Escape)
			{
				Capturing = null;
			}
			else if (e.keyCode != KeyCode.None)
			{
				Capturing.Key.Value = e.keyCode;
				Notifications.Show($"{Capturing.Name} bound to {e.keyCode}");
				Capturing = null;
				e.Use();
			}
		}
	}
}
