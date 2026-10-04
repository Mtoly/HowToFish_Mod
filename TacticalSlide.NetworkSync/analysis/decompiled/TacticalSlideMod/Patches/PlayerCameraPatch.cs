using HarmonyLib;
using UnityEngine;

namespace TacticalSlideMod.Patches;

public static class PlayerCameraPatch
{
	[HarmonyPatch(typeof(PlayerCamera), "SetFov")]
	public static class PlayerCamera_SetFov_Patch
	{
		public static void Postfix(PlayerCamera __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && TacticalSlidePlugin.EnableFovWarp.Value && (!(__instance.Owner != null) || __instance.Owner.IsLocalClient) && Mathf.Abs(SlideController.CurrentFovOffset) > 0.01f)
			{
				Camera camera = _camRef.Invoke(__instance);
				if (camera != null)
				{
					camera.fieldOfView += SlideController.CurrentFovOffset;
				}
			}
		}
	}

	[HarmonyPatch(typeof(PlayerCamera), "SetCamPosRot")]
	public static class PlayerCamera_SetCamPosRot_Patch
	{
		public static void Postfix(PlayerCamera __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && TacticalSlidePlugin.EnableCameraTilt.Value && (!(__instance.Owner != null) || __instance.Owner.IsLocalClient) && (Mathf.Abs(SlideController.CurrentCameraHeightOffset) > 0.01f || Mathf.Abs(SlideController.CurrentCameraPitchOffset) > 0.01f))
			{
				Transform camTransform = __instance.CamTransform;
				if (camTransform != null)
				{
					camTransform.position += Vector3.up * SlideController.CurrentCameraHeightOffset;
					camTransform.eulerAngles += new Vector3(SlideController.CurrentCameraPitchOffset, 0f, 0f);
				}
			}
		}
	}

	private static readonly FieldRef<PlayerCamera, Camera> _camRef = AccessTools.FieldRefAccess<PlayerCamera, Camera>("_cam");
}
