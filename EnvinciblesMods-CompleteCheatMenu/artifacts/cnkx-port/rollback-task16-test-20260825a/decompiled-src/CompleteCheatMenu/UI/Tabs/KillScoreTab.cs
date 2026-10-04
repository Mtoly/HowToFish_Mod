using System.Collections.Generic;
using System.Linq;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class KillScoreTab
{
	internal const string Title = "Kill score";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		if (!Widgets.RequireHost(CheatGate.BlockReason()))
		{
			return;
		}
		if (!KillScoreCheats.Available)
		{
			Widgets.Note("PlayerKillScore.AddKillScore did not resolve. See Diagnostics.");
			return;
		}
		Widgets.Section("Override", delegate
		{
			CheatState.KillScoreOverride = Widgets.ToggleRow("Use my bonuses", CheatState.KillScoreOverride, CheatState.KillScoreOverride ? "ACTIVE" : "off");
			GUILayout.Label($"Multiplier: ×{KillScoreCheats.PreviewMultiplier:0.##}", Theme.H1);
			CheatState.KillFlatMultiplier = Widgets.SliderRow("Extra multiplier", CheatState.KillFlatMultiplier, 1f, 50f, "0.##", 1f);
			CheatState.KillBonusLabel = Widgets.FieldRow("Its label", CheatState.KillBonusLabel, 90f);
			Widgets.Note("The extra multiplier is added as its own named row, so it shows up on the banner like any other bonus rather than appearing from nowhere.");
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Select all", delegate
			{
				for (int i = 0; i < KillScoreCheats.Catalogue.Count; i++)
				{
					if (!KillScoreCheats.Catalogue[i].Selected)
					{
						KillScoreCheats.Toggle(i);
					}
				}
				return true;
			}, "All bonuses selected", Theme.Btn);
			Widgets.ActionButton("Clear", delegate
			{
				KillScoreCheats.ClearSelection();
				return true;
			}, "Selection cleared", Theme.Btn);
			GUILayout.EndHorizontal();
		});
		Widgets.Section($"Bonuses ({KillScoreCheats.Selected.Count()} of {KillScoreCheats.Catalogue.Count})", delegate
		{
			_scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(300f));
			List<KillScoreCheats.BonusOption> catalogue = KillScoreCheats.Catalogue;
			for (int i = 0; i < catalogue.Count; i++)
			{
				KillScoreCheats.BonusOption bonusOption = catalogue[i];
				int index = i;
				GUILayout.BeginHorizontal();
				if (GUILayout.Toggle(bonusOption.Selected, "  " + bonusOption.Name, Theme.Toggle) != bonusOption.Selected)
				{
					KillScoreCheats.Toggle(index);
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label($"×{bonusOption.Worth:0.##}", Theme.Muted, GUILayout.Width(52f));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
			Widgets.Note("Bonuses multiply together — the game computes the payout as the product of every row, which is why picking several stacks up fast.");
		});
	}
}
