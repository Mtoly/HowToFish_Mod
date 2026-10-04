using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Targeting;

internal static class MagicShotContext
{
	internal static bool IsActive { get; private set; }

	internal static Weapon ActiveWeapon { get; private set; }

	internal static TargetSolution ActiveTarget { get; private set; }

	internal static WeaponInfo ActiveWeaponInfo { get; private set; }

	internal static bool Begin(Weapon weapon, TargetSolution target)
	{
		Clear();
		if (!ProjectileOwnership.IsLocalWeapon(weapon) || !ProjectileTracker.IsValidTarget(target))
		{
			return false;
		}
		ActiveWeapon = weapon;
		ActiveTarget = target;
		GameBinder.TryGet<WeaponInfo>(ProjectileBindings.ActiveWeaponInfo, weapon, out WeaponInfo weaponInfo);
		ActiveWeaponInfo = weaponInfo;
		IsActive = true;
		return true;
	}

	internal static bool MatchesSpawn(Player owner, WeaponInfo weaponInfo, bool isLocal, bool fromNpc)
	{
		return IsActive && isLocal && !fromNpc && owner != null && ActiveWeapon != null
			&& ReferenceEquals(ActiveWeapon.Holder, owner)
			&& ActiveWeaponInfo != null && ReferenceEquals(ActiveWeaponInfo, weaponInfo)
			&& ProjectileTracker.IsValidTarget(ActiveTarget);
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
		ActiveWeaponInfo = null;
	}
}
