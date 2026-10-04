using System;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI;

internal static class Widgets
{
	internal static void Section(string title, Action body)
	{
		GUILayout.BeginVertical(Theme.Section);
		if (!string.IsNullOrEmpty(title))
		{
			GUILayout.Label(title.ToUpperInvariant(), Theme.H2);
		}
		body();
		GUILayout.EndVertical();
		GUILayout.Space(6f);
	}

	internal static void Divider()
	{
		GUILayout.Box(GUIContent.none, Theme.Divider);
	}

	internal static void Note(string text)
	{
		GUILayout.Label(text, Theme.Muted);
	}

	internal static float SliderRow(string label, float value, float min, float max, string format = "0.##", float? resetTo = null)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, Theme.Body, GUILayout.Width(150f));
		float result = GUILayout.HorizontalSlider(value, min, max, Theme.SliderBar, Theme.SliderThumb);
		GUILayout.Label(result.ToString(format), Theme.Muted, GUILayout.Width(52f));
		if (resetTo.HasValue && GUILayout.Button("↺", Theme.Btn, GUILayout.Width(24f)))
		{
			result = resetTo.Value;
		}
		GUILayout.EndHorizontal();
		return result;
	}

	internal static int IntSliderRow(string label, int value, int min, int max)
	{
		return Mathf.RoundToInt(SliderRow(label, value, min, max, "0"));
	}

	internal static bool ToggleRow(string label, bool value, string hint = null)
	{
		GUILayout.BeginHorizontal();
		bool result = GUILayout.Toggle(value, "  " + label, Theme.Toggle);
		GUILayout.FlexibleSpace();
		if (!string.IsNullOrEmpty(hint))
		{
			GUILayout.Label(hint, Theme.Muted);
		}
		GUILayout.EndHorizontal();
		return result;
	}

	internal static string FieldRow(string label, string value, float labelWidth = 150f)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, Theme.Body, GUILayout.Width(labelWidth));
		string result = GUILayout.TextField(value ?? "", Theme.Field);
		GUILayout.EndHorizontal();
		return result;
	}

	internal static string SearchBox(string value, string placeholder = "Search…")
	{
		GUILayout.BeginHorizontal();
		string text = GUILayout.TextField(value ?? "", Theme.Field);
		if (string.IsNullOrEmpty(text))
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			GUI.Label(new Rect(lastRect.x + 8f, lastRect.y + 2f, lastRect.width, lastRect.height), placeholder, Theme.Muted);
		}
		if (GUILayout.Button("×", Theme.Btn, GUILayout.Width(26f)))
		{
			text = "";
		}
		GUILayout.EndHorizontal();
		return text;
	}

	internal static void ActionButton(string label, Func<bool> action, string okText, GUIStyle style = null, float width = 0f)
	{
		GUILayoutOption[] options = ((!(width > 0f)) ? new GUILayoutOption[0] : new GUILayoutOption[1] { GUILayout.Width(width) });
		if (GUILayout.Button(label, style ?? Theme.Btn, options))
		{
			bool ok;
			try
			{
				ok = action();
			}
			catch (Exception ex)
			{
				ok = false;
				Plugin.Log.LogError((object)ex);
			}
			Notifications.Result(ok, okText, label + " failed — see log");
		}
	}

	internal static bool RequireHost(string blockReason)
	{
		if (blockReason == null)
		{
			return true;
		}
		GUILayout.BeginVertical(Theme.Section);
		GUILayout.Label("Unavailable", Theme.H2);
		GUILayout.Label(blockReason, Theme.Muted);
		GUILayout.EndVertical();
		return false;
	}
}
