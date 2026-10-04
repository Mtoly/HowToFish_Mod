using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class RendererBoundsResolver
{
	internal static bool TryResolve(Transform root, out Bounds bounds)
	{
		bounds = default(Bounds);
		if (root == null)
		{
			return false;
		}
		Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
		bool found = false;
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer renderer = renderers[i];
			if (renderer == null)
			{
				continue;
			}
			Bounds current = renderer.bounds;
			if (!IsFinite(current.center) || !IsFinite(current.size) || current.size.sqrMagnitude <= 0.000001f)
			{
				continue;
			}
			if (!found)
			{
				bounds = current;
				found = true;
			}
			else
			{
				bounds.Encapsulate(current);
			}
		}
		return found;
	}

	internal static Vector3 UpperPoint(Bounds bounds)
	{
		return bounds.center + Vector3.up * (bounds.extents.y * 0.5f);
	}

	private static bool IsFinite(Vector3 value)
	{
		return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
	}

	private static bool IsFinite(float value)
	{
		return !float.IsNaN(value) && !float.IsInfinity(value);
	}
}
