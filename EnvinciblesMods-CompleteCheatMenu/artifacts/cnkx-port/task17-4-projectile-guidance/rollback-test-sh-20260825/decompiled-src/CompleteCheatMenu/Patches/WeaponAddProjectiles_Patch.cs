using System;
using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.Targeting;
using HarmonyLib;
using UnityEngine;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class WeaponAddProjectiles_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("ProjectileManager", "AddProjectiles", 8);
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Prefix(ProjectileManager __instance, Player owner, WeaponInfo weaponInfo, bool isLocal, Vector3 pos, ref Vector3[] velocities, uint id, bool canHitOwner, out ProjectileSpawnState __state)
	{
		__state = default;
		try
		{
			if (!MagicShotContext.MatchesSpawn(owner, weaponInfo, isLocal, canHitOwner))
			{
				return;
			}
			TargetSolution target = MagicShotContext.ActiveTarget;
			uint baseId = ProjectileSpawnRegistration.CaptureBaseId(__instance, isLocal, id);
			if (!InitialVelocityRedirector.TryRedirectAll(pos, velocities, target, CheatState.AimPrediction, CheatState.AimGravityCompensation, out Vector3[] redirected))
			{
				return;
			}
			velocities = redirected;
			__state = new ProjectileSpawnState
			{
				Active = true,
				Owner = owner,
				BaseId = baseId,
				Count = redirected.Length,
				Target = target
			};
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError((object)("多弹丸初始速度注入失败：" + ex.Message));
		}
	}

	private static void Postfix(ProjectileManager __instance, ProjectileSpawnState __state)
	{
		if (__state.Active)
		{
			ProjectileSpawnRegistration.RegisterCreatedRange(__instance, __state.Owner, __state.BaseId, __state.Count, __state.Target);
		}
	}
}
