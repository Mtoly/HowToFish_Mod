using System;
using System.Reflection;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class AddKillScore_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("PlayerKillScore", "AddKillScore", 3);
	}

	private static bool Prepare()
	{
		bool num = TargetMethod() != null;
		if (!num)
		{
			Plugin.Log.LogWarning((object)"PlayerKillScore.AddKillScore not found — kill score control disabled.");
		}
		return num;
	}

	private static void Prefix(object[] __args)
	{
		try
		{
			if (__args != null && __args.Length >= 3 && KillScoreCheats.ShouldOverride)
			{
				object obj = KillScoreCheats.BuildBonusList();
				if (obj != null)
				{
					__args[2] = obj;
				}
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError((object)("Kill score override failed: " + ex.Message));
		}
	}
}
