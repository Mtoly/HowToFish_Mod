using System;
using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.Targeting;
using HarmonyLib;
using UnityEngine;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class WeaponAddProjectile_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("ProjectileManager", "AddProjectile", 8);
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Prefix(ProjectileManager __instance, Player owner, WeaponInfo weaponInfo, bool isLocal, Vector3 pos, ref Vector3 velocity, uint id, bool fromNpc, out ProjectileSpawnState __state)
	{
		__state = default;
		try
		{
			if (!MagicShotContext.MatchesSpawn(owner, weaponInfo, isLocal, fromNpc))
			{
				return;
			}
			TargetSolution target = MagicShotContext.ActiveTarget;
			uint baseId = ProjectileSpawnRegistration.CaptureBaseId(__instance, isLocal, id);
			if (!InitialVelocityRedirector.TryRedirect(pos, velocity, target, weaponInfo.ProjectileGravity, CheatState.AimPrediction, CheatState.AimGravityCompensation, out Vector3 redirected))
			{
				return;
			}
			velocity = redirected;
			__state = new ProjectileSpawnState
			{
				Active = true,
				Owner = owner,
				BaseId = baseId,
				Count = 1,
				Target = target
			};
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError((object)("单弹丸初始速度注入失败：" + ex.Message));
		}
	}

	private static void Postfix(ProjectileManager __instance, ProjectileSpawnState __state)
	{
		if (__state.Active)
		{
			ProjectileSpawnRegistration.RegisterCreated(__instance, __state.Owner, __state.BaseId, __state.Target);
		}
	}
}
