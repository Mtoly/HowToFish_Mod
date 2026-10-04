using System;
using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Targeting;
using HarmonyLib;
using UnityEngine;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class ProjectileUpdateScan_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("ProjectileManager", "UpdateProjectileScan", 2);
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Prefix(Projectile projectile)
	{
		try
		{
			ProjectileGuidance.Step(projectile, Time.unscaledTime, Time.fixedDeltaTime);
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError((object)("飞行中弹丸制导失败：" + ex.Message));
		}
	}
}
