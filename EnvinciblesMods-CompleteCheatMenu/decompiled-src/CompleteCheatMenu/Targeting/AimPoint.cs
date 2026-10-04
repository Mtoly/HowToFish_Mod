using System.Collections.Generic;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal sealed class AimPoint
{
	internal readonly Transform Transform;
	internal readonly Vector3 Position;
	internal readonly string Bone;
	internal readonly string Source;
	internal Vector2 ScreenPosition;
	internal float ScreenDistance;

	internal AimPoint(Transform transform, Vector3 position, string bone, string source)
	{
		Transform = transform;
		Position = position;
		Bone = bone;
		Source = source;
	}

	internal static AimPoint SelectClosestProjected(IReadOnlyList<AimPoint> points, Camera camera, Vector2 screenCenter)
	{
		if (points == null || camera == null)
		{
			return null;
		}
		AimPoint best = null;
		float bestDistanceSquared = float.PositiveInfinity;
		for (int i = 0; i < points.Count; i++)
		{
			AimPoint point = points[i];
			if (point == null)
			{
				continue;
			}
			Vector3 screen = camera.WorldToScreenPoint(point.Position);
			if (screen.z <= 0f || !IsFinite(screen.x) || !IsFinite(screen.y) || !IsFinite(screen.z))
			{
				continue;
			}
			Vector2 position = new Vector2(screen.x, screen.y);
			float distanceSquared = (position - screenCenter).sqrMagnitude;
			if (distanceSquared < bestDistanceSquared)
			{
				bestDistanceSquared = distanceSquared;
				point.ScreenPosition = position;
				point.ScreenDistance = Mathf.Sqrt(distanceSquared);
				best = point;
			}
		}
		return best;
	}

	private static bool IsFinite(float value)
	{
		return !float.IsNaN(value) && !float.IsInfinity(value);
	}
}
