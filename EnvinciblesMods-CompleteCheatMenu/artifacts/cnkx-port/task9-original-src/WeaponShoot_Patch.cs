using System;
using System.Reflection;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class WeaponShoot_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("Weapon", "Shoot", 0);
	}

	private static bool Prepare()
	{
		bool num = TargetMethod() != null;
		if (!num)
		{
			Plugin.Log.LogWarning((object)"Weapon.Shoot not found — aimbot disabled.");
		}
		return num;
	}

	private static void Prefix()
	{
		try
		{
			WeaponCheats.AimBeforeShot();
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError((object)("Aim failed: " + ex.Message));
		}
	}
}
