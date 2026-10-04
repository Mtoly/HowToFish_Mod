namespace CompleteCheatMenu.Targeting;

internal static class MagicShotContext
{
	internal static bool IsActive { get; private set; }

	internal static Weapon ActiveWeapon { get; private set; }

	internal static TargetSolution ActiveTarget { get; private set; }

	internal static bool Begin(Weapon weapon, TargetSolution target)
	{
		Clear();
		if (!ProjectileOwnership.IsLocalWeapon(weapon) || !ProjectileTracker.IsValidTarget(target))
		{
			return false;
		}
		ActiveWeapon = weapon;
		ActiveTarget = target;
		IsActive = true;
		return true;
	}

	internal static void End()
	{
		Clear();
	}

	internal static void Clear()
	{
		IsActive = false;
		ActiveWeapon = null;
		ActiveTarget = null;
	}
}
