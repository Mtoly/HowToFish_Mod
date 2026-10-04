using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Targeting;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class ProjectileRemove_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("ProjectileManager", "AddToRemoveQueue", 1);
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Postfix(Projectile projectile)
	{
		ProjectileGuidance.Forget(projectile);
		ProjectileTracker.Unregister(projectile);
	}
}
