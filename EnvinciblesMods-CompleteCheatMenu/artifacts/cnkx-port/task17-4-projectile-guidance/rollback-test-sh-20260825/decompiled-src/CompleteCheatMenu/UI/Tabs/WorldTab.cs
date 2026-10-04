using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class WorldTab
{
	internal const string Title = "World";

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason()))
		{
			return;
		}
		Widgets.Section("Journal", delegate
		{
			int totalCreatures = WorldCheats.TotalCreatures;
			GUILayout.Label($"Normal  {WorldCheats.KilledCount(drip: false)} / {totalCreatures}", Theme.Body);
			GUILayout.Label($"Drip    {WorldCheats.KilledCount(drip: true)} / {totalCreatures}", Theme.Body);
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Complete normal", () => WorldCheats.SetAllCreaturesKilled(to: true, drip: false), "Normal journal completed", Theme.BtnAccent);
			Widgets.ActionButton("Complete drip", () => WorldCheats.SetAllCreaturesKilled(to: true, drip: true), "Drip journal completed", Theme.BtnAccent);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Reset normal", () => WorldCheats.SetAllCreaturesKilled(to: false, drip: false), "Normal journal reset", Theme.BtnDanger);
			Widgets.ActionButton("Reset drip", () => WorldCheats.SetAllCreaturesKilled(to: false, drip: true), "Drip journal reset", Theme.BtnDanger);
			GUILayout.EndHorizontal();
		});
		Widgets.Section("Boss", delegate
		{
			if (!WorldCheats.BossActive)
			{
				Widgets.Note("No boss active. Spawn one from the Spawn tab.");
			}
			else
			{
				GUILayout.Label($"HP {WorldCheats.BossHp:N0} / {WorldCheats.BossMaxHp:N0}", Theme.Body);
				bool immortal = WorldCheats.Immortal;
				bool flag = Widgets.ToggleRow("Boss immortal", immortal, immortal ? "on" : "off");
				if (flag != immortal)
				{
					Notifications.Result(WorldCheats.SetBossImmortal(flag), flag ? "Boss is immortal" : "Boss is mortal", "Immortality toggle failed");
				}
				CheatState.BossHeal = Widgets.IntSliderRow("Heal amount", CheatState.BossHeal, 1, 10000);
				GUILayout.BeginHorizontal();
				Widgets.ActionButton("Heal boss", () => WorldCheats.HealBoss(CheatState.BossHeal), $"Boss healed {CheatState.BossHeal:N0}");
				Widgets.ActionButton("Kill boss", WorldCheats.KillBoss, "Boss killed", Theme.BtnDanger);
				GUILayout.EndHorizontal();
			}
		});
		Widgets.Section("World unlocks", delegate
		{
			Widgets.ActionButton("Unlock grill", WorldCheats.UnlockGrill, "Grill unlocked", Theme.BtnAccent);
		});
	}
}
