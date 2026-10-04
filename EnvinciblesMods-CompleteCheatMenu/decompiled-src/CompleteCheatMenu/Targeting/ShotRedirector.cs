using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class ShotRedirector
{
	private static float _nextDiagnostic;

	internal static bool TryRedirect(TargetingSystem targetingSystem, Camera camera, Transform firePoint, float projectileSpeed, out TargetSolution solution)
	{
		solution = null;
		if (targetingSystem == null || camera == null || firePoint == null || !CheatState.Aimbot)
		{
			return false;
		}
		if (!ShouldTrack(CheatState.AimTrackingChance, Random.value))
		{
			LogLimited("本次开火未触发方向追踪。");
			return false;
		}
		solution = targetingSystem.Acquire(camera);
		if (solution == null || solution.Transform == null)
		{
			LogLimited("开火时没有可用追踪目标。");
			return false;
		}
		Vector3 point = solution.AimPoint;
		if (CheatState.AimPrediction)
		{
			point = BallisticPredictor.Predict(firePoint.position, point, solution.Velocity, projectileSpeed, CheatState.AimGravityCompensation);
		}
		Vector3 direction = point - firePoint.position;
		if (direction.sqrMagnitude <= 0.0001f)
		{
			return false;
		}
		// Magic Bullet style: AddProjectile prefixes redirect the actual velocity.
		// This method only selects and caches the target; it never rotates the FirePoint.
		LogLimited("已缓存当前目标，弹丸将在生成前重定向速度。");
		return true;
	}

	internal static bool ShouldTrack(float percent, float sample)
	{
		float probability = Mathf.Clamp01(percent / 100f);
		if (probability <= 0f)
		{
			return false;
		}
		if (probability >= 1f)
		{
			return true;
		}
		return sample < probability;
	}

	private static void LogLimited(string message)
	{
		if (!CheatState.AimTrackingDiagnostics || Time.unscaledTime < _nextDiagnostic)
		{
			return;
		}
		_nextDiagnostic = Time.unscaledTime + 2f;
		Plugin.Log.LogInfo((object)("[方向追踪] " + message));
	}
}
