using System.Collections.Generic;
using System.Linq;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class SkinsTab
{
	internal const string Title = "Skins";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		if (!Widgets.RequireHost(CheatGate.BlockReason()))
		{
			return;
		}
		Widgets.Section("Everything", delegate
		{
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Unlock all weapon skins", SkinCheats.UnlockAll, "All weapon skins unlocked", Theme.BtnAccent);
			Widgets.ActionButton("Lock all", SkinCheats.LockAll, "All skins locked", Theme.BtnDanger);
			GUILayout.EndHorizontal();
			Widgets.ActionButton("Unlock all characters", SkinCheats.UnlockAllCharacters, "All characters unlocked", Theme.BtnAccent);
		});
		Widgets.Section("Held item skin", delegate
		{
			object held = ItemCheats.HeldItem;
			if (held == null)
			{
				Widgets.Note("Hold an item to see and apply its skins.");
			}
			else
			{
				List<SkinCheats.SkinEntry> list = SkinCheats.SkinsFor(held);
				GUILayout.Label($"{ItemCheats.ItemName(held)} — {list.Count} skins", Theme.Body);
				if (list.Count == 0)
				{
					Widgets.Note("This item has no skin preset.");
				}
				else
				{
					_scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(190f));
					foreach (SkinCheats.SkinEntry item in list)
					{
						SkinCheats.SkinEntry s = item;
						GUILayout.BeginHorizontal();
						GUILayout.Label(s.Name, Theme.Body);
						GUILayout.FlexibleSpace();
						if (!string.IsNullOrEmpty(s.Rarity))
						{
							GUILayout.Label(s.Rarity, Theme.Muted);
						}
						Widgets.ActionButton("Unlock", () => SkinCheats.UnlockSkin(held, s.Index), "Unlocked " + s.Name, Theme.Btn, 62f);
						Widgets.ActionButton("Wear", () => ItemCheats.SetSkin(held, s.Index), "Applied " + s.Name, Theme.BtnAccent, 54f);
						GUILayout.EndHorizontal();
					}
					GUILayout.EndScrollView();
				}
			}
		});
		Widgets.Section("Boat skin", delegate
		{
			List<SkinCheats.SkinEntry> list = SkinCheats.BoatSkins();
			if (list.Count == 0)
			{
				Widgets.Note("No boat in the scene, or it has no skin preset.");
			}
			else
			{
				GUILayout.Label("Current: " + list.FirstOrDefault((SkinCheats.SkinEntry skinEntry) => skinEntry.Index == SkinCheats.BoatSkin).Name, Theme.Body);
				int num = 0;
				GUILayout.BeginHorizontal();
				foreach (SkinCheats.SkinEntry item2 in list)
				{
					SkinCheats.SkinEntry s = item2;
					Widgets.ActionButton(s.Name, () => SkinCheats.SetBoatSkin(s.Index), "Boat skin: " + s.Name, (s.Index == SkinCheats.BoatSkin) ? Theme.BtnAccent : Theme.Btn, 110f);
					if (++num % 3 == 0)
					{
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
					}
				}
				GUILayout.EndHorizontal();
			}
		});
		Widgets.Section("Characters", delegate
		{
			CheatState.SkinFilter = Widgets.SearchBox(CheatState.SkinFilter, "Filter characters…");
			string text = (CheatState.SkinFilter ?? "").Trim().ToLowerInvariant();
			int num = 0;
			GUILayout.BeginHorizontal();
			string[] characterUnlocks = SkinCheats.CharacterUnlocks;
			foreach (string text2 in characterUnlocks)
			{
				string text3 = SkinCheats.PrettyCharacter(text2);
				if (text.Length <= 0 || text3.ToLowerInvariant().Contains(text))
				{
					string m = text2;
					Widgets.ActionButton(text3, () => SkinCheats.UnlockCharacter(m), "Unlocked " + text3, Theme.Btn, 140f);
					if (++num % 3 == 0)
					{
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
					}
				}
			}
			GUILayout.EndHorizontal();
		});
	}
}
