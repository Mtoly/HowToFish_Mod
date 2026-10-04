using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal sealed class TargetSolution
{
	internal object Entity;

	internal Transform Transform;

	internal Vector3 AimPoint;

	internal string Bone;

	internal float Distance;

	internal Vector2 ScreenPosition;

	internal float ScreenDistance;

	internal float Angle;

	internal bool Visible;

	internal Vector3 Velocity;

	internal float Score;

	internal float Timestamp;

	internal static TargetSolution FromCandidate(TargetCandidate candidate, float timestamp)
	{
		if (candidate == null)
		{
			return null;
		}
		return new TargetSolution
		{
			Entity = candidate.Entity,
			Transform = candidate.Transform,
			AimPoint = candidate.AimPoint,
			Bone = candidate.Bone,
			Distance = candidate.Distance,
			ScreenPosition = candidate.ScreenPosition,
			ScreenDistance = candidate.ScreenDistance,
			Angle = candidate.Angle,
			Visible = candidate.Visible,
			Velocity = candidate.Velocity,
			Score = candidate.EntityScore,
			Timestamp = timestamp
		};
	}
}
