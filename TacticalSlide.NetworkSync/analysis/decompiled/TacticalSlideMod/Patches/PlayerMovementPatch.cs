using HarmonyLib;
using UnityEngine;

namespace TacticalSlideMod.Patches;

public static class PlayerMovementPatch
{
	[HarmonyPatch(typeof(PlayerMovement), "FixedUpdate")]
	public static class PlayerMovement_FixedUpdate_Patch
	{
		public static void Prefix(PlayerMovement __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && (!(__instance.Owner != null) || __instance.Owner.IsLocalClient))
			{
				SlideController.Instance?.CheckAndConsumeSlideInput(__instance);
			}
		}
	}

	[HarmonyPatch(typeof(PlayerMovement), "Move")]
	public static class PlayerMovement_Move_Patch
	{
		public static void Postfix(PlayerMovement __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && (!(__instance.Owner != null) || __instance.Owner.IsLocalClient) && SlideController.IsSliding)
			{
				Rigidbody rigidbody = _rigRef.Invoke(__instance);
				if (rigidbody != null)
				{
					ref Vector3 curVel = ref _curVelRef.Invoke(__instance);
					SlideController.Instance?.ApplySlidePhysics(__instance, rigidbody, ref curVel);
				}
			}
		}
	}

	[HarmonyPatch(typeof(PlayerMovement), "Jump")]
	public static class PlayerMovement_Jump_Patch
	{
		public static void Prefix(PlayerMovement __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && (!(__instance.Owner != null) || __instance.Owner.IsLocalClient) && SlideController.IsSliding)
			{
				Rigidbody rig = _rigRef.Invoke(__instance);
				SlideController.Instance?.OnSlideHop(__instance, rig);
			}
		}
	}

	[HarmonyPatch(typeof(PlayerMovement), "Crouch")]
	public static class PlayerMovement_Crouch_Patch
	{
		public static void Postfix(PlayerMovement __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && SlideController.IsSliding)
			{
				CapsuleCollider capsuleCollider = _colRef.Invoke(__instance);
				float num = _origColHeightRef.Invoke(__instance);
				if (capsuleCollider != null && num > 0.1f)
				{
					float num2 = num * 0.45f;
					float num3 = (num - num2) * 0.5f;
					capsuleCollider.height = num2;
					capsuleCollider.center = Vector3.up * num3;
				}
			}
		}
	}

	private static readonly FieldRef<PlayerMovement, Rigidbody> _rigRef = AccessTools.FieldRefAccess<PlayerMovement, Rigidbody>("_rig");

	private static readonly FieldRef<PlayerMovement, Vector3> _curVelRef = AccessTools.FieldRefAccess<PlayerMovement, Vector3>("_curVel");

	private static readonly FieldRef<PlayerMovement, CapsuleCollider> _colRef = AccessTools.FieldRefAccess<PlayerMovement, CapsuleCollider>("_col");

	private static readonly FieldRef<PlayerMovement, float> _origColHeightRef = AccessTools.FieldRefAccess<PlayerMovement, float>("_origColHeight");
}
