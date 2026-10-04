using HarmonyLib;
using UnityEngine;

namespace TacticalSlideMod.Patches;

public static class PlayerLegsPatch
{
	[HarmonyPatch(typeof(PlayerLegs), "Update")]
	public static class PlayerLegs_Update_SlidePose_Patch
	{
		public static bool Prefix(PlayerLegs __instance)
		{
			if (__instance == null || !TacticalSlidePlugin.ModEnabled.Value)
			{
				return true;
			}
			if (SlideController.IsSliding)
			{
				Player player = _playerRef.Invoke(__instance);
				if (player == null || player.Body == null || player.Body.LowerBody == null)
				{
					return true;
				}
				Transform[] array = _legTargetsRef.Invoke(__instance);
				Transform[] array2 = _ikPolesRef.Invoke(__instance);
				Transform[] array3 = _footModelsRef.Invoke(__instance);
				if (array == null || array.Length < 2)
				{
					return true;
				}
				Vector3 position = player.Body.LowerBody.position;
				Vector3 currentSlideDir = SlideController.CurrentSlideDir;
				Vector3 normalized = Vector3.Cross(Vector3.up, currentSlideDir).normalized;
				Vector3 vector = position - Vector3.up * 0.45f;
				if (Physics.Raycast(position + Vector3.up * 0.2f, Vector3.down, out var hitInfo, 1.2f, GameInfo.CanJumpOnLayers, QueryTriggerInteraction.Ignore))
				{
					vector = hitInfo.point;
				}
				Vector3 position2 = vector + currentSlideDir * 0.45f + normalized * 0.12f + Vector3.up * 0.04f;
				array[0].position = position2;
				if (array2 != null && array2.Length >= 2 && array2[0] != null)
				{
					array2[0].position = position + currentSlideDir * 0.35f + Vector3.up * 0.15f;
				}
				if (array3 != null && array3.Length >= 2 && array3[0] != null && array3[0].parent != null)
				{
					array3[0].rotation = Quaternion.LookRotation(currentSlideDir, Vector3.up) * Quaternion.Euler(-12f, 0f, 0f);
				}
				Vector3 position3 = vector - currentSlideDir * 0.2f - normalized * 0.16f + Vector3.up * 0.06f;
				array[1].position = position3;
				if (array2 != null && array2.Length >= 2 && array2[1] != null)
				{
					array2[1].position = position - normalized * 0.25f + Vector3.up * 0.1f;
				}
				if (array3 != null && array3.Length >= 2 && array3[1] != null && array3[1].parent != null)
				{
					array3[1].rotation = Quaternion.LookRotation(currentSlideDir, Vector3.up) * Quaternion.Euler(18f, -20f, 0f);
				}
				return false;
			}
			return true;
		}
	}

	private static readonly FieldRef<PlayerLegs, Player> _playerRef = AccessTools.FieldRefAccess<PlayerLegs, Player>("_player");

	private static readonly FieldRef<PlayerLegs, Transform[]> _legTargetsRef = AccessTools.FieldRefAccess<PlayerLegs, Transform[]>("_legTargets");

	private static readonly FieldRef<PlayerLegs, Transform[]> _ikPolesRef = AccessTools.FieldRefAccess<PlayerLegs, Transform[]>("_ikPoles");

	private static readonly FieldRef<PlayerLegs, Transform[]> _footModelsRef = AccessTools.FieldRefAccess<PlayerLegs, Transform[]>("_footModels");
}
