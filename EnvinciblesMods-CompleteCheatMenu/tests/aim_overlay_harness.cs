using System;
using CompleteCheatMenu.Targeting;

namespace UnityEngine
{
	internal static class Mathf
	{
		internal const float Deg2Rad = (float)(Math.PI / 180.0);
		internal static float Sqrt(float value) => (float)Math.Sqrt(value);
		internal static float Tan(float value) => (float)Math.Tan(value);
		internal static float Min(float a, float b) => Math.Min(a, b);
		internal static float Max(float a, float b) => Math.Max(a, b);
		internal static float Clamp(float value, float min, float max) => Math.Min(max, Math.Max(min, value));
	}
}

internal static class AimOverlayHarness
{
	private static int _cases;

	private static void Near(float expected, float actual, float tolerance, string name)
	{
		_cases++;
		if (Math.Abs(expected - actual) > tolerance)
		{
			throw new Exception(name + ": expected " + expected + ", got " + actual);
		}
	}

	private static void Equal(float expected, float actual, string name)
	{
		Near(expected, actual, 0.0001f, name);
	}

	public static int Main()
	{
		Near(540f, AimOverlayMath.AngleToPixelRadius(60f, 1920, 1080, 30f), 0.01f, "thirty degrees");
		Near(164.925f, AimOverlayMath.AngleToPixelRadius(60f, 1920, 1080, 10f), 0.01f, "ten degrees");
		Near(1101.454f, AimOverlayMath.AngleToPixelRadius(60f, 1920, 1080, 90f), 0.01f, "hemisphere cap");
		Equal(0f, AimOverlayMath.AngleToPixelRadius(0f, 1920, 1080, 30f), "invalid camera fov");
		Equal(0f, AimOverlayMath.AngleToPixelRadius(60f, 0, 1080, 30f), "invalid width");
		Equal(0f, AimOverlayMath.AngleToPixelRadius(60f, 1920, 1080, 0f), "invalid aim angle");
		Equal(250f, AimOverlayMath.EffectiveRadius(250f, 540f), "screen radius wins");
		Equal(164.925f, AimOverlayMath.EffectiveRadius(250f, 164.925f), "angle radius wins");
		Equal(0f, AimOverlayMath.EffectiveRadius(-1f, 100f), "negative screen radius");
		Equal(0f, AimOverlayMath.EffectiveRadius(100f, float.NaN), "non-finite angle radius");
		Console.WriteLine("AIM_OVERLAY_HARNESS=PASS cases=" + _cases);
		return 0;
	}
}
