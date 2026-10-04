using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class AimOverlayMath
{
	internal static float AngleToPixelRadius(float verticalFov, int screenWidth, int screenHeight, float aimAngle)
	{
		if (screenWidth <= 0 || screenHeight <= 0 || !IsFinite(verticalFov) || !IsFinite(aimAngle) || verticalFov <= 0f || aimAngle <= 0f)
		{
			return 0f;
		}
		float halfWidth = screenWidth * 0.5f;
		float halfHeight = screenHeight * 0.5f;
		float viewportRadius = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
		if (aimAngle >= 89.9f)
		{
			return viewportRadius;
		}
		float safeVerticalFov = Mathf.Clamp(verticalFov, 0.01f, 179.8f);
		float focalLength = halfHeight / Mathf.Tan(safeVerticalFov * 0.5f * Mathf.Deg2Rad);
		float angleRadius = focalLength * Mathf.Tan(Mathf.Clamp(aimAngle, 0f, 89.9f) * Mathf.Deg2Rad);
		if (!IsFinite(angleRadius))
		{
			return viewportRadius;
		}
		return Mathf.Min(viewportRadius, Mathf.Max(0f, angleRadius));
	}

	internal static float EffectiveRadius(float screenRadius, float angleRadius)
	{
		if (!IsFinite(screenRadius) || !IsFinite(angleRadius) || screenRadius <= 0f || angleRadius <= 0f)
		{
			return 0f;
		}
		return Mathf.Min(screenRadius, angleRadius);
	}

	private static bool IsFinite(float value)
	{
		return !float.IsNaN(value) && !float.IsInfinity(value);
	}
}
