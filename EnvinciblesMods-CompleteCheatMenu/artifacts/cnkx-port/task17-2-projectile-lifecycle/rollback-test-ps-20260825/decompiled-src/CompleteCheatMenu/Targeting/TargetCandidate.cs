using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal sealed class TargetCandidate
{
	internal EntityRecord Record;

	internal object Entity;

	internal Transform Transform;

	internal Vector3 AimPoint;

	internal string Bone;

	internal float Distance;

	internal float Angle;

	internal Vector2 ScreenPosition;

	internal float ScreenDistance;

	internal bool Visible;

	internal Vector3 Velocity;

	internal bool Priority;

	internal float BoneScore;

	internal float EntityScore;
}
