using System;
using System.Collections.Generic;
using CompleteCheatMenu.Targeting;
using UnityEngine;

namespace UnityEngine
{
	internal static class Mathf
	{
		internal const float Deg2Rad = (float)(Math.PI / 180.0);
		internal static float Max(float a, float b) => Math.Max(a, b);
	}

	internal struct Vector3
	{
		internal float x;
		internal float y;
		internal float z;
		internal Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
		internal static Vector3 zero => new Vector3();
		internal float sqrMagnitude => x * x + y * y + z * z;
		internal float magnitude => (float)Math.Sqrt(sqrMagnitude);
		internal Vector3 normalized => magnitude <= 0.000001f ? zero : this * (1f / magnitude);
		internal static Vector3 RotateTowards(Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta)
		{
			float cl = current.magnitude, tl = target.magnitude;
			if (cl <= 0.000001f || tl <= 0.000001f) return target;
			Vector3 a = current * (1f / cl), b = target * (1f / tl);
			float dot = Math.Max(-1f, Math.Min(1f, a.x * b.x + a.y * b.y + a.z * b.z));
			float angle = (float)Math.Acos(dot);
			if (angle <= maxRadiansDelta) return b * cl;
			float t = maxRadiansDelta / angle;
			return (a * (1f - t) + b * t).normalized * cl;
		}
		public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x * b, a.y * b, a.z * b);
	}

	internal sealed class Rigidbody
	{
		internal Vector3 Velocity;
		internal int Calls;
		internal Vector3 GetPointVelocity(Vector3 point) { Calls++; return Velocity; }
	}

	internal sealed class Transform
	{
		internal Vector3 position;
		internal Rigidbody Rig;
		internal T GetComponentInParent<T>() where T : class => Rig as T;
	}
}

internal sealed class Projectile
{
	internal Vector3 Position;
	internal Vector3 Velocity;
	internal float GravityForce;
}

namespace CompleteCheatMenu.Runtime
{
	internal static class CheatState
	{
		internal static bool Aimbot = true;
		internal static float AimProjectileTrackingTime = 0.05f;
		internal static float AimProjectileUpdateInterval = 0.02f;
		internal static float AimProjectileTurnSpeed;
		internal static bool AimPrediction;
		internal static bool AimGravityCompensation;
		internal static float AimMaxPredictionTime = 2f;
		internal static float AimMaxTargetSpeed = 250f;
	}
}

namespace CompleteCheatMenu.Targeting
{
	internal sealed class TargetSolution
	{
		internal object Entity = new object();
		internal Transform Transform;
		internal Vector3 AimPoint;
	}

	internal static class ProjectileTracker
	{
		internal static readonly Dictionary<Projectile, TargetSolution> Targets = new Dictionary<Projectile, TargetSolution>();
		internal static int UnregisterCalls;
		internal static bool TryGetTarget(Projectile projectile, out TargetSolution target) => Targets.TryGetValue(projectile, out target);
		internal static bool Unregister(Projectile projectile) { UnregisterCalls++; return Targets.Remove(projectile); }
	}

	internal sealed class VelocityTracker
	{
		internal static int ObserveCalls;
		internal Vector3 Observe(int instanceId, Vector3 position, float now) { ObserveCalls++; return new Vector3(0f, 1f, 0f); }
		internal void Clear() { }
	}

	internal static class BallisticPredictor
	{
		internal static Vector3 SanitizeVelocity(Vector3 velocity, float maxSpeed) => velocity;
		internal static Vector3 PredictProjectile(Vector3 origin, Vector3 point, Vector3 velocity, float speed, float gravity, float maxTime, bool compensate) => point + velocity;
	}
}

internal static class ProjectileGuidanceLifecycleHarness
{
	private static int _cases;
	private static void Check(bool value, string name) { _cases++; if (!value) throw new InvalidOperationException(name); }
	private static bool Close(float a, float b) => Math.Abs(a - b) <= 0.001f;

	public static int Main()
	{
		Rigidbody rig = new Rigidbody { Velocity = new Vector3(2f, 0f, 0f) };
		TargetSolution target = new TargetSolution { Transform = new Transform { position = new Vector3(0f, 10f, 0f), Rig = rig }, AimPoint = new Vector3(0f, 11f, 0f) };
		Projectile projectile = new Projectile { Position = Vector3.zero, Velocity = new Vector3(10f, 0f, 0f), GravityForce = 9f };
		ProjectileTracker.Targets[projectile] = target;

		Check(ProjectileGuidance.Step(projectile, 1f, 0.02f), "first guidance update");
		Check(ProjectileGuidance.ActiveCount == 1, "state created");
		Check(rig.Calls == 1, "rigidbody velocity preferred");
		Check(Close(projectile.Velocity.magnitude, 10f) && projectile.Velocity.y > 0f, "velocity redirected with speed preserved");
		Vector3 afterFirst = projectile.Velocity;
		Check(!ProjectileGuidance.Step(projectile, 1.01f, 0.02f), "interval skip");
		Check(ProjectileGuidance.IntervalSkipCount == 1, "interval skip counted");
		Check(Close(projectile.Velocity.x, afterFirst.x) && Close(projectile.Velocity.y, afterFirst.y), "interval skip keeps velocity");
		Check(ProjectileGuidance.Step(projectile, 1.03f, 0.02f), "second scheduled update");

		Check(!ProjectileGuidance.Step(projectile, 1.07f, 0.02f), "tracking expires");
		Check(ProjectileTracker.UnregisterCalls == 1 && !ProjectileTracker.Targets.ContainsKey(projectile), "expiry unregisters tracker");
		Check(ProjectileGuidance.ActiveCount == 0 && ProjectileGuidance.ExpiredCount == 1, "expiry forgets state");

		Projectile fallbackProjectile = new Projectile { Position = Vector3.zero, Velocity = new Vector3(5f, 0f, 0f) };
		TargetSolution fallbackTarget = new TargetSolution { Transform = new Transform { position = new Vector3(0f, 5f, 0f) }, AimPoint = new Vector3(0f, 5f, 0f) };
		ProjectileTracker.Targets[fallbackProjectile] = fallbackTarget;
		Check(ProjectileGuidance.Step(fallbackProjectile, 2f, 0.02f), "fallback velocity update");
		Check(VelocityTracker.ObserveCalls == 1, "velocity tracker fallback used");

		CompleteCheatMenu.Runtime.CheatState.Aimbot = false;
		Vector3 disabledVelocity = fallbackProjectile.Velocity;
		Check(!ProjectileGuidance.Step(fallbackProjectile, 2.03f, 0.02f), "disabled aimbot skips guidance");
		Check(Close(disabledVelocity.x, fallbackProjectile.Velocity.x) && Close(disabledVelocity.y, fallbackProjectile.Velocity.y), "disabled aimbot keeps velocity");
		Check(ProjectileGuidance.ActiveCount == 0, "disabled aimbot forgets guidance state");
		ProjectileGuidance.Clear();
		Check(ProjectileGuidance.ActiveCount == 0, "clear removes states");

		Console.WriteLine($"PROJECTILE_GUIDANCE_LIFECYCLE_HARNESS=PASS cases={_cases}");
		return 0;
	}
}
