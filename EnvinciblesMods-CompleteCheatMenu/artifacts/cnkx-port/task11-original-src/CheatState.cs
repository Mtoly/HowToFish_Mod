using CompleteCheatMenu.Cheats;

namespace CompleteCheatMenu.Runtime;

internal static class CheatState
{
	internal static bool Fly;

	internal static float FlySpeed = 20f;

	internal static float FlyHorizontalSpeed = 25f;

	internal static float FlyVerticalSpeed = 12f;

	internal static bool BoatFly;

	internal static float BoatFlySpeed = 15f;

	internal static float DespawnRadius = 30f;

	internal static bool InfiniteAmmo;

	internal static int AmmoTopUp = 99;

	internal static bool NoSpread;

	internal static bool ZeroRecoil;

	internal static bool RapidFire;

	internal static float FireInterval = 0.05f;

	internal static bool MultiShot;

	internal static float ProjectileCount = 1f;

	internal static bool FastBullets;

	internal static float ProjectileSpeed = 300f;

	internal static bool FireWhereLooking;

	internal static bool Aimbot;

	internal static bool AimAtCreatures = true;

	internal static bool AimAtPlayers;

	internal static bool AimRequireLineOfSight = true;

	internal static float AimFov = 35f;

	internal static float AimRange = 200f;

	internal static float AimHeightOffset = 0.5f;

	internal static bool AimExcludeDead = true;

	internal static bool AimExcludeSeagulls = true;

	internal static bool AimExcludeHookedFish = true;

	internal static bool AimPrioritizeAlbatross = true;

	internal static float AimDistanceWeight = 1f;

	internal static float AimAngleWeight = 0.5f;

	internal static float AimScreenRadius = 250f;

	internal static float AimScreenDistanceWeight;

	internal static float AimPriorityBonus = 50f;

	internal static float AimPriorityBonusLimit = 200f;

	internal static float AimLockDuration = 0.2f;

	internal static float AimSwitchPenalty = 8f;

	internal static bool AimPrediction;

	internal static float AimVelocityMinInterval = 0.02f;

	internal static float AimTeleportDistance = 25f;

	internal static float AimMaxTargetSpeed = 250f;

	internal static bool AimGravityCompensation;

	internal static float AimGravityScale = 1f;

	internal static float AimTrackingChance = 100f;

	internal static bool AimTrackingDiagnostics;

	internal static bool AimVisibleAssist;

	internal static float AimVisibleResponse = 8f;

	internal static float AimDistanceDropPerUnit;

	internal static bool AimRecoilCompensation;

	internal static float AimRecoilCompensationStrength = 1f;

	internal static bool KillScoreOverride;

	internal static float KillFlatMultiplier = 1f;

	internal static string KillBonusLabel = "Rigged";

	internal static bool KeepInventory;

	internal static int DuplicateCount = 5;

	internal static string SpawnFilter = "";

	internal static int SpawnCount = 1;

	internal static bool SpawnDead;

	internal static bool SpawnDrip;

	internal static SpawnCatalog.SortMode SpawnSort = SpawnCatalog.SortMode.Name;

	internal static SpawnCatalog.Category? SpawnCategory;

	internal static string MoneyAmount = "10000";

	internal static int HealAmount = 100;

	internal static string WaypointName = "spot 1";

	internal static ItemCheats.Target ItemTarget = ItemCheats.Target.Held;

	internal static string ItemWorth = "1000";

	internal static float ItemCookness = 1f;

	internal static float ItemWeight = 1f;

	internal static float ItemKillScore = 1f;

	internal static float ItemBetting = 1f;

	internal static string SkinFilter = "";

	internal static int BossHeal = 500;

	internal static int BetColorIndex;

	internal static float BetBoost = 1f;

	internal static string SlotSkinFilter = "";

	internal static string SlotTargetFilter = "";

	internal static string SlotRarity = "";

	internal static int SlotTargetIndex = -1;

	internal static bool EspEnabled;

	internal static bool EspCreatures = true;

	internal static bool EspItems;

	internal static bool EspPlayers = true;

	internal static bool EspTracers;

	internal static bool EspLabels = true;

	internal static bool EspDistanceLabels = true;

	internal static float EspDistance = 150f;

	internal static float Fov = 60f;

	internal static float FreeCamSpeed = 15f;

	internal static float FreeCamSensitivity = 2f;

	internal static string PresetName = "my setup";

	internal static string GlobalSearch = "";
}
