using System;
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

		internal Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		internal static Vector3 zero => new Vector3();
		internal float sqrMagnitude => x * x + y * y + z * z;
		internal float magnitude => (float)Math.Sqrt(sqrMagnitude);
		internal Vector3 normalized => magnitude <= 0.000001f ? zero : this * (1f / magnitude);

		internal static Vector3 RotateTowards(Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta)
		{
			float currentLength = current.magnitude;
			float targetLength = target.magnitude;
			if (currentLength <= 0.000001f || targetLength <= 0.000001f)
			{
				return target;
			}
			Vector3 a = current * (1f / currentLength);
			Vector3 b = target * (1f / targetLength);
			float dot = Math.Max(-1f, Math.Min(1f, a.x * b.x + a.y * b.y + a.z * b.z));
			float angle = (float)Math.Acos(dot);
			if (angle <= maxRadiansDelta || maxRadiansDelta == float.PositiveInfinity)
			{
				return b * currentLength;
			}
			float t = maxRadiansDelta / angle;
			Vector3 blended = (a * (1f - t) + b * t).normalized;
			return blended * currentLength;
		}

		public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x * b, a.y * b, a.z * b);
	}
}

namespace CompleteCheatMenu.Targeting
{
	internal static class BallisticPredictor
	{
		internal static int Calls;
		internal static float LastGravity;
		internal static float LastMaxTime;

		internal static Vector3 PredictProjectile(Vector3 origin, Vector3 point, Vector3 velocity, float speed, float gravity, float maxPredictionTime, bool compensateGravity)
		{
			Calls++;
			LastGravity = gravity;
			LastMaxTime = maxPredictionTime;
			return point + velocity;
		}
	}
}

internal static class ProjectileGuidanceHarness
{
	private static int _cases;

	private static void Check(bool condition, string name)
	{
		_cases++;
		if (!condition) throw new InvalidOperationException(name);
	}

	private static bool Close(float left, float right, float tolerance = 0.001f) => Math.Abs(left - right) <= tolerance;

	public static int Main()
	{
		Vector3 current = new Vector3(10f, 0f, 0f);
		Vector3 target = new Vector3(0f, 100f, 0f);
		Check(ProjectileGuidanceMath.TrySteer(Vector3.zero, current, target, Vector3.zero, 0f, false, false, 2f, 0f, 0.02f, out Vector3 snap), "unlimited steering succeeds");
		Check(Close(snap.magnitude, 10f), "unlimited speed preserved");
		Check(Close(snap.x, 0f) && snap.y > 9.99f, "unlimited steering reaches target direction");

		Check(ProjectileGuidanceMath.TrySteer(Vector3.zero, current, target, Vector3.zero, 0f, false, false, 2f, 90f, 0.5f, out Vector3 limited), "limited steering succeeds");
		Check(Close(limited.magnitude, 10f), "limited speed preserved");
		Check(limited.x > 0f && limited.y > 0f, "limited steering remains between directions");
		float angle = (float)(Math.Atan2(limited.y, limited.x) * 180.0 / Math.PI);
		Check(angle > 40f && angle < 50f, "limited steering near 45 degrees");

		Vector3 targetVelocity = new Vector3(5f, 0f, 0f);
		Check(ProjectileGuidanceMath.TrySteer(Vector3.zero, current, target, targetVelocity, 12.5f, true, true, 1.75f, 0f, 0.02f, out Vector3 predicted), "prediction steering succeeds");
		Check(BallisticPredictor.Calls == 1, "prediction called once");
		Check(Close(BallisticPredictor.LastGravity, 12.5f), "real projectile gravity forwarded");
		Check(Close(BallisticPredictor.LastMaxTime, 1.75f), "prediction time forwarded");
		Check(predicted.x > 0f && predicted.y > 0f && Close(predicted.magnitude, 10f), "predicted direction valid");

		Check(!ProjectileGuidanceMath.TrySteer(Vector3.zero, Vector3.zero, target, Vector3.zero, 0f, false, false, 2f, 720f, 0.02f, out Vector3 zero), "zero velocity rejected");
		Check(Close(zero.magnitude, 0f), "zero velocity unchanged");
		Check(!ProjectileGuidanceMath.TrySteer(Vector3.zero, new Vector3(float.NaN, 0f, 0f), target, Vector3.zero, 0f, false, false, 2f, 720f, 0.02f, out _), "nan velocity rejected");

		Console.WriteLine($"PROJECTILE_GUIDANCE_HARNESS=PASS cases={_cases}");
		return 0;
	}
}
