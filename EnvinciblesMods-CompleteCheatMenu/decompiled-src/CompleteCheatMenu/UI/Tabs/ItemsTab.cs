using System;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class ItemsTab
{
	internal const string Title = "Items";

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason()))
		{
			return;
		}
		object held = ItemCheats.HeldItem;
		Widgets.Section("Target", delegate
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Held item", (CheatState.ItemTarget == ItemCheats.Target.Held) ? Theme.BtnAccent : Theme.Btn))
			{
				CheatState.ItemTarget = ItemCheats.Target.Held;
			}
			if (GUILayout.Button("Every item in world", (CheatState.ItemTarget == ItemCheats.Target.World) ? Theme.BtnAccent : Theme.Btn))
			{
				CheatState.ItemTarget = ItemCheats.Target.World;
			}
			GUILayout.EndHorizontal();
			int count = ItemCheats.Resolve(CheatState.ItemTarget).Count;
			GUILayout.Label((CheatState.ItemTarget != ItemCheats.Target.Held) ? $"{count} items in the world." : ((held == null) ? "Nothing in hand." : ("Holding: " + ItemCheats.ItemName(held))), Theme.Muted);
		});
		if (held != null && CheatState.ItemTarget == ItemCheats.Target.Held)
		{
			Widgets.Section("Current values", delegate
			{
				Stat("Base worth", ItemCheats.Worth(held).ToString("N0"));
				Stat("Total worth", ItemCheats.TotalWorth(held).ToString("N0"));
				Stat("Weight", ItemCheats.Weight(held).ToString("0.###"));
				Stat("Cookness", ItemCheats.Cookness(held).ToString("0.###"));
				Stat("Kill score multi", ItemCheats.KillScoreMulti(held).ToString("0.###"));
				Stat("Betting multi", ItemCheats.BettingMulti(held).ToString("0.###"));
			});
		}
		Widgets.Section("Worth", delegate
		{
			CheatState.ItemWorth = Widgets.FieldRow("Base worth", CheatState.ItemWorth, 100f);
			int worth = MoneyTab.Parse(CheatState.ItemWorth);
			Widgets.ActionButton("Apply worth", () => Apply((object i) => ItemCheats.SetWorth(i, worth)), $"Worth set to {worth:N0}", Theme.BtnAccent);
			Widgets.Note("Total worth is derived from base worth, weight and cookness.");
		});
		Widgets.Section("Multipliers", delegate
		{
			CheatState.ItemWeight = Widgets.SliderRow("Weight", CheatState.ItemWeight, 0.1f, 50f, "0.##", 1f);
			Widgets.ActionButton("Apply weight", () => Apply((object i) => ItemCheats.SetWeight(i, CheatState.ItemWeight)), $"Weight set to {CheatState.ItemWeight:0.##}");
			CheatState.ItemCookness = Widgets.SliderRow("Cookness", CheatState.ItemCookness, 0f, 2f, "0.##", 1f);
			Widgets.ActionButton("Apply cookness", () => Apply((object i) => ItemCheats.SetCookness(i, CheatState.ItemCookness)), $"Cookness set to {CheatState.ItemCookness:0.##}");
			CheatState.ItemKillScore = Widgets.SliderRow("Kill score", CheatState.ItemKillScore, 0f, 25f, "0.##", 1f);
			Widgets.ActionButton("Apply kill score", () => Apply((object i) => ItemCheats.SetKillScoreMulti(i, CheatState.ItemKillScore)), $"Kill score multi {CheatState.ItemKillScore:0.##}");
			CheatState.ItemBetting = Widgets.SliderRow("Betting", CheatState.ItemBetting, 0f, 25f, "0.##", 1f);
			Widgets.ActionButton("Apply betting", () => Apply((object i) => ItemCheats.SetBettingMulti(i, CheatState.ItemBetting)), $"Betting multi {CheatState.ItemBetting:0.##}");
		});
		Widgets.Section("Stacking and inventory", delegate
		{
			CheatState.KeepInventory = Widgets.ToggleRow("Keep inventory on death", CheatState.KeepInventory, CheatState.KeepInventory ? "ACTIVE" : "off");
			Widgets.Note("Skips the drop that normally scatters your items when you die, on both the inventory and the server side so nothing desyncs.");
			GUILayout.Space(4f);
			CheatState.DuplicateCount = Widgets.IntSliderRow("Copies", CheatState.DuplicateCount, 1, 50);
			Widgets.ActionButton($"Duplicate held item ×{CheatState.DuplicateCount}", () => InventoryCheats.DuplicateHeld(CheatState.DuplicateCount) > 0, $"Made {CheatState.DuplicateCount} copies", Theme.BtnAccent);
			Widgets.Note("The game has no stack count — every slot holds one real item with its own worth and skin — so this spawns that many genuine copies at your feet instead of faking a stack number.");
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Save inventory", InventoryCheats.SaveInventory, "Inventory saved");
			Widgets.ActionButton("Drop everything", InventoryCheats.DropAll, "Dropped", Theme.BtnDanger);
			GUILayout.EndHorizontal();
		});
		Widgets.Section("Bulk", delegate
		{
			Widgets.ActionButton("Make everything valuable", () => Apply((object i) => ItemCheats.SetWorth(i, 100000) && ItemCheats.SetWeight(i, 10f)), "Applied to the current target", Theme.BtnDanger);
			Widgets.Note("Applies base worth 100,000 and weight 10 to whichever target is selected above.");
		});
	}

	private static bool Apply(Func<object, bool> action)
	{
		return ItemCheats.ApplyToAll(CheatState.ItemTarget, action) > 0;
	}

	private static void Stat(string label, string value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, Theme.Muted, GUILayout.Width(130f));
		GUILayout.Label(value, Theme.Body);
		GUILayout.EndHorizontal();
	}
}
