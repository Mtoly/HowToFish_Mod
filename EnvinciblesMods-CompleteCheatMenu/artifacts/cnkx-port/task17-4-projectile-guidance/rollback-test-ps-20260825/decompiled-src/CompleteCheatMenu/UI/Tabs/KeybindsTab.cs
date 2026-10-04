using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class KeybindsTab
{
	internal const string Title = "Keybinds";

	internal static void Draw()
	{
		Widgets.Section("Master switch", delegate
		{
			bool enabled = Keybinds.Enabled;
			bool flag = Widgets.ToggleRow("Enable all keybinds", enabled, enabled ? "ACTIVE" : "off");
			if (flag != enabled)
			{
				Plugin.KeybindsEnabled.Value = flag;
				Notifications.Show(flag ? "Cheat hotkeys enabled" : "Cheat hotkeys disabled");
			}
			GUILayout.Space(2f);
			GUILayout.Label(enabled ? "Hotkeys below are live." : "Hotkeys below do nothing while this is off.", enabled ? Theme.Body : Theme.Muted);
			Widgets.Note($"The menu key ({Plugin.ToggleKey.Value}) is not affected by this switch " + "and always works.");
		});
		Widgets.Section("Hotkeys", delegate
		{
			if (!Keybinds.Enabled)
			{
				Widgets.Note("Currently inactive — you can still rebind them.");
			}
			Widgets.Note("Click a key to rebind it, then press the new key. Escape cancels.");
			GUILayout.Space(2f);
			foreach (Keybinds.Bind item in Keybinds.All)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(item.Name, Keybinds.Enabled ? Theme.Body : Theme.Muted, GUILayout.Width(150f));
				bool flag = Keybinds.Capturing == item;
				if (GUILayout.Button(flag ? "press a key…" : item.Key.Value.ToString(), flag ? Theme.BtnAccent : Theme.Btn, GUILayout.Width(130f)))
				{
					Keybinds.Capturing = (flag ? null : item);
				}
				GUILayout.Label(item.IsOn() ? "on" : "off", Theme.Muted, GUILayout.Width(30f));
				if (GUILayout.Button("clear", Theme.Btn, GUILayout.Width(56f)))
				{
					item.Key.Value = KeyCode.None;
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.Space(4f);
			Widgets.Note("A cleared bind is stored as None and stays inactive until you set a key.");
		});
		Widgets.Section("Menu", delegate
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Open / close menu", Theme.Body, GUILayout.Width(150f));
			GUILayout.Label(Plugin.ToggleKey.Value.ToString(), Theme.Body, GUILayout.Width(130f));
			GUILayout.Label("always on", Theme.Muted);
			GUILayout.EndHorizontal();
			Widgets.Note("Change this one in BepInEx/config/envincible.howtofish.completecheatmenu.cfg, so a mis-set key can never lock you out of the menu.");
		});
	}
}
