using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class ProjectileGuidanceMath
{
	internal static bool TrySteer(Vector3 origin, Vector3 currentVelocity, Vector3 targetPoint, Vector3 targetVelocity, float projectileGravity, bool usePrediction, bool compensateGravity, float maxPredictionTime, float turnSpeedDegrees, float deltaTime, out Vector3 steeredVelocity)
	{
		steeredVelocity = currentVelocity;
		if (!IsFinite(origin) || !IsFinite(currentVelocity) || !IsFinite(targetPoint) || !IsFinite(targetVelocity))
		{
			return false;
		}
		float speed = currentVelocity.magnitude;
		if (!IsFinite(speed) || speed <= 0.0001f)
		{
			return false;
		}
		Vector3 point = targetPoint;
		if (usePrediction)
		{
			point = BallisticPredictor.PredictProjectile(origin, point, targetVelocity, speed, projectileGravity, maxPredictionTime, compensateGravity);
		}
		Vector3 direction = point - origin;
		if (!IsFinite(direction) || direction.sqrMagnitude <= 0.0001f)
		{
			return false;
		}
		Vector3 desired = direction.normalized * speed;
		Vector3 steered = desired;
		if (turnSpeedDegrees > 0f)
		{
			float maxRadiansDelta = Mathf.Max(0f, turnSpeedDegrees) * Mathf.Deg2Rad * Mathf.Max(0f, deltaTime);
			steered = Vector3.RotateTowards(currentVelocity, desired, maxRadiansDelta, 0f);
			if (steered.sqrMagnitude > 0.0001f)
			{
				steered = steered.normalized * speed;
			}
		}
		if (!IsFinite(steered))
		{
			return false;
		}
		steeredVelocity = steered;
		return true;
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
