using System;
using System.Reflection;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class RouletteResult_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("CasinoManager", "ServerRouletteResult", 1);
	}

	private static bool Prepare()
	{
		bool found = TargetMethod() != null;
		if (!found)
		{
			Plugin.Log.LogWarning((object)"CasinoManager.ServerRouletteResult not found — forced roulette result disabled.");
		}
		return found;
	}

	private static void Prefix(object[] __args)
	{
		try
		{
			if (GamblingCheats.OverrideRouletteResult(__args))
			{
				Plugin.Log.LogInfo((object)("Roulette result overridden to " + GamblingCheats.ForcedColour + " (host result path)."));
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError((object)("Forced roulette result override failed: " + ex.Message));
		}
	}
}
