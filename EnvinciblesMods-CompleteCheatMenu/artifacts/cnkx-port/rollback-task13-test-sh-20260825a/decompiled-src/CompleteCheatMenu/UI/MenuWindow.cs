using System;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.UI.Tabs;
using UnityEngine;

namespace CompleteCheatMenu.UI;

internal static class MenuWindow
{
	private struct Tab
	{
		internal string Title;

		internal Action Draw;

		internal bool ServerSide;
	}

	private const int WindowId = 4408141;

	private static readonly Tab[] Tabs = new Tab[19]
	{
		new Tab
		{
			Title = "Player",
			Draw = PlayerTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Weapons",
			Draw = WeaponsTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Movement",
			Draw = MovementTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "Boat",
			Draw = BoatTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Teleport",
			Draw = TeleportTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Spawn",
			Draw = SpawnTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Entities",
			Draw = EntitiesTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Items",
			Draw = ItemsTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Skins",
			Draw = SkinsTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Money",
			Draw = MoneyTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "World",
			Draw = WorldTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Kill score",
			Draw = KillScoreTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Progression",
			Draw = ProgressionTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Gambling",
			Draw = GamblingTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Fishing",
			Draw = FishingTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "Visuals",
			Draw = VisualsTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "Keybinds",
			Draw = KeybindsTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "Settings",
			Draw = SettingsTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "Diagnostics",
			Draw = DiagnosticsTab.Draw,
			ServerSide = false
		}
	};

	private static int _active;

	private static Rect _rect = new Rect(80f, 80f, 620f, 520f);

	private static Vector2 _contentScroll;

	private static bool _resizing;

	internal static bool Visible { get; private set; }

	internal static void Toggle()
	{
		Visible = !Visible;
	}

	internal static void Draw()
	{
		if (Visible)
		{
			Theme.EnsureBuilt();
			_rect = GUILayout.Window(4408141, _rect, DrawWindow, "完整作弊菜单  v0.7.0", Theme.Window);
			DrawToasts();
		}
	}

	private static void DrawWindow(int id)
	{
		Keybinds.HandleCapture(Event.current);
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(Theme.SidebarBox, GUILayout.Width(132f));
		for (int i = 0; i < Tabs.Length; i++)
		{
			if (GUILayout.Button((Tabs[i].ServerSide && !Refs.IsHost) ? (Tabs[i].Title + "  ·") : Tabs[i].Title, (i == _active) ? Theme.TabActive : Theme.TabNormal))
			{
				_active = i;
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.Label(Refs.IsHost ? "HOST" : (Refs.InGame ? "CLIENT" : "MENU"), Theme.Muted);
		if (!Refs.IsHost)
		{
			GUILayout.Label("· = needs host", Theme.Muted);
		}
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
		_contentScroll = GUILayout.BeginScrollView(_contentScroll);
		try
		{
			Tabs[_active].Draw();
		}
		catch (Exception ex)
		{
			GUILayout.Label("This tab hit an error.", Theme.H2);
			GUILayout.Label(ex.Message, Theme.Muted);
			Plugin.Log.LogError((object)$"Tab '{Tabs[_active].Title}' failed: {ex}");
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		HandleResize();
		GUI.DragWindow(new Rect(0f, 0f, _rect.width, 24f));
	}

	private static void HandleResize()
	{
		Rect position = new Rect(_rect.width - 16f, _rect.height - 16f, 14f, 14f);
		GUI.Label(position, "◢", Theme.Muted);
		Event current = Event.current;
		if (current.type == EventType.MouseDown && position.Contains(current.mousePosition))
		{
			_resizing = true;
		}
		if (current.type == EventType.MouseUp)
		{
			_resizing = false;
		}
		if (_resizing && current.type == EventType.MouseDrag)
		{
			_rect.width = Mathf.Clamp(_rect.width + current.delta.x, 460f, Screen.width);
			_rect.height = Mathf.Clamp(_rect.height + current.delta.y, 320f, Screen.height);
			current.Use();
		}
	}

	private static void DrawToasts()
	{
		float num = 12f;
		foreach (Notifications.Toast item in Notifications.Active)
		{
			GUIStyle gUIStyle = (item.Ok ? Theme.ToastOk : Theme.ToastBad);
			float num2 = Mathf.Max(gUIStyle.CalcSize(new GUIContent(item.Text)).x + 20f, 160f);
			GUI.Label(new Rect((float)Screen.width - num2 - 16f, num, num2, 26f), item.Text, gUIStyle);
			num += 30f;
		}
	}
}
