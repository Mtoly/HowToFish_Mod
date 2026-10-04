using System.Collections.Generic;
using System.Linq;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class TeleportTab
{
	internal const string Title = "Teleport";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason()))
		{
			return;
		}
		Widgets.Section("Islands", delegate
		{
			GUILayout.Label($"Current island {TeleportCheats.CurrentIsland}   " + $"unlocked up to {TeleportCheats.MaxUnlocked}", Theme.Body);
			GUILayout.BeginHorizontal();
			int num = Mathf.Max(TeleportCheats.TotalIslands, 6);
			for (int i = 0; i < num; i++)
			{
				byte idx = (byte)i;
				Widgets.ActionButton($"{i}", () => TeleportCheats.GoToIsland(idx), $"Travelled to island {idx}", (idx == TeleportCheats.CurrentIsland) ? Theme.BtnAccent : Theme.Btn, 38f);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("◀ Previous", () => TeleportCheats.NextIsland(previous: true), "Previous island");
			Widgets.ActionButton("Next ▶", () => TeleportCheats.NextIsland(), "Next island");
			Widgets.ActionButton("Unlock all", TeleportCheats.UnlockAllIslands, "All islands unlocked", Theme.BtnAccent);
			GUILayout.EndHorizontal();
			Widgets.Note("Island travel moves everyone on the server, same as the game's own command.");
		});
		Widgets.Section("Places", delegate
		{
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Go to boat", TeleportCheats.GoToBoat, "Teleported to boat");
			GUILayout.EndHorizontal();
		});
		Widgets.Section("Waypoints", delegate
		{
			CheatState.WaypointName = Widgets.FieldRow("Name", CheatState.WaypointName, 60f);
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Save here", () => TeleportCheats.SaveWaypoint(CheatState.WaypointName), "Saved '" + CheatState.WaypointName + "'", Theme.BtnAccent);
			GUILayout.EndHorizontal();
			if (TeleportCheats.Waypoints.Count == 0)
			{
				Widgets.Note("No waypoints yet. They last until you quit the game.");
				return;
			}
			foreach (string item in TeleportCheats.Waypoints.Keys.ToList())
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(item, Theme.Body);
				GUILayout.FlexibleSpace();
				string n = item;
				Widgets.ActionButton("Go", () => TeleportCheats.GoToWaypoint(n), "Teleported to '" + n + "'", Theme.Btn, 48f);
				if (GUILayout.Button("×", Theme.Btn, GUILayout.Width(26f)))
				{
					TeleportCheats.Waypoints.Remove(n);
				}
				GUILayout.EndHorizontal();
			}
		});
		Widgets.Section("Players", delegate
		{
			List<object> list = TeleportCheats.AlivePlayers();
			if (list.Count == 0)
			{
				Widgets.Note("No other players found.");
			}
			else
			{
				_scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(140f));
				foreach (object item2 in list)
				{
					object player = item2;
					GUILayout.BeginHorizontal();
					GUILayout.Label(TeleportCheats.PlayerName(player), Theme.Body);
					GUILayout.FlexibleSpace();
					Widgets.ActionButton("Go to", () => TeleportCheats.GoToPlayer(player), "Teleported", Theme.Btn, 58f);
					Widgets.ActionButton("Bring", () => TeleportCheats.BringPlayer(player), "Player brought", Theme.Btn, 58f);
					GUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();
			}
		});
	}
}
