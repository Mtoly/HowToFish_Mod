using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class MagicShotContext
{
	internal static bool IsActive { get; private set; }

	internal static Weapon ActiveWeapon { get; private set; }

	internal static TargetSolution ActiveTarget { get; private set; }

	internal static WeaponInfo ActiveWeaponInfo { get; private set; }

	internal static Transform ActiveFirePoint { get; private set; }

	internal static Quaternion OriginalFirePointRotation { get; private set; }

	internal static bool HasOriginalFirePointRotation { get; private set; }

	internal static bool Start(Weapon weapon)
	{
		Clear();
		if (!ProjectileOwnership.IsLocalWeapon(weapon))
		{
			return false;
		}
		ActiveWeapon = weapon;
		try
		{
			object attachments = GameBinder.Property("Weapon", "Attachments")?.GetValue(weapon);
			ActiveFirePoint = GameBinder.Property("Attachments", "FirePoint")?.GetValue(attachments) as Transform;
			if (ActiveFirePoint != null)
			{
				OriginalFirePointRotation = ActiveFirePoint.rotation;
				HasOriginalFirePointRotation = true;
			}
		}
		catch
		{
			ActiveFirePoint = null;
			HasOriginalFirePointRotation = false;
		}
		return HasOriginalFirePointRotation;
	}

	internal static bool Begin(Weapon weapon, TargetSolution target)
	{
		if (ActiveWeapon == null || !ReferenceEquals(ActiveWeapon, weapon) || !ProjectileOwnership.IsLocalWeapon(weapon) || !ProjectileTracker.IsValidTarget(target))
		{
			return false;
		}
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

	internal static void RestoreFirePoint()
	{
		if (!HasOriginalFirePointRotation || ActiveFirePoint == null)
		{
			return;
		}
		try
		{
			ActiveFirePoint.rotation = OriginalFirePointRotation;
		}
		catch
		{
			// Preserve the original Shoot exception path if the transform disappears.
		}
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
		ActiveFirePoint = null;
		OriginalFirePointRotation = Quaternion.identity;
		HasOriginalFirePointRotation = false;
	}
}
