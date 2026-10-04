using CompleteCheatMenu.Runtime;

namespace CompleteCheatMenu.Targeting;

internal sealed class TargetingSettings
{
	internal float MaxDistance;

	internal float MaxAngle;

	internal float ScreenRadius;

	internal float DistanceWeight;

	internal float AngleWeight;

	internal float ScreenDistanceWeight;

	internal float SwitchPenalty;

	internal float PriorityBonus;

	internal float PriorityBonusLimit;

	internal bool RequireLineOfSight;

	internal static TargetingSettings FromCheatState()
	{
		return new TargetingSettings
		{
			MaxDistance = CheatState.AimRange,
			MaxAngle = CheatState.AimFov,
			ScreenRadius = CheatState.AimScreenRadius,
			DistanceWeight = CheatState.AimDistanceWeight,
			AngleWeight = CheatState.AimAngleWeight,
			ScreenDistanceWeight = CheatState.AimScreenDistanceWeight,
			SwitchPenalty = CheatState.AimSwitchPenalty,
			PriorityBonus = CheatState.AimPriorityBonus,
			PriorityBonusLimit = CheatState.AimPriorityBonusLimit,
			RequireLineOfSight = CheatState.AimRequireLineOfSight
		};
	}
}
