using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Targeting;

internal static class ProjectileOwnership
{
	internal static bool IsLocalWeapon(Weapon weapon)
	{
		if (weapon == null || Refs.LocalPlayer is not Player localPlayer)
		{
			return false;
		}
		Player holder = weapon.Holder;
		if (holder == null || !ReferenceEquals(holder, localPlayer) || holder.Owner == null)
		{
			return false;
		}
		return holder.Owner.IsLocalClient;
	}

	internal static bool IsLocalProjectile(Projectile projectile)
	{
		if (projectile == null || projectile.FromNpc || !projectile.IsLocal || Refs.LocalPlayer is not Player localPlayer)
		{
			return false;
		}
		return projectile.Owner != null && ReferenceEquals(projectile.Owner, localPlayer);
	}

	internal static bool CanTrack(Weapon weapon, Projectile projectile)
	{
		return IsLocalWeapon(weapon) && IsLocalProjectile(projectile);
	}
}
