using CompleteCheatMenu.Cheats;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch(typeof(Creature), "Awake")]
internal static class CreatureRegistry_Patch
{
	private static void Postfix(Creature __instance)
	{
		AimTargetRegistry.Register(__instance);
	}
}
