using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class BallisticPredictor
{
	internal static Vector3 Predict(Vector3 origin, Vector3 currentPoint, Vector3 velocity, float projectileSpeed, bool compensateGravity = false)
	{
		if (!IsFinite(projectileSpeed) || projectileSpeed <= 0.0001f)
		{
			return currentPoint;
		}
		velocity = SanitizeVelocity(velocity, CheatState.AimMaxTargetSpeed);
		float flightTime = Vector3.Distance(origin, currentPoint) / projectileSpeed;
		for (int i = 0; i < 2; i++)
		{
			Vector3 estimate = currentPoint + velocity * flightTime;
			flightTime = Vector3.Distance(origin, estimate) / projectileSpeed;
		}
		Vector3 predicted = currentPoint + velocity * flightTime;
		if (compensateGravity && CheatState.AimGravityCompensation)
		{
			predicted -= Physics.gravity * (0.5f * flightTime * flightTime * Mathf.Max(0f, CheatState.AimGravityScale));
		}
		return IsFinite(predicted) ? predicted : currentPoint;
	}

	internal static Vector3 SanitizeVelocity(Vector3 velocity, float maxSpeed)
	{
		if (!IsFinite(velocity) || maxSpeed <= 0f || velocity.sqrMagnitude > maxSpeed * maxSpeed)
		{
			return Vector3.zero;
		}
		return velocity;
	}

	internal static Vector3 PredictProjectile(Vector3 origin, Vector3 currentPoint, Vector3 velocity, float projectileSpeed, float projectileGravity, float maxPredictionTime, bool compensateGravity)
	{
		if (!IsFinite(projectileSpeed) || projectileSpeed <= 0.0001f || !IsFinite(maxPredictionTime) || maxPredictionTime <= 0f)
		{
			return currentPoint;
		}
		velocity = SanitizeVelocity(velocity, CheatState.AimMaxTargetSpeed);
		float limit = Mathf.Max(0f, maxPredictionTime);
		float flightTime = Mathf.Clamp(Vector3.Distance(origin, currentPoint) / projectileSpeed, 0f, limit);
		for (int i = 0; i < 3; i++)
		{
			Vector3 estimate = currentPoint + velocity * flightTime;
			flightTime = Mathf.Clamp(Vector3.Distance(origin, estimate) / projectileSpeed, 0f, limit);
		}
		Vector3 predicted = currentPoint + velocity * flightTime;
		if (compensateGravity && projectileGravity > 0f)
		{
			predicted += Vector3.up * (0.5f * projectileGravity * flightTime * flightTime * Mathf.Max(0f, CheatState.AimGravityScale));
		}
		return IsFinite(predicted) ? predicted : currentPoint;
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
