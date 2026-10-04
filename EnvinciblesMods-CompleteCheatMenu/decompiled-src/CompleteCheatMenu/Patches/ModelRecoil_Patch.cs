using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class ModelRecoil_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("Weapon", "AddModelRecoil", 1);
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static bool Prefix()
	{
		return !CheatState.ZeroRecoil;
	}
}
