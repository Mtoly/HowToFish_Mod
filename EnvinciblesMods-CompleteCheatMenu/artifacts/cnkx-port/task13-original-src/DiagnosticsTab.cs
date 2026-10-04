using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class DiagnosticsTab
{
	internal const string Title = "Diagnostics";

	private static Vector2 _scroll;

	internal static void Draw()
	{
		Widgets.Section("Status", delegate
		{
			Line("In game", Refs.InGame);
			Line("Host / solo", Refs.IsHost);
			Line("Dev cheats unlocked", CheatGate.CheatsEnabled);
			Line("Money API", MoneyCheats.Available);
			Line("Movement API", MovementCheats.Available);
			Line("Spawn API", SpawnCheats.Available);
			Line("Spawn catalogue", SpawnCatalog.Loaded, $"{SpawnCatalog.Count} entries");
			Line("Boat present", BoatCheats.Available);
			Line("Casino present", GamblingCheats.Available);
			Line("Skin machine", GamblingCheats.SlotsAvailable);
			Line("Inventory API", FishingCheats.Available);
			Line("Boss active", WorldCheats.BossActive);
			Line("FOV control", VisualCheats.FovAvailable);
			Line("Free camera", VisualCheats.FreeCamAvailable);
			Line("Holding a weapon", WeaponCheats.HoldingWeapon, WeaponCheats.WeaponName);
			Line("Kill score control", KillScoreCheats.Available, $"{KillScoreCheats.Catalogue.Count} bonuses");
		});
		Widgets.Section("Bindings", delegate
		{
			int failed = BindingReport.Failed;
			GUILayout.Label((failed == 0) ? $"All {BindingReport.Total} resolved." : $"{failed} of {BindingReport.Total} failed to resolve.", (failed == 0) ? Theme.Body : Theme.H2);
			if (GUILayout.Button("Re-run checks", Theme.Btn, GUILayout.Width(120f)))
			{
				RunChecks();
			}
		});
		_scroll = GUILayout.BeginScrollView(_scroll);
		foreach (BindingReport.Entry item in BindingReport.All)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(item.Ok ? "OK" : "MISSING", item.Ok ? Theme.Muted : Theme.H2, GUILayout.Width(64f));
			GUILayout.Label(item.Name, Theme.Body);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
	}

	private static void Line(string label, bool ok, string extra = null)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, Theme.Body, GUILayout.Width(170f));
		GUILayout.Label(ok ? "yes" : "no", ok ? Theme.Body : Theme.H2, GUILayout.Width(40f));
		if (!string.IsNullOrEmpty(extra))
		{
			GUILayout.Label(extra, Theme.Muted);
		}
		GUILayout.EndHorizontal();
	}

	internal static void RunChecks()
	{
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
