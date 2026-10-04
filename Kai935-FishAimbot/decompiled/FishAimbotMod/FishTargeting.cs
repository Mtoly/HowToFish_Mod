using UnityEngine;

namespace FishAimbotMod;

public static class FishTargeting
{
	public static Transform GetBestTarget(Vector3 camPos, Vector3 lookDir, float maxFov)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		Creature[] array = Object.FindObjectsByType<Creature>((FindObjectsSortMode)0);
		Transform result = null;
		float num = float.MaxValue;
		Creature[] array2 = array;
		foreach (Creature val in array2)
		{
			if ((Object)(object)val == (Object)null || val.IsDead)
			{
				continue;
			}
			Fish val2 = (Fish)(object)((val is Fish) ? val : null);
			if (val2 != null && (Object)(object)((Item)val2).AttachedRod != (Object)null)
			{
				continue;
			}
			string text = ((Object)val).name.ToLower();
			if (text.Contains("seagull"))
			{
				continue;
			}
			Vector3 position = ((Component)val).transform.position;
			Vector3 val3 = position - camPos;
			Vector3 normalized = ((Vector3)(ref val3)).normalized;
			float num2 = Vector3.Angle(lookDir, normalized);
			if (!(num2 > maxFov / 2f))
			{
				float num3 = Vector3.Distance(camPos, position);
				float num4 = (text.Contains("albatross") ? (num2 * 0.2f) : (num3 + num2 * 0.5f));
				if (num4 < num)
				{
					num = num4;
					result = ((Component)val).transform;
				}
			}
		}
		return result;
	}
}
