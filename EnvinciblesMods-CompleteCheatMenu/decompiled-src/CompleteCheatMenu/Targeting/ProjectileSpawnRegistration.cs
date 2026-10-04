using System;
using System.Collections;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal struct ProjectileSpawnState
{
	internal bool Active;

	internal Player Owner;

	internal uint BaseId;

	internal int Count;

	internal TargetSolution Target;

	internal Vector3 RawVelocity;

	internal Vector3 LaunchVelocity;

	internal Vector3[] RawVelocities;

	internal Vector3[] LaunchVelocities;
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
		return RegisterCreated(manager, owner, id, target, default, default);
	}

	internal static bool RegisterCreated(ProjectileManager manager, Player owner, uint id, TargetSolution target, Vector3 rawVelocity, Vector3 launchVelocity)
	{
		if (!TryFind(manager, owner, id, out Projectile projectile))
		{
			return false;
		}
		return ProjectileTracker.Register(projectile, target, rawVelocity, launchVelocity);
	}

	internal static int RegisterCreatedRange(ProjectileManager manager, Player owner, uint baseId, int count, TargetSolution target)
	{
		return RegisterCreatedRange(manager, owner, baseId, count, target, null, null);
	}

	internal static int RegisterCreatedRange(ProjectileManager manager, Player owner, uint baseId, int count, TargetSolution target, Vector3[] rawVelocities, Vector3[] launchVelocities)
	{
		int registered = 0;
		for (int i = 0; i < Math.Max(0, count); i++)
		{
			Vector3 rawVelocity = rawVelocities != null && i < rawVelocities.Length ? rawVelocities[i] : default;
			Vector3 launchVelocity = launchVelocities != null && i < launchVelocities.Length ? launchVelocities[i] : default;
			if (RegisterCreated(manager, owner, unchecked(baseId + (uint)i), target, rawVelocity, launchVelocity))
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
