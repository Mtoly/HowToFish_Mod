using System.Collections.Generic;
using CompleteCheatMenu.Cheats;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class FishingTab
{
	internal const string Title = "Fishing";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason()))
		{
			return;
		}
		if (!FishingCheats.Available)
		{
			Widgets.Note("No player inventory yet. Load into a save first.");
			return;
		}
		Widgets.Section("Bait", delegate
		{
			GUILayout.Label($"Currently equipped: bait {FishingCheats.CurrentBait}", Theme.Body);
			Widgets.ActionButton("Grant every bait", FishingCheats.GrantAllBaits, "All baits granted", Theme.BtnAccent);
			Widgets.Note("Grants ownership without charging — the same call the game makes once a purchase is approved.");
		});
		List<FishingCheats.BaitEntry> baits = FishingCheats.AllBaits();
		if (baits.Count == 0)
		{
			Widgets.Note("GameInfo.AllBaits did not resolve. See Diagnostics.");
			return;
		}
		Widgets.Section($"All baits ({baits.Count})", delegate
		{
			_scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(260f));
			foreach (FishingCheats.BaitEntry item in baits)
			{
				FishingCheats.BaitEntry b = item;
				GUILayout.BeginHorizontal();
				GUILayout.Label(b.Name, Theme.Body);
				GUILayout.FlexibleSpace();
				if (b.Cost > 0)
				{
					GUILayout.Label($"{b.Cost:N0}", Theme.Muted);
				}
				Widgets.ActionButton("Grant", () => FishingCheats.GrantBait(b.Index), "Granted " + b.Name, Theme.Btn, 58f);
				Widgets.ActionButton("Equip", () => FishingCheats.SelectBait(b.Index), "Equipped " + b.Name, (b.Index == FishingCheats.CurrentBait) ? Theme.BtnAccent : Theme.Btn, 58f);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		});
	}
}
