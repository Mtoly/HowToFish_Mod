using System.Collections.Generic;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Targeting;

internal static class ProjectileTracker
{
	private static readonly object Sync = new object();

	private static readonly Dictionary<Projectile, TargetSolution> ProjectileToTarget = new Dictionary<Projectile, TargetSolution>();

	private static readonly Dictionary<uint, TargetSolution> IdToTarget = new Dictionary<uint, TargetSolution>();

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
					UnregisteredCount++;
				}
			}

			bool changed = !ProjectileToTarget.TryGetValue(projectile, out TargetSolution existing)
				|| !ReferenceEquals(existing, target);
			ProjectileToTarget[projectile] = target;
			IdToTarget[projectile.Id] = target;
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
		}
	}

	internal static bool IsValidTarget(TargetSolution target)
	{
		return target != null && Refs.IsAlive(target.Entity) && target.Transform != null;
	}

	private static bool RemoveLocked(Projectile projectile, bool invalid)
	{
		if (ReferenceEquals(projectile, null))
		{
			return false;
		}
		bool removed = ProjectileToTarget.TryGetValue(projectile, out TargetSolution removedTarget)
			&& ProjectileToTarget.Remove(projectile);
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
