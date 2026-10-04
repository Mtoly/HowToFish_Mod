using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.Targeting;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class WeaponsTab
{
	internal const string Title = "武器";

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason())) return;
		Widgets.Section("当前武器", DrawHeldWeapon);
		Widgets.Section("射击参数", DrawFiring);
		Widgets.Section("弹丸参数", DrawProjectiles);
		Widgets.Section("自动瞄准", DrawAimAssist);
	}

	private static void DrawHeldWeapon()
	{
		if (!WeaponCheats.HoldingWeapon)
		{
			Widgets.Note("请先手持枪械。切换武器时会安全恢复上一件武器的原值。");
			return;
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(WeaponCheats.WeaponName, Theme.H1);
		GUILayout.FlexibleSpace();
		GUILayout.Label($"弹药 {WeaponCheats.Ammo}", Theme.Muted);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		Widgets.ActionButton("立即补满弹药", WeaponCheats.RefillAmmo, "弹药已补满");
		Widgets.ActionButton("免费升级子弹", WeaponCheats.BuyBulletUpgrade, "子弹升级已应用", Theme.BtnAccent);
		GUILayout.EndHorizontal();
	}

	private static void DrawFiring()
	{
		CheatState.InfiniteAmmo = Widgets.ToggleRow("无限弹药", CheatState.InfiniteAmmo, OnOff(CheatState.InfiniteAmmo));
		if (CheatState.InfiniteAmmo)
		{
			CheatState.AmmoTopUp = Widgets.IntSliderRow("保持弹药数量", CheatState.AmmoTopUp, 1, 999);
			Widgets.Note("同时取消等待中的换弹，使枪械持续射击。");
		}
		CheatState.NoSpread = Widgets.ToggleRow("无散布", CheatState.NoSpread, OnOff(CheatState.NoSpread));
		CheatState.FireWhereLooking = Widgets.ToggleRow("朝准星射击", CheatState.FireWhereLooking, OnOff(CheatState.FireWhereLooking));
		Widgets.Note("将枪口方向与镜头方向对齐，消除枪管与视线之间的偏移。");
		CheatState.ZeroRecoil = Widgets.ToggleRow("无后坐力", CheatState.ZeroRecoil, OnOff(CheatState.ZeroRecoil));
		CheatState.RapidFire = Widgets.ToggleRow("快速射击", CheatState.RapidFire, OnOff(CheatState.RapidFire));
		if (CheatState.RapidFire) CheatState.FireInterval = Widgets.SliderRow("射击间隔（秒）", CheatState.FireInterval, 0.01f, 0.5f, "0.###", 0.05f);
	}

	private static void DrawProjectiles()
	{
		CheatState.MultiShot = Widgets.ToggleRow("多弹丸", CheatState.MultiShot, OnOff(CheatState.MultiShot));
		if (CheatState.MultiShot) CheatState.ProjectileCount = Widgets.SliderRow("每发弹丸数量", CheatState.ProjectileCount, 1f, 30f, "0", 1f);
		CheatState.FastBullets = Widgets.ToggleRow("高速子弹", CheatState.FastBullets, OnOff(CheatState.FastBullets));
		if (CheatState.FastBullets) CheatState.ProjectileSpeed = Widgets.SliderRow("高速子弹速度", CheatState.ProjectileSpeed, 20f, 2000f, "0", 300f);
		CheatState.InstantHit = Widgets.ToggleRow("瞬击", CheatState.InstantHit, OnOff(CheatState.InstantHit));
		if (CheatState.InstantHit)
		{
			CheatState.InstantHitProjectileSpeed = Widgets.SliderRow("瞬击等效弹速", CheatState.InstantHitProjectileSpeed, 1000f, 30000f, "0", 10000f);
			Widgets.Note("瞬击与高速子弹同时开启时，瞬击弹速优先。");
		}
		GUILayout.Space(4f);
		Widgets.ActionButton("恢复当前武器原值", WeaponCheats.ResetHeldWeapon, "已恢复原始数值", Theme.Btn);
		Widgets.Note(WeaponCheats.HasOriginals ? "当前武器的初始字段值已经保存。" : "当前武器尚未修改。");
	}

	private static void DrawAimAssist()
	{
		CheatState.Aimbot = Widgets.ToggleRow("自动瞄准", CheatState.Aimbot, CheatState.Aimbot ? "运行中" : "关闭");
		Widgets.Note("开火瞬间重新解算最佳目标并修正枪口方向；可见拉枪会在真实开火时平滑调整镜头。");
		GUILayout.Label("目标类型", Theme.Body);
		GUILayout.BeginHorizontal();
		if (GUILayout.Toggle(CheatState.AimTargets == AimTargetMode.Creatures, "  仅生物", Theme.Toggle)) CheatState.AimTargets = AimTargetMode.Creatures;
		if (GUILayout.Toggle(CheatState.AimTargets == AimTargetMode.Players, "  仅玩家", Theme.Toggle)) CheatState.AimTargets = AimTargetMode.Players;
		if (GUILayout.Toggle(CheatState.AimTargets == AimTargetMode.CreaturesAndPlayers, "  生物与玩家", Theme.Toggle)) CheatState.AimTargets = AimTargetMode.CreaturesAndPlayers;
		GUILayout.EndHorizontal();
		CheatState.AimRequireLineOfSight = Widgets.ToggleRow("需要视线", CheatState.AimRequireLineOfSight, OnOff(CheatState.AimRequireLineOfSight));
		GUILayout.BeginHorizontal();
		CheatState.AimExcludeDead = GUILayout.Toggle(CheatState.AimExcludeDead, "  排除死亡目标", Theme.Toggle);
		CheatState.AimExcludeSeagulls = GUILayout.Toggle(CheatState.AimExcludeSeagulls, "  排除海鸥", Theme.Toggle);
		CheatState.AimExcludeHookedFish = GUILayout.Toggle(CheatState.AimExcludeHookedFish, "  排除上钩鱼", Theme.Toggle);
		GUILayout.EndHorizontal();
		CheatState.AimPrioritizeAlbatross = Widgets.ToggleRow("信天翁优先", CheatState.AimPrioritizeAlbatross, OnOff(CheatState.AimPrioritizeAlbatross));
		CheatState.AimFov = Widgets.SliderRow("瞄准锥角（度）", CheatState.AimFov, 1f, 180f, "0", 35f);
		CheatState.AimRange = Widgets.SliderRow("最大距离", CheatState.AimRange, 10f, 600f, "0", 200f);
		CheatState.AimScreenRadius = Widgets.SliderRow("自瞄范围", CheatState.AimScreenRadius, 10f, 1000f, "0", 250f);
		Widgets.Note("最大距离使用三维世界距离；自瞄范围使用屏幕像素半径。");
		CheatState.AimHeightOffset = Widgets.SliderRow("回退瞄准高度", CheatState.AimHeightOffset, -1f, 3f, "0.##", 0.5f);
		CheatState.AimDistanceWeight = Widgets.SliderRow("距离权重", CheatState.AimDistanceWeight, 0f, 3f, "0.##", 1f);
		CheatState.AimAngleWeight = Widgets.SliderRow("角度权重", CheatState.AimAngleWeight, 0f, 3f, "0.##", 0.5f);
		CheatState.AimScreenDistanceWeight = Widgets.SliderRow("屏幕距离权重", CheatState.AimScreenDistanceWeight, 0f, 3f, "0.##", 0f);
		CheatState.AimPriorityBonus = Widgets.SliderRow("优先目标奖励", CheatState.AimPriorityBonus, 0f, 200f, "0", 50f);
		CheatState.AimPriorityBonusLimit = Widgets.SliderRow("优先奖励上限", CheatState.AimPriorityBonusLimit, 0f, 500f, "0", 200f);
		CheatState.AimLockDuration = Widgets.SliderRow("目标保持时间", CheatState.AimLockDuration, 0f, 1f, "0.##", 0.2f);
		CheatState.AimSwitchPenalty = Widgets.SliderRow("切换目标惩罚", CheatState.AimSwitchPenalty, 0f, 30f, "0.#", 8f);
		CheatState.AimPrediction = Widgets.ToggleRow("弹道预测", CheatState.AimPrediction, OnOff(CheatState.AimPrediction));
		if (CheatState.AimPrediction)
		{
			CheatState.AimMaxPredictionTime = Widgets.SliderRow("最大预测时间", CheatState.AimMaxPredictionTime, 0.1f, 5f, "0.##", 2f);
			CheatState.AimVelocityMinInterval = Widgets.SliderRow("速度采样最小间隔", CheatState.AimVelocityMinInterval, 0.005f, 0.2f, "0.###", 0.02f);
			CheatState.AimTeleportDistance = Widgets.SliderRow("传送距离阈值", CheatState.AimTeleportDistance, 1f, 100f, "0.#", 25f);
			CheatState.AimMaxTargetSpeed = Widgets.SliderRow("目标最大速度", CheatState.AimMaxTargetSpeed, 10f, 500f, "0", 250f);
			CheatState.AimGravityCompensation = Widgets.ToggleRow("重力补偿", CheatState.AimGravityCompensation, OnOff(CheatState.AimGravityCompensation));
			if (CheatState.AimGravityCompensation) CheatState.AimGravityScale = Widgets.SliderRow("重力补偿倍率", CheatState.AimGravityScale, 0f, 3f, "0.##", 1f);
		}
		CheatState.AimTrackingChance = Widgets.SliderRow("追踪概率（百分比）", CheatState.AimTrackingChance, 0f, 100f, "0", 100f);
		CheatState.AimProjectileTrackingTime = Widgets.SliderRow("弹丸追踪时长", CheatState.AimProjectileTrackingTime, 0f, 10f, "0.##", 3f);
		CheatState.AimMagicBulletStyle = Widgets.ToggleRow("静默弹道模式", CheatState.AimMagicBulletStyle, OnOff(CheatState.AimMagicBulletStyle));
		CheatState.AimProjectileUpdateInterval = Widgets.SliderRow("制导更新间隔", CheatState.AimProjectileUpdateInterval, 0f, 0.2f, "0.###", 0.02f);
		CheatState.AimProjectileTurnSpeed = Widgets.SliderRow("最大转向速度", CheatState.AimProjectileTurnSpeed, 0f, 3600f, "0", 720f);
		CheatState.AimReturnToLaunchVelocity = Widgets.ToggleRow("目标失效后回正", CheatState.AimReturnToLaunchVelocity, OnOff(CheatState.AimReturnToLaunchVelocity));
		if (CheatState.AimReturnToLaunchVelocity)
		{
			CheatState.AimReturnDuration = Widgets.SliderRow("回正持续时间", CheatState.AimReturnDuration, 0.01f, 1f, "0.##", 0.15f);
			CheatState.AimReturnTurnSpeed = Widgets.SliderRow("回正转向速度", CheatState.AimReturnTurnSpeed, 0f, 3600f, "0", 720f);
		}
		Widgets.Note("静默弹道模式只在弹丸生成前重定向一次真实速度，飞行中不再持续改写速度；关闭后恢复连续制导。");
		CheatState.AimVisibleAssist = Widgets.ToggleRow("可见拉枪", CheatState.AimVisibleAssist, OnOff(CheatState.AimVisibleAssist));
		if (CheatState.AimVisibleAssist)
		{
			CheatState.AimVisibleResponse = Widgets.SliderRow("拉枪响应速度", CheatState.AimVisibleResponse, 0.5f, 30f, "0.##", 8f);
			CheatState.AimDistanceDropPerUnit = Widgets.SliderRow("距离下调", CheatState.AimDistanceDropPerUnit, 0f, 0.05f, "0.####", 0f);
			CheatState.AimRecoilCompensation = Widgets.ToggleRow("后坐补偿", CheatState.AimRecoilCompensation, OnOff(CheatState.AimRecoilCompensation));
			if (CheatState.AimRecoilCompensation) CheatState.AimRecoilCompensationStrength = Widgets.SliderRow("后坐补偿强度", CheatState.AimRecoilCompensationStrength, 0f, 2f, "0.##", 1f);
		}
		CheatState.AimTrackingDiagnostics = Widgets.ToggleRow("追踪诊断日志", CheatState.AimTrackingDiagnostics, OnOff(CheatState.AimTrackingDiagnostics));
		TargetSolution target = WeaponCheats.CurrentTarget;
		GUILayout.Space(4f);
		if (target == null)
		{
			GUILayout.Label(CheatState.Aimbot ? "当前范围内没有可用目标。" : "自动瞄准已关闭。", Theme.Muted);
			return;
		}
		string name = (target.Entity as Object)?.name ?? target.Transform?.name ?? "-";
		GUILayout.Label($"目标：{name}　距离：{target.Distance:0.0} 米　角度：{target.Angle:0.0}°　瞄准点：{target.Bone}　可见：{(target.Visible ? "是" : "否")}", Theme.Body);
	}

	private static string OnOff(bool value) => value ? "开启" : "关闭";
}


