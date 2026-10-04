using System;
using CompleteCheatMenu.Targeting;
using UnityEngine;

namespace UnityEngine
{
	internal struct Vector3
	{
		internal float x;
		internal float y;
		internal float z;

		internal Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		internal static Vector3 zero => new Vector3(0f, 0f, 0f);

		internal float sqrMagnitude => x * x + y * y + z * z;

		internal float magnitude => (float)Math.Sqrt(sqrMagnitude);

		internal Vector3 normalized
		{
			get
			{
				float length = magnitude;
				return length <= 0.000001f ? zero : new Vector3(x / length, y / length, z / length);
			}
		}

		public static Vector3 operator +(Vector3 left, Vector3 right) => new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);

		public static Vector3 operator -(Vector3 left, Vector3 right) => new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);

		public static Vector3 operator *(Vector3 value, float scale) => new Vector3(value.x * scale, value.y * scale, value.z * scale);

		public static bool operator ==(Vector3 left, Vector3 right) => left.x == right.x && left.y == right.y && left.z == right.z;

		public static bool operator !=(Vector3 left, Vector3 right) => !(left == right);

		public override bool Equals(object value) => value is Vector3 other && this == other;

		public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() * 397) ^ z.GetHashCode();
	}
}

namespace CompleteCheatMenu.Targeting
{
	internal sealed class TargetSolution
	{
		internal Vector3 AimPoint;
		internal Vector3 Velocity;
	}

	internal static class BallisticPredictor
	{
		internal static int Calls;

		internal static Vector3 Predict(Vector3 origin, Vector3 currentPoint, Vector3 velocity, float projectileSpeed, bool compensateGravity)
		{
			Calls++;
			return currentPoint + velocity;
		}

		internal static Vector3 PredictProjectile(Vector3 origin, Vector3 currentPoint, Vector3 velocity, float projectileSpeed, float projectileGravity, float maxPredictionTime, bool compensateGravity)
		{
			Calls++;
			return currentPoint + velocity;
		}
	}
}

namespace CompleteCheatMenu.Runtime
{
	internal static class CheatState
	{
		internal static float AimMaxPredictionTime = 2f;
	}
}

internal static class ProjectileInitialVelocityHarness
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

	private static bool Close(float left, float right)
	{
		return Math.Abs(left - right) <= 0.0001f;
	}

	public static int Main()
	{
		TargetSolution target = new TargetSolution { AimPoint = new Vector3(0f, 10f, 0f) };
		Vector3 source = new Vector3(3f, 4f, 0f);
		Check(InitialVelocityRedirector.TryRedirect(Vector3.zero, source, target, false, false, out Vector3 redirected), "single redirect succeeds");
		Check(Close(redirected.magnitude, 5f), "single speed preserved");
		Check(Close(redirected.x, 0f) && Close(redirected.y, 5f) && Close(redirected.z, 0f), "single direction replaced");

		Check(!InitialVelocityRedirector.TryRedirect(Vector3.zero, Vector3.zero, target, false, false, out Vector3 zero), "zero speed rejected");
		Check(zero == Vector3.zero, "zero speed unchanged");
		Check(!InitialVelocityRedirector.TryRedirect(Vector3.zero, source, null, false, false, out Vector3 noTarget), "null target rejected");
		Check(noTarget == source, "null target unchanged");

		TargetSolution coincident = new TargetSolution { AimPoint = Vector3.zero };
		Check(!InitialVelocityRedirector.TryRedirect(Vector3.zero, source, coincident, false, false, out Vector3 samePoint), "coincident point rejected");
		Check(samePoint == source, "coincident point unchanged");

		TargetSolution moving = new TargetSolution { AimPoint = new Vector3(0f, 10f, 0f), Velocity = new Vector3(10f, 0f, 0f) };
		Check(InitialVelocityRedirector.TryRedirect(Vector3.zero, source, moving, true, true, out Vector3 predicted), "prediction redirect succeeds");
		Check(BallisticPredictor.Calls == 1, "prediction seam called once");
		Check(predicted.x > 0f && predicted.y > 0f && Close(predicted.magnitude, 5f), "predicted direction and speed");

		Vector3[] velocities = { new Vector3(10f, 0f, 0f), new Vector3(0f, 0f, 20f) };
		Check(InitialVelocityRedirector.TryRedirectAll(Vector3.zero, velocities, target, false, false, out Vector3[] redirectedAll), "multi redirect succeeds");
		Check(!ReferenceEquals(velocities, redirectedAll), "multi array cloned");
		Check(velocities[0] == new Vector3(10f, 0f, 0f) && velocities[1] == new Vector3(0f, 0f, 20f), "caller array unchanged");
		Check(Close(redirectedAll[0].magnitude, 10f) && Close(redirectedAll[1].magnitude, 20f), "multi speeds preserved");
		Check(redirectedAll[0].y > 0f && redirectedAll[1].y > 0f, "multi directions replaced");

		Vector3[] invalid = { Vector3.zero, new Vector3(float.NaN, 0f, 0f) };
		Check(!InitialVelocityRedirector.TryRedirectAll(Vector3.zero, invalid, target, false, false, out Vector3[] invalidResult), "all invalid rejected");
		Check(ReferenceEquals(invalid, invalidResult), "all invalid array unchanged");

		Console.WriteLine($"PROJECTILE_INITIAL_VELOCITY_HARNESS=PASS cases={_cases}");
		return 0;
	}
}
