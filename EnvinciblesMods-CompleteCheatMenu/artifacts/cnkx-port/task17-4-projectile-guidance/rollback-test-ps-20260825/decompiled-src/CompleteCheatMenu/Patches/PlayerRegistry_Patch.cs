using CompleteCheatMenu.Runtime;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch(typeof(Player), "Awake")]
internal static class PlayerRegistry_Patch
{
	private static void Postfix(Player __instance)
	{
		EntityRegistry.RegisterPlayer(__instance);
	}
}
