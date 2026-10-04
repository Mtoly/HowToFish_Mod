using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class PlayerTab
{
	internal const string Title = "Player";

	internal static void Draw()
	{
		if (!Widgets.RequireHost(CheatGate.BlockReason()))
		{
			return;
		}
		Widgets.Section("Invulnerability", delegate
		{
			bool godMode = CombatCheats.GodMode;
			bool flag = Widgets.ToggleRow("God mode", godMode, godMode ? "on" : "off");
			if (flag != godMode)
			{
				Notifications.Result(CombatCheats.SetGodMode(flag), flag ? "God mode on" : "God mode off", "God mode toggle failed");
			}
			bool oneShot = CombatCheats.OneShot;
			bool flag2 = Widgets.ToggleRow("One-shot kills", oneShot, oneShot ? "on" : "off");
			if (flag2 != oneShot)
			{
				Notifications.Result(CombatCheats.SetOneShot(flag2), flag2 ? "One-shot on" : "One-shot off", "One-shot toggle failed");
			}
			bool friendlyFire = CombatCheats.FriendlyFire;
			bool flag3 = Widgets.ToggleRow("Friendly fire", friendlyFire, friendlyFire ? "on" : "off");
			if (flag3 != friendlyFire)
			{
				Notifications.Result(CombatCheats.SetFriendlyFire(flag3), flag3 ? "Friendly fire on" : "Friendly fire off", "Friendly fire toggle failed");
			}
		});
		Widgets.Section("Vitals", delegate
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label($"Health {CombatCheats.Health}", Theme.Body, GUILayout.Width(120f));
			GUILayout.Label($"Fullness {CombatCheats.Fullness}", Theme.Body);
			GUILayout.EndHorizontal();
			CheatState.HealAmount = Widgets.IntSliderRow("Amount", CheatState.HealAmount, 1, 100);
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Heal", () => CombatCheats.Heal(CheatState.HealAmount), $"Healed {CheatState.HealAmount}");
			Widgets.ActionButton("Feed", () => CombatCheats.RestoreFullness(CheatState.HealAmount), $"Fullness +{CheatState.HealAmount}");
			Widgets.ActionButton("Full reset", CombatCheats.ResetVitals, "Vitals reset", Theme.BtnAccent);
			GUILayout.EndHorizontal();
			Widgets.Note("Full reset also clears poison and fire.");
		});
		Widgets.Section("Developer cheats", delegate
		{
			bool on = CheatGate.CheatsEnabled;
			GUILayout.Label(on ? "The game's own command suite is unlocked." : "Locked — the game's dev commands will refuse to run.", Theme.Muted);
			GUILayout.BeginHorizontal();
			Widgets.ActionButton(on ? "Disable dev cheats" : "Enable dev cheats", () => CheatGate.EnableCheats(!on), on ? "Dev cheats disabled" : "Dev cheats enabled", on ? Theme.Btn : Theme.BtnAccent);
			GUILayout.EndHorizontal();
		});
	}
}
