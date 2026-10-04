using System;
using System.Reflection;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Targeting;
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

	private static void Prefix(Weapon __instance)
	{
		MagicShotContext.Clear();
		try
		{
			TargetSolution solution = WeaponCheats.PrepareShot(__instance);
			if (solution != null)
			{
				MagicShotContext.Begin(__instance, solution);
			}
		}
		catch (Exception ex)
		{
			MagicShotContext.Clear();
			Plugin.Log.LogError((object)("Aim failed: " + ex.Message));
		}
	}

	private static Exception Finalizer(Exception __exception)
	{
		MagicShotContext.End();
		return __exception;
	}
}
