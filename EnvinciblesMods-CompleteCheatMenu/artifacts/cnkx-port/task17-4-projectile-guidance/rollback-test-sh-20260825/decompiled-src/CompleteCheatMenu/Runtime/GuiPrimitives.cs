using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal static class GuiPrimitives
{
	private static Texture2D _pixel;

	private static Texture2D Pixel
	{
		get
		{
			if (_pixel != null)
			{
				return _pixel;
			}
			_pixel = new Texture2D(1, 1)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			_pixel.SetPixel(0, 0, Color.white);
			_pixel.Apply();
			return _pixel;
		}
	}

	internal static void DrawBox(Rect rect, Color color, float thickness = 1.5f)
	{
		Color previousColor = GUI.color;
		Matrix4x4 previousMatrix = GUI.matrix;
		try
		{
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Pixel);
			GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Pixel);
			GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Pixel);
			GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Pixel);
		}
		finally
		{
			GUI.matrix = previousMatrix;
			GUI.color = previousColor;
		}
	}

	internal static void DrawLine(Vector2 from, Vector2 to, Color color, float width = 1f)
	{
		Color previousColor = GUI.color;
		Matrix4x4 previousMatrix = GUI.matrix;
		try
		{
			Vector2 delta = to - from;
			float length = delta.magnitude;
			if (length < 0.01f)
			{
				return;
			}
			GUI.color = color;
			GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, from);
			GUI.DrawTexture(new Rect(from.x, from.y - width * 0.5f, length, width), Pixel);
		}
		finally
		{
			GUI.matrix = previousMatrix;
			GUI.color = previousColor;
		}
	}

	internal static void DrawCircle(Vector2 center, float radius, Color color, float width = 1f, int segments = 72)
	{
		Color previousColor = GUI.color;
		Matrix4x4 previousMatrix = GUI.matrix;
		try
		{
			if (radius <= 0.5f || segments < 3)
			{
				return;
			}
			float step = Mathf.PI * 2f / segments;
			Vector2 previous = center + new Vector2(radius, 0f);
			for (int i = 1; i <= segments; i++)
			{
				float angle = step * i;
				Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
				DrawLine(previous, next, color, width);
				previous = next;
			}
		}
		finally
		{
			GUI.matrix = previousMatrix;
			GUI.color = previousColor;
		}
	}

	internal static void DrawBar(Rect rect, float fraction, Color fill, Color background)
	{
		Color previousColor = GUI.color;
		Matrix4x4 previousMatrix = GUI.matrix;
		try
		{
			float clamped = Mathf.Clamp01(fraction);
			GUI.color = background;
			GUI.DrawTexture(rect, Pixel);
			GUI.color = fill;
			float height = rect.height * clamped;
			GUI.DrawTexture(new Rect(rect.x + 1f, rect.yMax - height, Mathf.Max(1f, rect.width - 2f), height), Pixel);
		}
		finally
		{
			GUI.matrix = previousMatrix;
			GUI.color = previousColor;
		}
	}

	internal static void DrawLabel(Rect rect, string text, GUIStyle style, Color color)
	{
		Color previousColor = GUI.color;
		Matrix4x4 previousMatrix = GUI.matrix;
		Color previousText = style.normal.textColor;
		try
		{
			GUI.color = Color.white;
			style.normal.textColor = color;
			GUI.Label(rect, text, style);
		}
		finally
		{
			style.normal.textColor = previousText;
			GUI.matrix = previousMatrix;
			GUI.color = previousColor;
		}
	}

	internal static void Dispose()
	{
		if (_pixel != null)
		{
			Object.Destroy(_pixel);
			_pixel = null;
		}
	}
}
