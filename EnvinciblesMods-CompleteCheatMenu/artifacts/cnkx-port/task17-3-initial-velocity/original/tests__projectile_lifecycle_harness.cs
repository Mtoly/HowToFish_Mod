using System;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Targeting;
using UnityEngine;

namespace UnityEngine
{
	internal sealed class Transform { }
}

internal sealed class NetworkConnection
{
	internal bool IsLocalClient;
}

internal sealed class Player
{
	internal NetworkConnection Owner;
}

internal sealed class Weapon
{
	internal Player Holder;
}

internal sealed class Projectile
{
	internal uint Id;
	internal Player Owner;
	internal bool IsLocal;
	internal bool FromNpc;
}

namespace CompleteCheatMenu.Game
{
	internal static class Refs
	{
		internal static object LocalPlayer;

		internal static bool IsAlive(object value) => value != null;
	}
}

namespace CompleteCheatMenu.Targeting
{
	internal sealed class TargetSolution
	{
		internal object Entity;
		internal Transform Transform;
	}
}

internal static class ProjectileLifecycleHarness
{
	private static int _cases;

	private static void Check(bool condition, string name)
	{
		_cases++;
		if (!condition)
		{
			throw new InvalidOperationException(name);
		}
	}

	private static TargetSolution Target()
	{
		return new TargetSolution
		{
			Entity = new object(),
			Transform = new Transform()
		};
	}

	public static int Main()
	{
		Player local = new Player { Owner = new NetworkConnection { IsLocalClient = true } };
		Player remote = new Player { Owner = new NetworkConnection { IsLocalClient = false } };
		Refs.LocalPlayer = local;

		Weapon localWeapon = new Weapon { Holder = local };
		Weapon remoteWeapon = new Weapon { Holder = remote };
		Check(ProjectileOwnership.IsLocalWeapon(localWeapon), "local weapon accepted");
		Check(!ProjectileOwnership.IsLocalWeapon(remoteWeapon), "remote weapon rejected");

		Projectile localProjectile = new Projectile { Id = 41, Owner = local, IsLocal = true };
		Projectile remoteProjectile = new Projectile { Id = 42, Owner = remote, IsLocal = true };
		Projectile npcProjectile = new Projectile { Id = 43, Owner = local, IsLocal = true, FromNpc = true };
		Check(ProjectileOwnership.IsLocalProjectile(localProjectile), "local projectile accepted");
		Check(!ProjectileOwnership.IsLocalProjectile(remoteProjectile), "remote projectile rejected");
		Check(!ProjectileOwnership.IsLocalProjectile(npcProjectile), "npc projectile rejected");

		TargetSolution target = Target();
		Check(MagicShotContext.Begin(localWeapon, target), "context begins");
		Check(MagicShotContext.IsActive && ReferenceEquals(MagicShotContext.ActiveTarget, target), "context exposes target");
		MagicShotContext.End();
		Check(!MagicShotContext.IsActive && MagicShotContext.ActiveTarget == null, "context ends");

		ProjectileTracker.Clear();
		Check(ProjectileTracker.Register(localProjectile, target), "register local projectile");
		Check(ProjectileTracker.ActiveCount == 1, "active count");
		Check(ProjectileTracker.TryGetTarget(localProjectile, out TargetSolution direct) && ReferenceEquals(direct, target), "direct lookup");
		Projectile copied = new Projectile { Id = 41, Owner = local, IsLocal = true };
		Check(ProjectileTracker.TryGetTarget(copied, out TargetSolution fallback) && ReferenceEquals(fallback, target), "id fallback lookup");
		Check(ProjectileTracker.IdFallbackCount == 1, "id fallback counter");
		Projectile remoteCollision = new Projectile { Id = 41, Owner = remote, IsLocal = true };
		Check(!ProjectileTracker.TryGetTarget(remoteCollision, out _), "remote id collision rejected");
		Check(!ProjectileTracker.Register(remoteProjectile, target), "tracker rejects remote projectile");
		long registrations = ProjectileTracker.RegisteredCount;
		Check(ProjectileTracker.Register(localProjectile, target), "duplicate registration accepted");
		Check(ProjectileTracker.RegisteredCount == registrations, "duplicate registration not recounted");

		Projectile replacement = new Projectile { Id = 41, Owner = local, IsLocal = true };
		TargetSolution replacementTarget = Target();
		Check(ProjectileTracker.Register(replacement, replacementTarget), "same id replacement registered");
		Check(ProjectileTracker.ActiveCount == 1, "same id replacement removes stale object mapping");
		Check(ProjectileTracker.TryGetTarget(localProjectile, out TargetSolution staleFallback) && ReferenceEquals(staleFallback, replacementTarget), "stale object uses current id mapping only");
		Check(ProjectileTracker.TryGetTarget(replacement, out TargetSolution replaced) && ReferenceEquals(replaced, replacementTarget), "replacement target available");
		Check(!ProjectileTracker.Unregister(localProjectile), "stale object unregister ignored");
		Check(ProjectileTracker.TryGetTarget(replacement, out replaced) && ReferenceEquals(replaced, replacementTarget), "stale unregister preserves replacement id mapping");

		replacementTarget.Entity = null;
		Check(!ProjectileTracker.TryGetTarget(replacement, out _), "invalid target removed");
		Check(ProjectileTracker.ActiveCount == 0, "invalid cleanup active count");
		Check(ProjectileTracker.InvalidTargetCount >= 1, "invalid counter");

		TargetSolution target2 = Target();
		Projectile projectile2 = new Projectile { Id = 44, Owner = local, IsLocal = true };
		Check(ProjectileTracker.Register(projectile2, target2), "second register");
		Check(ProjectileTracker.Unregister(projectile2), "explicit unregister");
		Check(ProjectileTracker.ActiveCount == 0, "unregister active count");
		ProjectileTracker.Clear();

		Console.WriteLine($"PROJECTILE_LIFECYCLE_HARNESS=PASS cases={_cases}");
		return 0;
	}
}
