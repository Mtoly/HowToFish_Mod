using System.Collections.Generic;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class SettingsTab
{
	internal const string Title = "Settings";

	internal static void Draw()
	{
		Widgets.Section("Presets", delegate
		{
			CheatState.PresetName = Widgets.FieldRow("Name", CheatState.PresetName, 60f);
			Widgets.ActionButton("Save current settings", () => Presets.Save(CheatState.PresetName), "Saved '" + CheatState.PresetName + "'", Theme.BtnAccent);
			List<string> list = Presets.List();
			if (list.Count == 0)
			{
				Widgets.Note("No presets yet. Presets store movement, ESP, camera and item values.");
				return;
			}
			GUILayout.Space(4f);
			foreach (string item in list)
			{
				string n = item;
				GUILayout.BeginHorizontal();
				GUILayout.Label(n, Theme.Body);
				GUILayout.FlexibleSpace();
				Widgets.ActionButton("Load", () => Presets.Load(n), "Loaded '" + n + "'", Theme.BtnAccent, 58f);
				if (GUILayout.Button("×", Theme.BtnDanger, GUILayout.Width(26f)))
				{
					Presets.Delete(n);
				}
				GUILayout.EndHorizontal();
			}
		});
		Widgets.Section("Plugin", delegate
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Menu key", Theme.Body, GUILayout.Width(150f));
			GUILayout.Label(Plugin.ToggleKey.Value.ToString(), Theme.Muted);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Cheat hotkeys", Theme.Body, GUILayout.Width(150f));
			GUILayout.Label(Keybinds.Enabled ? "enabled" : "disabled — see Keybinds tab", Theme.Muted);
			GUILayout.EndHorizontal();
			Plugin.AutoEnableCheats.Value = Widgets.ToggleRow("Auto-unlock dev cheats", Plugin.AutoEnableCheats.Value);
			Plugin.BackupSaves.Value = Widgets.ToggleRow("Back up saves before permanent changes", Plugin.BackupSaves.Value);
			Widgets.Note("Config: BepInEx/config/envincible.howtofish.completecheatmenu.cfg");
		});
	}
}
