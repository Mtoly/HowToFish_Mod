using System.Collections.Generic;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal readonly struct ProjectileLaunchData
{
	internal readonly Vector3 RawVelocity;
	internal readonly Vector3 LaunchVelocity;

	internal ProjectileLaunchData(Vector3 rawVelocity, Vector3 launchVelocity)
	{
		RawVelocity = rawVelocity;
		LaunchVelocity = launchVelocity;
	}
}

internal static class ProjectileTracker
{
	private static readonly object Sync = new object();

	private static readonly Dictionary<Projectile, TargetSolution> ProjectileToTarget = new Dictionary<Projectile, TargetSolution>();

	private static readonly Dictionary<uint, TargetSolution> IdToTarget = new Dictionary<uint, TargetSolution>();

	private static readonly Dictionary<Projectile, ProjectileLaunchData> ProjectileToLaunchData = new Dictionary<Projectile, ProjectileLaunchData>();

	private static readonly Dictionary<uint, ProjectileLaunchData> IdToLaunchData = new Dictionary<uint, ProjectileLaunchData>();

	internal static int ActiveCount
	{
		get
		{
			lock (Sync)
			{
				return ProjectileToTarget.Count;
			}
		}
	}

	internal static long RegisteredCount { get; private set; }

	internal static long UnregisteredCount { get; private set; }

	internal static long InvalidTargetCount { get; private set; }

	internal static long IdFallbackCount { get; private set; }

	internal static bool Register(Projectile projectile, TargetSolution target)
	{
		return Register(projectile, target, default, default);
	}

	internal static bool Register(Projectile projectile, TargetSolution target, Vector3 rawVelocity, Vector3 launchVelocity)
	{
		if (!ProjectileOwnership.IsLocalProjectile(projectile) || !IsValidTarget(target))
		{
			return false;
		}
		lock (Sync)
		{
			List<Projectile> idCollisions = null;
			foreach (Projectile tracked in ProjectileToTarget.Keys)
			{
				if (!ReferenceEquals(tracked, projectile) && tracked.Id == projectile.Id)
				{
					(idCollisions ??= new List<Projectile>()).Add(tracked);
				}
			}
			if (idCollisions != null)
			{
				for (int i = 0; i < idCollisions.Count; i++)
				{
					ProjectileToTarget.Remove(idCollisions[i]);
					ProjectileToLaunchData.Remove(idCollisions[i]);
					UnregisteredCount++;
				}
			}

			bool changed = !ProjectileToTarget.TryGetValue(projectile, out TargetSolution existing)
				|| !ReferenceEquals(existing, target);
			ProjectileToTarget[projectile] = target;
			IdToTarget[projectile.Id] = target;
			ProjectileLaunchData launchData = new ProjectileLaunchData(rawVelocity, launchVelocity);
			ProjectileToLaunchData[projectile] = launchData;
			IdToLaunchData[projectile.Id] = launchData;
			if (changed)
			{
				RegisteredCount++;
			}
		}
		return true;
	}

	internal static bool TryGetTarget(Projectile projectile, out TargetSolution target)
	{
		target = null;
		if (!ProjectileOwnership.IsLocalProjectile(projectile))
		{
			return false;
		}
		lock (Sync)
		{
			if (ProjectileToTarget.TryGetValue(projectile, out target))
			{
				if (IsValidTarget(target))
				{
					return true;
				}
				RemoveLocked(projectile, invalid: true);
				target = null;
				return false;
			}
			if (IdToTarget.TryGetValue(projectile.Id, out target))
			{
				if (IsValidTarget(target))
				{
					IdFallbackCount++;
					return true;
				}
				IdToTarget.Remove(projectile.Id);
				InvalidTargetCount++;
				target = null;
			}
		}
		return false;
	}

	internal static bool TryGetLaunchData(Projectile projectile, out ProjectileLaunchData launchData)
	{
		launchData = default;
		if (!ProjectileOwnership.IsLocalProjectile(projectile))
		{
			return false;
		}
		lock (Sync)
		{
			if (ProjectileToLaunchData.TryGetValue(projectile, out launchData))
			{
				return true;
			}
			return IdToLaunchData.TryGetValue(projectile.Id, out launchData);
		}
	}

	internal static bool Unregister(Projectile projectile)
	{
		if (projectile == null)
		{
			return false;
		}
		lock (Sync)
		{
			return RemoveLocked(projectile, invalid: false);
		}
	}

	internal static int PruneInvalid()
	{
		lock (Sync)
		{
			List<Projectile> invalid = null;
			foreach (KeyValuePair<Projectile, TargetSolution> pair in ProjectileToTarget)
			{
				if (!IsValidTarget(pair.Value))
				{
					(invalid ??= new List<Projectile>()).Add(pair.Key);
				}
			}
			if (invalid == null)
			{
				return 0;
			}
			for (int i = 0; i < invalid.Count; i++)
			{
				RemoveLocked(invalid[i], invalid: true);
			}
			return invalid.Count;
		}
	}

	internal static void Clear()
	{
		lock (Sync)
		{
			ProjectileToTarget.Clear();
			IdToTarget.Clear();
			ProjectileToLaunchData.Clear();
			IdToLaunchData.Clear();
		}
	}

	internal static bool IsValidTarget(TargetSolution target)
	{
		if (target == null || !Refs.IsAlive(target.Entity) || target.Transform == null)
		{
			return false;
		}
		try
		{
			if (target.Entity is Creature creature && creature.IsDead)
			{
				return false;
			}
			if (target.Entity is Item item && item.IsDestroying)
			{
				return false;
			}
			if (target.Entity is Behaviour behaviour && !behaviour.isActiveAndEnabled)
			{
				return false;
			}
			if (!target.Transform.gameObject.activeInHierarchy)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		return true;
	}

	private static bool RemoveLocked(Projectile projectile, bool invalid)
	{
		if (ReferenceEquals(projectile, null))
		{
			return false;
		}
		bool removed = ProjectileToTarget.TryGetValue(projectile, out TargetSolution removedTarget)
			&& ProjectileToTarget.Remove(projectile);
		ProjectileToLaunchData.Remove(projectile);
		IdToLaunchData.Remove(projectile.Id);
		if (removed && IdToTarget.TryGetValue(projectile.Id, out TargetSolution idTarget)
			&& ReferenceEquals(idTarget, removedTarget))
		{
			IdToTarget.Remove(projectile.Id);
		}
		if (removed)
		{
			UnregisteredCount++;
			if (invalid)
			{
				InvalidTargetCount++;
			}
		}
		return removed;
	}
}

