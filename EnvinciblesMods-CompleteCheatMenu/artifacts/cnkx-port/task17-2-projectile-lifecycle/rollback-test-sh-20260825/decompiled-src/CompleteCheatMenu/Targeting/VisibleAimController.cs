using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class VisibleAimController
{
	private static FieldInfo _holdingFireInput;
	private static FieldInfo _cameraRotation;
	private static FieldInfo _recoilCurrent;
	private static FieldInfo _recoilMultiplier;
	private static bool _resolved;

	internal static bool Step(object weapon, object playerCamera, Camera camera, TargetSolution target, float deltaTime)
	{
		if (!CheatState.AimVisibleAssist || !CheatState.Aimbot || weapon == null || playerCamera == null || camera == null || target == null || target.Transform == null)
		{
			return false;
		}
		Resolve();
		if (!IsFiring(weapon) || _cameraRotation == null || deltaTime <= 0f)
		{
			return false;
		}
		Vector3 point = target.AimPoint;
		float drop = Mathf.Max(0f, target.Distance) * Mathf.Max(0f, CheatState.AimDistanceDropPerUnit);
		point -= Vector3.up * drop;
		Vector3 direction = point - camera.transform.position;
		if (direction.sqrMagnitude <= 0.0001f)
		{
			return false;
		}
		Vector3 currentAngles;
		try
		{
			currentAngles = (Vector3)_cameraRotation.GetValue(playerCamera);
		}
		catch
		{
			return false;
		}
		Quaternion rootRotation = Quaternion.identity;
		if (Refs.LocalPlayer is Component playerComponent && playerComponent != null)
		{
			rootRotation = playerComponent.transform.rotation;
		}
		Quaternion desiredWorld = Quaternion.LookRotation(direction.normalized, Vector3.up);
		Quaternion desiredLocal = Quaternion.Inverse(rootRotation) * desiredWorld;
		Vector3 desiredAngles = NormalizeEuler(desiredLocal.eulerAngles);
		ApplyRecoilCompensation(playerCamera, ref desiredAngles);
		desiredLocal = Quaternion.Euler(desiredAngles);
		Quaternion currentLocal = Quaternion.Euler(currentAngles);
		float response = Mathf.Max(0f, CheatState.AimVisibleResponse);
		float alpha = 1f - Mathf.Exp(-response * deltaTime);
		Quaternion smoothed = Quaternion.Slerp(currentLocal, desiredLocal, Mathf.Clamp01(alpha));
		Vector3 result = NormalizeEuler(smoothed.eulerAngles);
		result.x = Mathf.Clamp(result.x, -90f, 90f);
		result.z = 0f;
		try
		{
			_cameraRotation.SetValue(playerCamera, result);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void Resolve()
	{
		if (_resolved)
		{
			return;
		}
		_resolved = true;
		_holdingFireInput = GameBinder.Field("Weapon", "_holdingFireInput");
		_cameraRotation = GameBinder.Field("PlayerCamera", "_rot");
		_recoilCurrent = GameBinder.Field("PlayerCamera", "_recoilCur");
		_recoilMultiplier = GameBinder.Field("PlayerCamera", "_screenShakeRotMulti");
	}

	private static bool IsFiring(object weapon)
	{
		return GameBinder.TryGet<bool>(_holdingFireInput, weapon, out var firing) && firing;
	}

	private static void ApplyRecoilCompensation(object playerCamera, ref Vector3 desiredAngles)
	{
		if (!CheatState.AimRecoilCompensation || CheatState.ZeroRecoil || _recoilCurrent == null)
		{
			return;
		}
		try
		{
			Vector2 recoil = (Vector2)_recoilCurrent.GetValue(playerCamera);
			float cameraMultiplier = 1f;
			if (_recoilMultiplier != null)
			{
				cameraMultiplier = (float)_recoilMultiplier.GetValue(playerCamera);
			}
			float strength = Mathf.Max(0f, CheatState.AimRecoilCompensationStrength) * cameraMultiplier;
			desiredAngles.x += recoil.y * strength;
			desiredAngles.y -= recoil.x * strength;
		}
		catch
		{
		}
	}

	private static Vector3 NormalizeEuler(Vector3 value)
	{
		value.x = NormalizeAngle(value.x);
		value.y = NormalizeAngle(value.y);
		value.z = NormalizeAngle(value.z);
		return value;
	}

	private static float NormalizeAngle(float value)
	{
		return Mathf.Repeat(value + 180f, 360f) - 180f;
	}
}
