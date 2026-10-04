using CompleteCheatMenu.Cheats;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class ProgressionTab
{
	internal const string Title = "Progression";

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason()))
		{
			return;
		}
		Widgets.Section("Everything at once", delegate
		{
			Widgets.ActionButton("Unlock everything", ProgressionCheats.UnlockEverything, "Skins, characters, islands, boat, radar, grill and pockets unlocked", Theme.BtnAccent);
			Widgets.Note("Skins, characters, all islands, boat, radar, grill and every inventory pocket.");
		});
		Widgets.Section("Inventory", delegate
		{
			GUILayout.Label($"Extra pockets unlocked: {ProgressionCheats.ExtraSlots}", Theme.Body);
			GUILayout.BeginHorizontal();
			for (byte b = 1; b <= 4; b++)
			{
				byte slot = b;
				Widgets.ActionButton($"Pocket {slot}", () => ProgressionCheats.UnlockPocket(slot), $"Pocket {slot} unlocked", Theme.Btn, 88f);
			}
			GUILayout.EndHorizontal();
			Widgets.ActionButton("Unlock all pockets", ProgressionCheats.UnlockAllPockets, "All pockets unlocked", Theme.BtnAccent);
		});
		Widgets.Section("Game completion", delegate
		{
			Widgets.ActionButton("Finish game", ProgressionCheats.FinishGame, "Game marked finished", Theme.BtnDanger);
			Widgets.ActionButton("Show kill scores", ProgressionCheats.ShowKillScores, "Kill scores shown");
		});
		Widgets.Section("Steam achievements", delegate
		{
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Unlock all", ProgressionCheats.UnlockAchievements, "All achievements unlocked", Theme.BtnDanger);
			Widgets.ActionButton("Lock all", ProgressionCheats.LockAchievements, "Achievements locked", Theme.Btn);
			GUILayout.EndHorizontal();
			Widgets.Note("Achievements are written to Steam. Locking clears the local flags, but anything already pushed to your Steam profile stays there.");
		});
		Widgets.Section("Save", delegate
		{
			Widgets.ActionButton("Save now", ProgressionCheats.SaveNow, "Server saved", Theme.BtnAccent);
			Widgets.Note("A backup copy of your save folder is taken automatically before the first permanent change each session.");
		});
	}
}
