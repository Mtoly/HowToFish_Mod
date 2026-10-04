using HarmonyLib;
using UnityEngine;

namespace FishAimbotMod;

[HarmonyPatch(typeof(PlayerCamera), "ApplyAimAssist")]
public class AimAssistPatch
{
	private static readonly FieldRef<PlayerCamera, Vector3> GetRot = AccessTools.FieldRefAccess<PlayerCamera, Vector3>("_rot");

	private static readonly KeyCode AimKey = (KeyCode)324;

	private const float FovAngle = 180f;

	private static void Postfix(PlayerCamera __instance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKey(AimKey))
		{
			Vector3 position = __instance.CamTransform.position;
			Vector3 forward = __instance.CamTransform.forward;
			Transform bestTarget = FishTargeting.GetBestTarget(position, forward, 180f);
			if (!((Object)(object)bestTarget == (Object)null))
			{
				Vector3 val = bestTarget.position - position;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				float y = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f;
				float num = (0f - Mathf.Asin(normalized.y)) * 57.29578f;
				ref Vector3 reference = ref GetRot.Invoke(__instance);
				reference.x = Mathf.Clamp(num, -90f, 90f);
				reference.y = y;
			}
		}
	}
}
