using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class InitialVelocityRedirector
{
	internal static bool TryRedirect(Vector3 origin, Vector3 originalVelocity, TargetSolution target, bool usePrediction, bool compensateGravity, out Vector3 redirectedVelocity)
	{
		redirectedVelocity = originalVelocity;
		if (target == null || !IsFinite(origin) || !IsFinite(originalVelocity))
		{
			return false;
		}
		float speed = originalVelocity.magnitude;
		if (!IsFinite(speed) || speed <= 0.0001f)
		{
			return false;
		}
		Vector3 point = target.AimPoint;
		if (usePrediction)
		{
			point = BallisticPredictor.Predict(origin, point, target.Velocity, speed, compensateGravity);
		}
		if (!IsFinite(point))
		{
			return false;
		}
		Vector3 direction = point - origin;
		if (!IsFinite(direction) || direction.sqrMagnitude <= 0.0001f)
		{
			return false;
		}
		Vector3 redirected = direction.normalized * speed;
		if (!IsFinite(redirected))
		{
			return false;
		}
		redirectedVelocity = redirected;
		return true;
	}

	internal static bool TryRedirectAll(Vector3 origin, Vector3[] velocities, TargetSolution target, bool usePrediction, bool compensateGravity, out Vector3[] redirectedVelocities)
	{
		redirectedVelocities = velocities;
		if (velocities == null || velocities.Length == 0 || target == null)
		{
			return false;
		}
		Vector3[] copy = (Vector3[])velocities.Clone();
		bool changed = false;
		for (int i = 0; i < copy.Length; i++)
		{
			if (TryRedirect(origin, copy[i], target, usePrediction, compensateGravity, out Vector3 redirected))
			{
				copy[i] = redirected;
				changed = true;
			}
		}
		if (changed)
		{
			redirectedVelocities = copy;
		}
		return changed;
	}

	private static bool IsFinite(Vector3 value)
	{
		return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
	}

	private static bool IsFinite(float value)
	{
		return !float.IsNaN(value) && !float.IsInfinity(value);
	}
}
