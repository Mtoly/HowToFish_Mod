using System.Collections.Generic;
using System.Linq;

namespace CompleteCheatMenu.Game;

internal static class BindingReport
{
	internal struct Entry
	{
		internal string Name;

		internal bool Ok;
	}

	private static readonly Dictionary<string, bool> _results = new Dictionary<string, bool>();

	internal static IEnumerable<Entry> All => from kv in _results
		orderby kv.Value, kv.Key
		select new Entry
		{
			Name = kv.Key,
			Ok = kv.Value
		};

	internal static int Total => _results.Count;

	internal static int Failed => _results.Count((KeyValuePair<string, bool> kv) => !kv.Value);

	internal static void Record(string name, bool ok)
	{
		if (!_results.TryGetValue(name, out var value) || value != ok)
		{
			_results[name] = ok;
			if (!ok)
			{
				Plugin.Log.LogWarning((object)("Binding failed: " + name));
			}
		}
	}
}
