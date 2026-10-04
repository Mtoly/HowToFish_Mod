using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.UI;
using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal static class EspRenderer
{
	private static Texture2D _pixel;

	private static GUIStyle _label;

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

	internal static void Draw()
	{
		if (!CheatState.EspEnabled)
		{
			return;
		}
		Camera currentCamera = Refs.CurrentCamera;
		if (currentCamera == null)
		{
			return;
		}
		if (_label == null)
		{
			_label = new GUIStyle(GUI.skin.label)
			{
				fontSize = 11,
				alignment = TextAnchor.MiddleCenter
			};
		}
		float num = CheatState.EspDistance * CheatState.EspDistance;
		Vector3 position = currentCamera.transform.position;
		foreach (VisualCheats.EspTarget target in VisualCheats.Targets)
		{
			if (target.Transform == null)
			{
				continue;
			}
			Vector3 position2 = target.Transform.position;
			if ((position2 - position).sqrMagnitude > num)
			{
				continue;
			}
			Vector3 vector = currentCamera.WorldToScreenPoint(position2);
			if (!(vector.z <= 0f))
			{
				float x = vector.x;
				float num2 = (float)Screen.height - vector.y;
				Color color = ColorFor(target.Kind);
				float z = vector.z;
				float num3 = Mathf.Clamp(600f / Mathf.Max(z, 1f), 10f, 90f);
				DrawBox(new Rect(x - num3 / 2f, num2 - num3 / 2f, num3, num3), color);
				if (CheatState.EspLabels)
				{
					_label.normal.textColor = color;
					string text = (CheatState.EspDistanceLabels ? $"{target.Label}  {z:0}m" : target.Label);
					GUI.Label(new Rect(x - 100f, num2 - num3 / 2f - 18f, 200f, 16f), text, _label);
				}
				if (CheatState.EspTracers)
				{
					DrawLine(new Vector2((float)Screen.width / 2f, Screen.height), new Vector2(x, num2), color, 1f);
				}
			}
		}
	}

	private static Color ColorFor(VisualCheats.EspKind kind)
	{
		return kind switch
		{
			VisualCheats.EspKind.Creature => Theme.Accent, 
			VisualCheats.EspKind.Player => Theme.Danger, 
			_ => new Color(0.85f, 0.78f, 0.35f), 
		};
	}

	private static void DrawBox(Rect r, Color color, float thickness = 1.5f)
	{
		Color color2 = GUI.color;
		GUI.color = color;
		GUI.DrawTexture(new Rect(r.x, r.y, r.width, thickness), Pixel);
		GUI.DrawTexture(new Rect(r.x, r.yMax - thickness, r.width, thickness), Pixel);
		GUI.DrawTexture(new Rect(r.x, r.y, thickness, r.height), Pixel);
		GUI.DrawTexture(new Rect(r.xMax - thickness, r.y, thickness, r.height), Pixel);
		GUI.color = color2;
	}

	private static void DrawLine(Vector2 from, Vector2 to, Color color, float width)
	{
		Color color2 = GUI.color;
		Matrix4x4 matrix = GUI.matrix;
		Vector2 vector = to - from;
		float magnitude = vector.magnitude;
		if (!(magnitude < 0.01f))
		{
			float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			GUI.color = color;
			GUIUtility.RotateAroundPivot(angle, from);
			GUI.DrawTexture(new Rect(from.x, from.y - width / 2f, magnitude, width), Pixel);
			GUI.matrix = matrix;
			GUI.color = color2;
		}
	}
}
