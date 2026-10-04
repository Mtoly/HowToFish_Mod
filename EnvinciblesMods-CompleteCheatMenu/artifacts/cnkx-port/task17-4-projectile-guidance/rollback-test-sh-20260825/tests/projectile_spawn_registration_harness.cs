using System;
using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Targeting;

internal sealed class Player
{
	internal object Owner;
}

internal sealed class Projectile
{
	internal uint Id;
}

internal sealed class ProjectileManager
{
	internal uint _nextId;
	internal Dictionary<object, Dictionary<uint, Projectile>> _playerProjectiles = new Dictionary<object, Dictionary<uint, Projectile>>();
}

namespace CompleteCheatMenu.Game
{
	internal static class GameBinder
	{
		internal static bool TryGet<T>(FieldInfo field, object instance, out T value)
		{
			value = field != null ? (T)field.GetValue(instance) : default;
			return field != null;
		}
	}
}

namespace CompleteCheatMenu.Targeting
{
	internal sealed class TargetSolution { }

	internal static class ProjectileBindings
	{
		internal static FieldInfo NextId => typeof(ProjectileManager).GetField("_nextId", BindingFlags.Instance | BindingFlags.NonPublic);
		internal static FieldInfo PlayerProjectiles => typeof(ProjectileManager).GetField("_playerProjectiles", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	internal static class ProjectileTracker
	{
		internal static readonly List<Projectile> Registered = new List<Projectile>();

		internal static bool Register(Projectile projectile, TargetSolution target)
		{
			if (projectile == null || target == null)
			{
				return false;
			}
			Registered.Add(projectile);
			return true;
		}
	}
}

internal static class ProjectileSpawnRegistrationHarness
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

	public static int Main()
	{
		object connection = new object();
		Player owner = new Player { Owner = connection };
		ProjectileManager manager = new ProjectileManager { _nextId = 100 };
		manager._playerProjectiles[connection] = new Dictionary<uint, Projectile>
		{
			[100] = new Projectile { Id = 100 },
			[101] = new Projectile { Id = 101 },
			[102] = new Projectile { Id = 102 }
		};
		TargetSolution target = new TargetSolution();

		Check(ProjectileSpawnRegistration.CaptureBaseId(manager, true, 7) == 100, "local base id captured from manager");
		Check(ProjectileSpawnRegistration.CaptureBaseId(manager, false, 7) == 7, "remote supplied id retained");
		Check(ProjectileSpawnRegistration.TryFind(manager, owner, 100, out Projectile found) && found.Id == 100, "nested dictionary lookup");
		Check(!ProjectileSpawnRegistration.TryFind(manager, new Player { Owner = new object() }, 100, out _), "wrong owner rejected");
		Check(!ProjectileSpawnRegistration.TryFind(manager, owner, 999, out _), "missing id rejected");
		Check(ProjectileSpawnRegistration.RegisterCreated(manager, owner, 100, target), "single registration");
		Check(ProjectileTracker.Registered.Count == 1 && ProjectileTracker.Registered[0].Id == 100, "single registered exact projectile");
		Check(ProjectileSpawnRegistration.RegisterCreatedRange(manager, owner, 100, 3, target) == 3, "range registration count");
		Check(ProjectileTracker.Registered.Count == 4, "range registered all projectiles");
		Check(ProjectileSpawnRegistration.RegisterCreatedRange(manager, owner, 101, 5, target) == 2, "range skips missing ids");

		Console.WriteLine($"PROJECTILE_SPAWN_REGISTRATION_HARNESS=PASS cases={_cases}");
		return 0;
	}
}
