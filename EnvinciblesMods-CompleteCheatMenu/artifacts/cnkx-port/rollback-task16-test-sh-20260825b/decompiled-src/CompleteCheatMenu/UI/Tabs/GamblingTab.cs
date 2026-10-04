using System;
using System.Collections.Generic;
using System.Linq;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class GamblingTab
{
	internal const string Title = "Gambling";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		if (Widgets.RequireHost(CheatGate.BlockReason()))
		{
			if (!GamblingCheats.Available)
			{
				Widgets.Note("No casino in this scene. Travel to the island with the casino.");
				DrawSlotMachine();
				return;
			}
			DrawRig();
			DrawLive();
			DrawTable();
			DrawSlotMachine();
		}
	}

	private static void DrawRig()
	{
		Widgets.Section("Roulette odds", delegate
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Off", Mode(GamblingCheats.Mode.Off)))
			{
				GamblingCheats.RigMode = GamblingCheats.Mode.Off;
			}
			if (GUILayout.Button("Win chance", Mode(GamblingCheats.Mode.Odds)))
			{
				GamblingCheats.RigMode = GamblingCheats.Mode.Odds;
			}
			if (GUILayout.Button("Force colour", Mode(GamblingCheats.Mode.ForceColour)))
			{
				GamblingCheats.RigMode = GamblingCheats.Mode.ForceColour;
			}
			GUILayout.EndHorizontal();
			switch (GamblingCheats.RigMode)
			{
			case GamblingCheats.Mode.Off:
				Widgets.Note("The wheel runs untouched. Real odds: 18 red, 18 black, 1 green.");
				break;
			case GamblingCheats.Mode.Odds:
				GamblingCheats.WinChance = Widgets.SliderRow("Win chance", GamblingCheats.WinChance, 0f, 100f, "0'%'", 50f);
				Widgets.Note("Rolled once per spin. On a win the ball is steered to the colour you bet; on a loss it goes to the other of red or black, never green, so a loss can't accidentally pay 35x.");
				break;
			case GamblingCheats.Mode.ForceColour:
			{
				string[] colours = GamblingCheats.Colours;
				if (colours.Length == 0)
				{
					Widgets.Note("BetColor enum did not resolve. See Diagnostics.");
				}
				else
				{
					GUILayout.BeginHorizontal();
					string[] array = colours;
					foreach (string text in array)
					{
						if (GUILayout.Button(text, (GamblingCheats.ForcedColour == text) ? Theme.BtnAccent : Theme.Btn, GUILayout.Width(96f)))
						{
							GamblingCheats.ForcedColour = text;
						}
					}
					GUILayout.EndHorizontal();
					Widgets.Note("Every spin lands on this colour regardless of what you bet. Green pays 35x.");
				}
				break;
			}
			}
		});
		Widgets.Section("How this works", delegate
		{
			Widgets.Note("The ball is a real physics object and the game reads its actual angle against the wheel to decide the result. Rather than forging the payout — which is why other mods pay out on a colour the ball never touched — this rotates the wheel a fraction of a pocket while the ball settles, so the ball genuinely comes to rest on the colour you asked for. What you see is what pays, and other players see the same wheel.");
		});
	}

	private static GUIStyle Mode(GamblingCheats.Mode m)
	{
		if (GamblingCheats.RigMode != m)
		{
			return Theme.Btn;
		}
		return Theme.BtnAccent;
	}

	private static void DrawLive()
	{
		Widgets.Section("Live", delegate
		{
			Row("Spin in progress", GamblingCheats.IsBetting ? "yes" : "no");
			Row("You bet", GamblingCheats.BetColour);
			Row("Ball is on", GamblingCheats.CurrentBallColour);
			Row("Steering toward", GamblingCheats.CurrentTarget);
			Row("Pot worth", GamblingCheats.PotWorth.ToString("N0"));
			Row("Last result", GamblingCheats.LastOutcome);
			GUILayout.Space(4f);
			Row("Spins", $"{GamblingCheats.SpinsSeen}   won {GamblingCheats.SpinsWon}   " + $"lost {GamblingCheats.SpinsLost}");
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Settle now", GamblingCheats.SettleNow, "Wheel settled", Theme.Btn);
			Widgets.ActionButton("Reset stats", delegate
			{
				GamblingCheats.ResetStats();
				return true;
			}, "Stats cleared", Theme.Btn);
			GUILayout.EndHorizontal();
			Widgets.Note("Settle now ends the spin immediately on whatever pocket the ball is already over — use it once the colour reads correctly.");
		});
	}

	private static void Row(string label, string value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, Theme.Muted, GUILayout.Width(140f));
		GUILayout.Label(value, Theme.Body);
		GUILayout.EndHorizontal();
	}

	private static void DrawTable()
	{
		Widgets.Section("Bet table", delegate
		{
			List<object> list = GamblingCheats.BetItems();
			GUILayout.Label($"{list.Count} item(s) on the table", Theme.Body);
			CheatState.BetBoost = Widgets.SliderRow("Item value multiplier", CheatState.BetBoost, 1f, 50f, "0.##", 1f);
			Widgets.ActionButton("Apply to table", () => GamblingCheats.BoostBetItems(CheatState.BetBoost), $"Table items multiplied by {CheatState.BetBoost:0.##}", Theme.BtnAccent);
			Widgets.Note("Raises the worth of everything you've placed, so a win pays more. Apply before the wheel resolves.");
			string[] colours = GamblingCheats.Colours;
			if (colours.Length != 0 && !GamblingCheats.IsBetting)
			{
				GUILayout.Space(4f);
				GUILayout.Label("Start a spin manually", Theme.Muted);
				GUILayout.BeginHorizontal();
				string[] array = colours;
				foreach (string text in array)
				{
					string colour = text;
					Widgets.ActionButton(colour, () => GamblingCheats.StartSpin(colour), "Spinning on " + colour, Theme.Btn, 96f);
				}
				GUILayout.EndHorizontal();
			}
		});
	}

	private static void DrawSlotMachine()
	{
		if (!GamblingCheats.SlotsAvailable)
		{
			Widgets.Section("Skin machine", delegate
			{
				Widgets.Note("SlotMachineManager did not resolve. See Diagnostics.");
			});
			return;
		}
		List<object> targets = GamblingCheats.SkinTargets();
		object selected = SelectedTarget(targets);
		Widgets.Section("Skin machine", delegate
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(GamblingCheats.SlotRigArmed ? "Armed:" : "Nothing armed", Theme.Body, GUILayout.Width(96f));
			GUILayout.Label(GamblingCheats.SlotRigArmed ? GamblingCheats.LastArmed : "the roll is random", Theme.Muted);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Roll the machine", GamblingCheats.RollSlots, "Rolled", Theme.BtnAccent);
			Widgets.ActionButton("Clear", GamblingCheats.ClearArmed, "Rig cleared");
			GUILayout.EndHorizontal();
			Widgets.Note("Arm a skin, then roll — the reel is built around your pick, so the machine visibly stops on the skin it hands you.");
		});
		Widgets.Section("1 · What are you winning a skin for", delegate
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Boat", (CheatState.SlotTargetIndex < 0) ? Theme.BtnAccent : Theme.Btn, GUILayout.Width(96f)))
			{
				CheatState.SlotTargetIndex = -1;
			}
			object held = ItemCheats.HeldItem;
			if (held != null)
			{
				int num = targets.FindIndex((object t) => t == held);
				if (GUILayout.Button("Held item", (num >= 0 && CheatState.SlotTargetIndex == num) ? Theme.BtnAccent : Theme.Btn, GUILayout.Width(110f)))
				{
					if (num >= 0)
					{
						CheatState.SlotTargetIndex = num;
					}
					else
					{
						Notifications.Show(ItemCheats.ItemName(held) + " has no skins", ok: false);
					}
				}
			}
			GUILayout.EndHorizontal();
			CheatState.SlotTargetFilter = Widgets.SearchBox(CheatState.SlotTargetFilter, "Find a weapon…");
			string text = (CheatState.SlotTargetFilter ?? "").Trim().ToLowerInvariant();
			int num2 = 0;
			GUILayout.BeginHorizontal();
			for (int num3 = 0; num3 < targets.Count; num3++)
			{
				string text2 = GamblingCheats.TargetName(targets[num3]);
				if (text.Length <= 0 || text2.ToLowerInvariant().Contains(text))
				{
					int num4 = num3;
					if (GUILayout.Button(text2, (CheatState.SlotTargetIndex == num4) ? Theme.BtnAccent : Theme.Btn, GUILayout.Width(128f)))
					{
						CheatState.SlotTargetIndex = num4;
					}
					if (++num2 % 3 == 0)
					{
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Selected: " + GamblingCheats.TargetName(selected), Theme.Body);
		});
		List<SkinCheats.SkinEntry> skins = GamblingCheats.SkinsFor(selected);
		if (skins.Count == 0)
		{
			Widgets.Section("2 · Pick a skin", delegate
			{
				Widgets.Note(GamblingCheats.TargetName(selected) + " has no skin preset.");
			});
			return;
		}
		Widgets.Section("2 · Pick a rarity, or take any of it", delegate
		{
			List<string> list = GamblingCheats.RaritiesFor(selected);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("All", string.IsNullOrEmpty(CheatState.SlotRarity) ? Theme.BtnAccent : Theme.Btn, GUILayout.Width(84f)))
			{
				CheatState.SlotRarity = "";
			}
			foreach (string item in list)
			{
				if (GUILayout.Button(item, (CheatState.SlotRarity == item) ? Theme.BtnAccent : Theme.Btn, GUILayout.Width(104f)))
				{
					CheatState.SlotRarity = item;
				}
			}
			GUILayout.EndHorizontal();
			if (!string.IsNullOrEmpty(CheatState.SlotRarity))
			{
				string rarity = CheatState.SlotRarity;
				Widgets.ActionButton("Arm a random " + rarity + " skin", () => GamblingCheats.ArmRandomOfRarity(selected, rarity), "Armed a " + rarity + " skin", Theme.BtnAccent);
			}
			Widgets.Note("Rarity also decides where on the reel it lands — the game seats commons, rares and legendaries in different positions.");
		});
		Widgets.Section("3 · Or choose the exact skin", delegate
		{
			CheatState.SlotSkinFilter = Widgets.SearchBox(CheatState.SlotSkinFilter, "Filter skins…");
			string filter = (CheatState.SlotSkinFilter ?? "").Trim().ToLowerInvariant();
			List<SkinCheats.SkinEntry> list = (from skinEntry in skins
				where string.IsNullOrEmpty(CheatState.SlotRarity) || string.Equals(skinEntry.Rarity, CheatState.SlotRarity, StringComparison.OrdinalIgnoreCase)
				where filter.Length == 0 || skinEntry.Name.ToLowerInvariant().Contains(filter) || (skinEntry.Rarity ?? "").ToLowerInvariant().Contains(filter)
				select skinEntry).ToList();
			GUILayout.Label($"{list.Count} of {skins.Count} skins", Theme.Muted);
			_scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(220f));
			foreach (SkinCheats.SkinEntry item2 in list)
			{
				SkinCheats.SkinEntry s = item2;
				GUILayout.BeginHorizontal();
				GUILayout.Label(s.Name, Theme.Body);
				GUILayout.FlexibleSpace();
				if (!string.IsNullOrEmpty(s.Rarity))
				{
					GUILayout.Label(s.Rarity, Theme.Muted);
				}
				Widgets.ActionButton("Arm", () => GamblingCheats.ArmSkin(selected, s), "Next roll awards " + s.Name, Theme.BtnAccent, 54f);
				Widgets.ActionButton("Unlock", () => SkinCheats.UnlockSkin(selected, s.Index), "Unlocked " + s.Name, Theme.Btn, 66f);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		});
	}

	private static object SelectedTarget(List<object> targets)
	{
		if (CheatState.SlotTargetIndex < 0)
		{
			return null;
		}
		if (CheatState.SlotTargetIndex >= targets.Count)
		{
			return null;
		}
		return targets[CheatState.SlotTargetIndex];
	}
}
