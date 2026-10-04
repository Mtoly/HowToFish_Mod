using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.Targeting;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class DiagnosticsTab
{
	internal const string Title = "诊断";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		Widgets.Section("运行状态", delegate
		{
			Line("已进入游戏", Refs.InGame);
			Line("本地功能", CheatGate.LocalFeaturesAvailable);
			Line("服务器写入", CheatGate.ServerFeaturesAvailable);
			Line("开发者作弊已解锁", CheatGate.CheatsEnabled);
			Line("金钱接口", MoneyCheats.Available);
			Line("移动接口", MovementCheats.Available);
			Line("生成接口", SpawnCheats.Available);
			Line("生成目录", SpawnCatalog.Loaded, $"{SpawnCatalog.Count} 个条目");
			Line("船只已存在", BoatCheats.Available);
			Line("赌场已存在", GamblingCheats.Available);
			Line("外观机器", GamblingCheats.SlotsAvailable);
			Line("背包接口", FishingCheats.Available);
			Line("首领活动中", WorldCheats.BossActive);
			Line("视野控制", VisualCheats.FovAvailable);
			Line("自由镜头", VisualCheats.FreeCamAvailable);
			Line("正在手持武器", WeaponCheats.HoldingWeapon, WeaponCheats.WeaponName);
			Line("击杀分数控制", KillScoreCheats.Available, $"{KillScoreCheats.Catalogue.Count} 个奖励");
			int creatures = EntityRegistry.Creatures.Count;
			int players = EntityRegistry.Players.Count;
			int items = EntityRegistry.Items.Count;
			int containers = EntityRegistry.Containers.Count;
			Line("目标注册", creatures + players + items + containers > 0, $"生物 {creatures} / 玩家 {players} / 物品 {items} / 容器 {containers}");
			Line("弹速字段", GameBinder.Field("Weapon", "_projSpeed") != null, "Weapon._projSpeed");
		Line("弹丸生命周期绑定", ProjectileBindings.LifecycleAvailable, $"活动 {ProjectileTracker.ActiveCount} / 注册 {ProjectileTracker.RegisteredCount} / 清理 {ProjectileTracker.UnregisteredCount}");
		Line("飞行中制导绑定", ProjectileBindings.GuidanceAvailable, $"活动 {ProjectileGuidance.ActiveCount} / 更新 {ProjectileGuidance.GuidedUpdateCount} / 超时 {ProjectileGuidance.ExpiredCount} / 间隔跳过 {ProjectileGuidance.IntervalSkipCount}");
			Line("人体骨骼接口", typeof(Animator).GetMethod("GetBoneTransform") != null, "Animator.GetBoneTransform");
			Line("包围盒回退", typeof(Renderer).GetProperty("bounds") != null, "Renderer.bounds");
			Line("开火追踪入口", GameBinder.Method("Weapon", "Shoot", 0) != null && GameBinder.Property("Attachments", "FirePoint") != null, "Weapon.Shoot → FirePoint");
		});
		Widgets.Section("绑定状态", delegate
		{
			int failed = BindingReport.Failed;
			GUILayout.Label((failed == 0) ? $"全部 {BindingReport.Total} 项已解析。" : $"{BindingReport.Total} 项中有 {failed} 项解析失败。", (failed == 0) ? Theme.Body : Theme.H2);
			if (GUILayout.Button("重新检查", Theme.Btn, GUILayout.Width(120f))) RunChecks();
		});
		_scroll = GUILayout.BeginScrollView(_scroll);
		foreach (BindingReport.Entry item in BindingReport.All)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(item.Ok ? "正常" : "缺失", item.Ok ? Theme.Muted : Theme.H2, GUILayout.Width(64f));
			GUILayout.Label(item.Name, Theme.Body);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
	}

	private static void Line(string label, bool ok, string extra = null)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, Theme.Body, GUILayout.Width(170f));
		GUILayout.Label(ok ? "是" : "否", ok ? Theme.Body : Theme.H2, GUILayout.Width(40f));
		if (!string.IsNullOrEmpty(extra)) GUILayout.Label(extra, Theme.Muted);
		GUILayout.EndHorizontal();
	}
	internal static void RunChecks()
	{
		ProjectileBindings.ResolveAll();
		_ = MoneyCheats.Available;
		_ = MovementCheats.Available;
		_ = SpawnCheats.Available;
		_ = CheatGate.CheatsEnabled;
		_ = Refs.IsHost;
		string[] fields = MovementCheats.Fields;
		foreach (string fieldName in fields)
		{
			GameBinder.Field("PlayerMovement", fieldName);
		}
		GameBinder.Method("PlayerManager", "ToggleGodMode", 0);
		GameBinder.Property("PlayerManager", "InGodMode");
		GameBinder.Method("ServerSettings", "ToggleOneShot", 0);
		GameBinder.Method("ServerSettings", "ToggleFriendlyFire", 1);
		GameBinder.Method("PlayerVitals", "Heal", 1);
		GameBinder.Method("PlayerVitals", "RestoreFullness", 1);
		GameBinder.Method("PlayerVitals", "ServerResetVitals", 0);
		GameBinder.Method("ClientSettings", "ToggleCheats", 1);
		GameBinder.Field("GameInfo", "_nameToSpawnable");
		GameBinder.Field("MoneyManager", "_money");
		GameBinder.Method("MoneyManager", "RemoveMoney", 2);
		GameBinder.Field("Player", "LocalPlayer");
		GameBinder.Property("BoatManager", "Boat");
		GameBinder.Method("Boat", "SetMotor", 1);
		GameBinder.Method("Boat", "UnlockBoat", 0);
		GameBinder.Method("BoatManager", "TryMoveBoat", 2);
		GameBinder.Method("OnlineIslandManager", "TpToSpecificIsland", 1);
		GameBinder.Method("OnlineIslandManager", "UnlockIsland", 1);
		GameBinder.Method("Server", "TeleportPlayer", 3);
		GameBinder.Property("PlayerManager", "AlivePlayers");
		GameBinder.Property("PlayerHolding", "HeldItem");
		GameBinder.Field("Item", "_worth");
		GameBinder.Field("Item", "_cookness");
		GameBinder.Method("Item", "SetKillscoreMultiplier", 1);
		GameBinder.Method("Server", "SetItemSkin", 2);
		GameBinder.Method("SaveManager", "UnlockSkin", 2);
		GameBinder.Method("SaveManager", "LockAllSkins", 0);
		GameBinder.Property("GameInfo", "ItemWithSkinsforCommands");
		GameBinder.Method("Server", "SetBoatSkin", 1);
		GameBinder.Method("GameInfo", "ToggleAllCreaturesKilled", 2);
		GameBinder.Method("BossManager", "ToggleImmortal", 1);
		GameBinder.Method("Creature", "HealBoss", 1);
		GameBinder.Method("NPCManager", "UnlockGrill", 0);
		GameBinder.Method("AchievementManager", "ToggleAllAchievements", 1);
		GameBinder.Method("DazedCommands", "UseFinishGameCommand", 0);
		GameBinder.Method("PlayerInventory", "UnlockExtraPocket", 1);
		GameBinder.Method("PlayerInventory", "ServerBoughtBait", 1);
		GameBinder.Method("PlayerInventory", "ServerSetCurBait", 1);
		GameBinder.Property("GameInfo", "AllBaits");
		GameBinder.Method("CasinoManager", "ServerRouletteResult", 1);
		GameBinder.Method("SlotMachineManager", "SetCheatSkin", 2);
		GameBinder.EnumNames("BetColor");
		GameBinder.Method("PlayerCamera", "SetFOV", 1);
		GameBinder.Property("PlayerCamera", "CamTransform");
		GameBinder.Method("DecalManager", "ToggleDamageNumbers", 1);
		GameBinder.Method("DecalManager", "ToggleBlood", 1);
		GameBinder.Method("DecalManager", "ToggleDecals", 1);
		GameBinder.Property("ItemManager", "Items");
		GameBinder.Property("Creature", "IsDead");
		GameBinder.Field("CasinoManager", "_curBetColor");
		GameBinder.Field("CasinoManager", "_itemsToBet");
		GameBinder.Property("CasinoManager", "IsBetting");
		GameBinder.Method("CasinoManager", "ServerStartBet", 1);
		GameBinder.Field("LocalCasino", "_wheel");
		GameBinder.Field("LocalCasino", "_ballAngleObject");
		GameBinder.Field("LocalCasino", "_slotSize");
		GameBinder.Field("LocalCasino", "_isPlaying");
		GameBinder.Field("LocalCasino", "_timeInSameSlot");
		GameBinder.Method("SlotMachineManager", "RollRandom", 1);
		GameBinder.Field("SlotMachineManager", "_cheatSkinIndex");
		GameBinder.Field("PlayerMovement", "_curMoveSpeed");
		GameBinder.Field("PlayerMovement", "_targetMoveSpeed");
		GameBinder.Method("PlayerMovement", "Teleport", 2);
		GameBinder.Property("Player", "Rigidbody");
		GameBinder.Property("Player", "Movement");
		GameBinder.Property("Player", "Skin");
		GameBinder.Field("Boat", "_motorIndex");
		GameBinder.Property("Boat", "HiddenPhysicsRig");
		GameBinder.Method("Creature", "ServerChangeHp", 1);
		GameBinder.Method("Item", "DestroyItem", 2);
		GameBinder.Method("PlayerSkin", "InitializeOther", 0);
		GameBinder.Field("PlayerSkin", "_bodyRenderer");
		GameBinder.Property("Item", "Weapon");
		GameBinder.Property("Weapon", "Attachments");
		GameBinder.Property("Weapon", "Ammo");
		GameBinder.Field("Weapon", "<Ammo>k__BackingField");
		GameBinder.Field("Weapon", "_spread");
		GameBinder.Field("Weapon", "_recoilKnockback");
		GameBinder.Field("Weapon", "_timeBetweenShots");
		GameBinder.Field("Weapon", "_projectileCountPerShot");
		GameBinder.Field("Weapon", "_projSpeed");
		GameBinder.Property("Attachments", "FirePoint");
		GameBinder.Method("Server", "BuyBulletUpgrade", 1);
		GameBinder.Method("PlayerKillScore", "AddKillScore", 3);
		GameBinder.Method("KillScoreCalculator", "GetAllBonuses", 1);
		GameBinder.Field("Bonus", "Name");
		GameBinder.Field("Bonus", "Worth");
		GameBinder.Method("PlayerInventory", "ServerDropAll", 2);
		GameBinder.Method("PlayerInventory", "SaveInventory", 1);
		GameBinder.Method("Server", "DropAllItems", 3);
		GameBinder.Method("Weapon", "Shoot", 0);
		GameBinder.Method("Weapon", "AddModelRecoil", 1);
		GameBinder.Method("PlayerCamera", "Recoil", 1);
		GameBinder.Method("PlayerToolMovement", "Recoil", 1);
		GameBinder.Field("Boat", "_motors");
		GameBinder.Field("Boat", "_propellerInWater");
		GameBinder.Property("BoatMotor", "Force");
		GameBinder.Field("BoatMotor", "<Force>k__BackingField");
		SpawnCatalog.Load();
		Plugin.Log.LogInfo((object)$"Diagnostics: {BindingReport.Failed}/{BindingReport.Total} bindings failed.");
	}
}



