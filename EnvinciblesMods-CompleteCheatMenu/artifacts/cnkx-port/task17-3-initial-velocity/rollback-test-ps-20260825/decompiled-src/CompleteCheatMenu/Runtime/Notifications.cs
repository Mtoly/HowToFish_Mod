using System.Collections.Generic;
using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal static class Notifications
{
	internal struct Toast
	{
		internal string Text;

		internal bool Ok;

		internal float Until;
	}

	private const int MaxVisible = 5;

	private static readonly List<Toast> _toasts = new List<Toast>();

	internal static IEnumerable<Toast> Active
	{
		get
		{
			_toasts.RemoveAll((Toast t) => Time.unscaledTime > t.Until);
			return _toasts;
		}
	}

	internal static void Show(string text, bool ok = true, float seconds = 3f)
	{
		_toasts.Add(new Toast
		{
			Text = text,
			Ok = ok,
			Until = Time.unscaledTime + seconds
		});
		if (_toasts.Count > 5)
		{
			_toasts.RemoveAt(0);
		}
		if (!ok)
		{
			Plugin.Log.LogWarning((object)text);
		}
	}

	internal static void Result(bool ok, string okText, string failText = null)
	{
		Show(ok ? okText : (failText ?? (okText + " failed")), ok);
	}
}
