using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using HarmonyLib;

namespace CompleteCheatMenu.Patches;

[HarmonyPatch]
internal static class ServerDropAll_Patch
{
	private static MethodBase TargetMethod()
	{
		return GameBinder.Method("PlayerInventory", "ServerDropAll", 2);
	}

	private static bool Prepare()
	{
		bool num = TargetMethod() != null;
		if (!num)
		{
			Plugin.Log.LogWarning((object)"PlayerInventory.ServerDropAll not found — keep inventory disabled.");
		}
		return num;
	}

	private static bool Prefix()
	{
		if (!CheatState.KeepInventory)
		{
			return true;
		}
		Plugin.Log.LogInfo((object)"Keep inventory: skipped the death drop.");
		Notifications.Show("Keep inventory: items retained");
		return false;
	}
}
