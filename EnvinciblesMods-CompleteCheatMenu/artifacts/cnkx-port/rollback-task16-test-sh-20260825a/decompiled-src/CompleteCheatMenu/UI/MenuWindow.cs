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
			Title = "玩家",
			Draw = PlayerTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "武器",
			Draw = WeaponsTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "移动",
			Draw = MovementTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "船只",
			Draw = BoatTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "传送",
			Draw = TeleportTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "生成",
			Draw = SpawnTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "实体",
			Draw = EntitiesTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "物品",
			Draw = ItemsTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "外观",
			Draw = SkinsTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "金钱",
			Draw = MoneyTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "世界",
			Draw = WorldTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "击杀分数",
			Draw = KillScoreTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "游戏进度",
			Draw = ProgressionTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "赌博",
			Draw = GamblingTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "钓鱼",
			Draw = FishingTab.Draw,
			ServerSide = true
		},
		new Tab
		{
			Title = "视觉",
			Draw = VisualsTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "按键",
			Draw = KeybindsTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "设置",
			Draw = SettingsTab.Draw,
			ServerSide = false
		},
		new Tab
		{
			Title = "诊断",
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
			if (GUILayout.Button(Tabs[i].Title, (i == _active) ? Theme.TabActive : Theme.TabNormal))
			{
				_active = i;
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.Label(Refs.IsHost ? "房主：全部功能" : (Refs.InGame ? "客户端：本地功能可用" : "主菜单"), Theme.Muted);
		if (Refs.InGame && !Refs.IsHost)
		{
			GUILayout.Label("部分操作由房主同步", Theme.Muted);
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

