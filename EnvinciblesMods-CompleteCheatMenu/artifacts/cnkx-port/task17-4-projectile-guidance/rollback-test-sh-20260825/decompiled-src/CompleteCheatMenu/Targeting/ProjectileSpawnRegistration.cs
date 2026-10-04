using System;
using System.Collections;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Targeting;

internal struct ProjectileSpawnState
{
	internal bool Active;

	internal Player Owner;

	internal uint BaseId;

	internal int Count;

	internal TargetSolution Target;
}

internal static class ProjectileSpawnRegistration
{
	internal static uint CaptureBaseId(ProjectileManager manager, bool isLocal, uint suppliedId)
	{
		if (isLocal && manager != null && GameBinder.TryGet<uint>(ProjectileBindings.NextId, manager, out uint nextId))
		{
			return nextId;
		}
		return suppliedId;
	}

	internal static bool RegisterCreated(ProjectileManager manager, Player owner, uint id, TargetSolution target)
	{
		if (!TryFind(manager, owner, id, out Projectile projectile))
		{
			return false;
		}
		return ProjectileTracker.Register(projectile, target);
	}

	internal static int RegisterCreatedRange(ProjectileManager manager, Player owner, uint baseId, int count, TargetSolution target)
	{
		int registered = 0;
		for (int i = 0; i < Math.Max(0, count); i++)
		{
			if (RegisterCreated(manager, owner, unchecked(baseId + (uint)i), target))
			{
				registered++;
			}
		}
		return registered;
	}

	internal static bool TryFind(ProjectileManager manager, Player owner, uint id, out Projectile projectile)
	{
		projectile = null;
		if (manager == null || owner == null || owner.Owner == null || ProjectileBindings.PlayerProjectiles == null)
		{
			return false;
		}
		try
		{
			if (ProjectileBindings.PlayerProjectiles.GetValue(manager) is not IDictionary players || !players.Contains(owner.Owner))
			{
				return false;
			}
			if (players[owner.Owner] is not IDictionary projectiles || !projectiles.Contains(id))
			{
				return false;
			}
			projectile = projectiles[id] as Projectile;
			return projectile != null;
		}
		catch
		{
			projectile = null;
			return false;
		}
	}
}
