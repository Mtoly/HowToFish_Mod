using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class VisualsTab
{
	internal const string Title = "视觉";

	internal static void Draw()
	{
		Widgets.Section("透视绘制", DrawEsp);
		Widgets.Section("镜头", DrawCamera);
		Widgets.Section("画面效果", DrawEffects);
	}

	private static void DrawEsp()
	{
		int registered = EntityRegistry.Creatures.Count + EntityRegistry.Players.Count + EntityRegistry.Items.Count + EntityRegistry.Containers.Count;
		CheatState.EspEnabled = Widgets.ToggleRow("启用透视", CheatState.EspEnabled, CheatState.EspEnabled ? $"已注册 {registered} 个目标" : "关闭");
		GUILayout.BeginHorizontal();
		CheatState.EspCreatures = GUILayout.Toggle(CheatState.EspCreatures, "  生物", Theme.Toggle);
		CheatState.EspItems = GUILayout.Toggle(CheatState.EspItems, "  物品与容器", Theme.Toggle);
		CheatState.EspPlayers = GUILayout.Toggle(CheatState.EspPlayers, "  玩家", Theme.Toggle);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		CheatState.EspBoxes = GUILayout.Toggle(CheatState.EspBoxes, "  方框", Theme.Toggle);
		CheatState.EspTracers = GUILayout.Toggle(CheatState.EspTracers, "  射线", Theme.Toggle);
		CheatState.EspSkeletons = GUILayout.Toggle(CheatState.EspSkeletons, "  骨架", Theme.Toggle);
		CheatState.EspHealthBars = GUILayout.Toggle(CheatState.EspHealthBars, "  血条", Theme.Toggle);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		CheatState.EspLabels = GUILayout.Toggle(CheatState.EspLabels, "  名称", Theme.Toggle);
		CheatState.EspDistanceLabels = GUILayout.Toggle(CheatState.EspDistanceLabels, "  距离", Theme.Toggle);
		CheatState.EspTypeInfo = GUILayout.Toggle(CheatState.EspTypeInfo, "  类型", Theme.Toggle);
		CheatState.EspVisibilityInfo = GUILayout.Toggle(CheatState.EspVisibilityInfo, "  可见状态", Theme.Toggle);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		CheatState.EspHeldItemInfo = GUILayout.Toggle(CheatState.EspHeldItemInfo, "  手持物", Theme.Toggle);
		CheatState.EspItemValueInfo = GUILayout.Toggle(CheatState.EspItemValueInfo, "  物品价值", Theme.Toggle);
		CheatState.EspContainerInfo = GUILayout.Toggle(CheatState.EspContainerInfo, "  容器信息", Theme.Toggle);
		GUILayout.EndHorizontal();
		CheatState.EspAimCircle = Widgets.ToggleRow("锁定范围圆", CheatState.EspAimCircle, CheatState.EspAimCircle ? "开启" : "关闭");
		CheatState.EspDistance = Widgets.SliderRow("最大绘制距离", CheatState.EspDistance, 20f, 600f, "0", 150f);
		Widgets.Note("实体列表使用注册表快照；几何、骨骼和可见性使用短周期缓存，避免逐帧全量扫描。");
	}

	private static void DrawCamera()
	{
		if (!VisualCheats.FovAvailable)
		{
			Widgets.Note("视野控制入口尚未解析。");
		}
		else
		{
			float fov = Widgets.SliderRow("视野角度", CheatState.Fov, 30f, 140f, "0", 60f);
			if (!Mathf.Approximately(fov, CheatState.Fov))
			{
				CheatState.Fov = fov;
				VisualCheats.SetFov(fov);
			}
		}
		if (!VisualCheats.FreeCamAvailable)
		{
			Widgets.Note("自由镜头需要已经加载的玩家镜头。");
			return;
		}
		bool active = VisualCheats.FreeCamActive;
		bool next = Widgets.ToggleRow("自由镜头", active, active ? "运行中" : "关闭");
		if (next != active) Notifications.Result(VisualCheats.ToggleFreeCam(next), next ? "自由镜头已开启" : "自由镜头已关闭", "自由镜头切换失败");
		CheatState.FreeCamSpeed = Widgets.SliderRow("移动速度", CheatState.FreeCamSpeed, 1f, 80f, "0.#", 15f);
		CheatState.FreeCamSensitivity = Widgets.SliderRow("鼠标灵敏度", CheatState.FreeCamSensitivity, 0.2f, 8f, "0.##", 2f);
		Widgets.Note("鼠标观察，WASD 移动，空格和左 Ctrl 控制高度，Shift 加速。");
		if (VisualCheats.BodyAvailable)
		{
			bool shown = VisualCheats.BodyShown;
			bool showNext = Widgets.ToggleRow("显示自己的角色", shown, shown ? "可见" : "隐藏");
			if (showNext != shown) Notifications.Result(VisualCheats.ShowOwnBody(showNext), showNext ? "角色已显示" : "角色已隐藏", "角色模型切换失败");
		}
	}

	private static void DrawEffects()
	{
		bool damage = VisualCheats.DamageNumbers;
		bool nextDamage = Widgets.ToggleRow("伤害数字", damage, damage ? "开启" : "关闭");
		if (nextDamage != damage) VisualCheats.SetDamageNumbers(nextDamage);
		bool blood = VisualCheats.Blood;
		bool nextBlood = Widgets.ToggleRow("血液效果", blood, blood ? "开启" : "关闭");
		if (nextBlood != blood) VisualCheats.SetBlood(nextBlood);
		GUILayout.BeginHorizontal();
		Widgets.ActionButton("清除贴花", VisualCheats.ClearDecals, "贴花已清除");
		Widgets.ActionButton("隐藏贴花", () => VisualCheats.SetDecals(to: false), "贴花已隐藏");
		Widgets.ActionButton("显示贴花", () => VisualCheats.SetDecals(to: true), "贴花已显示");
		GUILayout.EndHorizontal();
	}
}
