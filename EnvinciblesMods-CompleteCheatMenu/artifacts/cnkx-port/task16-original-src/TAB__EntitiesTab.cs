using System.Collections.Generic;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class EntitiesTab
{
	internal const string Title = "Entities";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		if (!Widgets.RequireHost(CheatGate.BlockReason()))
		{
			return;
		}
		if (!EntityCheats.Available)
		{
			Widgets.Note("ItemManager.Items did not resolve. See Diagnostics.");
			return;
		}
		Widgets.Section("What's out there", delegate
		{
			GUILayout.Label($"{EntityCheats.TotalCount} loose object(s) in the world", Theme.H1);
			Widgets.Note("Anything a player is holding or has stored is excluded and never removed.");
			List<EntityCheats.Group> list = EntityCheats.Summary();
			if (list.Count == 0)
			{
				Widgets.Note("Nothing loose right now.");
			}
			else
			{
				_scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(150f));
				foreach (EntityCheats.Group item in list)
				{
					SpawnCatalog.Category kind = item.Kind;
					GUILayout.BeginHorizontal();
					GUILayout.Label(kind.ToString(), Theme.Body, GUILayout.Width(120f));
					GUILayout.Label(item.Count.ToString("N0"), Theme.Muted, GUILayout.Width(60f));
					GUILayout.FlexibleSpace();
					Widgets.ActionButton("Despawn", () => EntityCheats.DespawnCategory(kind) > 0, $"Cleared {kind}", Theme.Btn, 80f);
					GUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();
			}
		});
		Widgets.Section("Clear near me", delegate
		{
			CheatState.DespawnRadius = Widgets.SliderRow("Radius", CheatState.DespawnRadius, 5f, 200f, "0", 30f);
			Widgets.ActionButton($"Despawn everything within {CheatState.DespawnRadius:0}m", () => EntityCheats.DespawnNearby(CheatState.DespawnRadius) > 0, "Area cleared", Theme.BtnAccent);
			Widgets.Note("The safest option after a spawn button gets away from you.");
		});
		Widgets.Section("Clear everything", delegate
		{
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("All creatures", () => EntityCheats.DespawnCreatures() > 0, "Creatures cleared", Theme.BtnDanger);
			Widgets.ActionButton("Everything loose", () => EntityCheats.DespawnAll() > 0, "World cleared", Theme.BtnDanger);
			GUILayout.EndHorizontal();
			Widgets.Note("Uses the game's own Item.DestroyItem, so objects despawn across the network properly rather than vanishing only on your screen. This will also remove loot and quest items lying on the ground.");
		});
	}
}
