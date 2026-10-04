using System;
using System.Collections.Generic;
using System.Linq;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class SpawnTab
{
	internal const string Title = "Spawn";

	private const int MaxRows = 200;

	private static Vector2 _scroll;

	private static readonly SpawnCatalog.Category[] Categories = (SpawnCatalog.Category[])Enum.GetValues(typeof(SpawnCatalog.Category));

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason()))
		{
			return;
		}
		if (!SpawnCatalog.Loaded)
		{
			SpawnCatalog.Load();
			if (!SpawnCatalog.Loaded)
			{
				Widgets.Note("GameInfo._nameToSpawnable did not resolve. See Diagnostics.");
				return;
			}
		}
		Widgets.Section("Options", delegate
		{
			CheatState.SpawnCount = Widgets.IntSliderRow("Quantity", CheatState.SpawnCount, 1, 25);
			GUILayout.BeginHorizontal();
			CheatState.SpawnDead = GUILayout.Toggle(CheatState.SpawnDead, "  Dead", Theme.Toggle);
			CheatState.SpawnDrip = GUILayout.Toggle(CheatState.SpawnDrip, "  Drip", Theme.Toggle);
			GUILayout.EndHorizontal();
			Widgets.Note("Spawns 2m in front of the camera. Dead and Drip only apply to creatures.");
		});
		Widgets.Section("Sort", delegate
		{
			GUILayout.BeginHorizontal();
			SortButton("A–Z", SpawnCatalog.SortMode.Name);
			SortButton("Category", SpawnCatalog.SortMode.Category);
			SortButton("Item ID", SpawnCatalog.SortMode.Id);
			GUILayout.EndHorizontal();
			Dictionary<SpawnCatalog.Category, int> dictionary = SpawnCatalog.Counts();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button($"All ({SpawnCatalog.Count})", (!CheatState.SpawnCategory.HasValue) ? Theme.BtnAccent : Theme.Btn))
			{
				CheatState.SpawnCategory = null;
			}
			int num = 1;
			SpawnCatalog.Category[] categories = Categories;
			foreach (SpawnCatalog.Category category in categories)
			{
				if (dictionary.TryGetValue(category, out var value) && value != 0)
				{
					SpawnCatalog.Category category2 = category;
					if (GUILayout.Button($"{category2} ({value})", (CheatState.SpawnCategory == category2) ? Theme.BtnAccent : Theme.Btn))
					{
						CheatState.SpawnCategory = category2;
					}
					if (++num % 4 == 0)
					{
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
					}
				}
			}
			GUILayout.EndHorizontal();
			Widgets.Note("Categories come from the components each prefab carries, not from its name.");
		});
		CheatState.SpawnFilter = Widgets.SearchBox(CheatState.SpawnFilter, "Filter spawnables…");
		List<SpawnCatalog.Entry> list = SpawnCatalog.Search(CheatState.SpawnFilter, CheatState.SpawnCategory, CheatState.SpawnSort).ToList();
		GUILayout.Label($"{list.Count} of {SpawnCatalog.Count} spawnables", Theme.Muted);
		_scroll = GUILayout.BeginScrollView(_scroll);
		foreach (SpawnCatalog.Entry item in list.Take(200))
		{
			GUILayout.BeginHorizontal(Theme.Section);
			GUILayout.Label(item.Display, Theme.Body);
			GUILayout.FlexibleSpace();
			SpawnCatalog.Category kind = item.Kind;
			GUILayout.Label(kind.ToString(), Theme.Muted);
			string key = item.Key;
			string display = item.Display;
			Widgets.ActionButton("Spawn", () => SpawnCheats.Spawn(key, CheatState.SpawnCount, CheatState.SpawnDead, CheatState.SpawnDrip), $"Spawned {CheatState.SpawnCount}× {display}", Theme.BtnAccent, 66f);
			GUILayout.EndHorizontal();
		}
		if (list.Count > 200)
		{
			Widgets.Note($"Showing the first {200}. Narrow the filter to see the rest.");
		}
		GUILayout.EndScrollView();
	}

	private static void SortButton(string label, SpawnCatalog.SortMode mode)
	{
		if (GUILayout.Button(label, (CheatState.SpawnSort == mode) ? Theme.BtnAccent : Theme.Btn))
		{
			CheatState.SpawnSort = mode;
		}
	}
}
