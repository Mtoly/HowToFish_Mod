using HarmonyLib;
using UnityEngine;

namespace TacticalSlideMod.Patches;

public static class PlayerBodyPatch
{
	[HarmonyPatch(typeof(PlayerBody), "SetBendPosRot")]
	public static class PlayerBody_SetBendPosRot_Patch
	{
		public static void Postfix(PlayerBody __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && TacticalSlidePlugin.EnableCameraTilt.Value && Mathf.Abs(SlideController.CurrentBodyLeanAngle) > 0.01f)
			{
				_bendRotRef.Invoke(__instance) *= Quaternion.Euler(SlideController.CurrentBodyLeanAngle, 0f, 0f);
			}
		}
	}

	private static readonly FieldRef<PlayerBody, Quaternion> _bendRotRef = AccessTools.FieldRefAccess<PlayerBody, Quaternion>("_bendRot");
}
