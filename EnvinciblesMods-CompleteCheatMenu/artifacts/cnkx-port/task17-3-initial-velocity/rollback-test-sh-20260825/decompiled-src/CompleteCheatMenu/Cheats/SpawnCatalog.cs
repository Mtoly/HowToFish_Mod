using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class SpawnCatalog
{
	internal enum Category
	{
		Other,
		Boss,
		Creature,
		Fish,
		Bird,
		Weapon,
		Melee,
		Explosive,
		Tool
	}

	internal enum SortMode
	{
		Name,
		Category,
		Id
	}

	internal struct Entry
	{
		internal string Key;

		internal string Display;

		internal byte Id;

		internal Category Kind;

		internal bool IsCreature
		{
			get
			{
				if (Kind != Category.Creature && Kind != Category.Boss && Kind != Category.Fish)
				{
					return Kind == Category.Bird;
				}
				return true;
			}
		}
	}

	private static List<Entry> _entries;

	internal static bool Loaded
	{
		get
		{
			if (_entries != null)
			{
				return _entries.Count > 0;
			}
			return false;
		}
	}

	internal static int Count => _entries?.Count ?? 0;

	internal static IReadOnlyList<Entry> All
	{
		get
		{
			if (_entries == null)
			{
				Load();
			}
			IReadOnlyList<Entry> entries = _entries;
			return entries ?? Array.Empty<Entry>();
		}
	}

	internal static void Load()
	{
		_entries = new List<Entry>();
		try
		{
			FieldInfo fieldInfo = GameBinder.Field("GameInfo", "_nameToSpawnable");
			if (fieldInfo == null || !(fieldInfo.GetValue(null) is IDictionary dictionary))
			{
				return;
			}
			foreach (DictionaryEntry item in dictionary)
			{
				string text = item.Key as string;
				if (!string.IsNullOrEmpty(text) && item.Value != null)
				{
					_entries.Add(new Entry
					{
						Key = text,
						Display = PrettyName(item.Value, text),
						Id = SkinCheats.ItemId(item.Value),
						Kind = Classify(item.Value)
					});
				}
			}
			_entries = _entries.OrderBy((Entry e) => e.Display, StringComparer.OrdinalIgnoreCase).ToList();
			Plugin.Log.LogInfo((object)$"Spawn catalogue loaded: {_entries.Count} entries.");
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Spawn catalogue failed to load: " + ex.Message));
		}
	}

	private static Category Classify(object item)
	{
		if (HasComponent(item, "Bird"))
		{
			return Category.Bird;
		}
		if (HasComponent(item, "Explosive"))
		{
			return Category.Explosive;
		}
		if (HasComponent(item, "Weapon"))
		{
			return Category.Weapon;
		}
		if (HasComponent(item, "Melee"))
		{
			return Category.Melee;
		}
		object component = GetComponent(item, "Creature");
		if (component != null)
		{
			if (IsBoss(component))
			{
				return Category.Boss;
			}
			if (!HasComponent(item, "Fish"))
			{
				return Category.Creature;
			}
			return Category.Fish;
		}
		if (HasComponent(item, "Fish"))
		{
			return Category.Fish;
		}
		if (HasComponent(item, "Tool"))
		{
			return Category.Tool;
		}
		return Category.Other;
	}

	private static bool IsBoss(object creature)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Creature", "BossType");
		if (propertyInfo == null)
		{
			return false;
		}
		try
		{
			string text = propertyInfo.GetValue(creature)?.ToString();
			return !string.IsNullOrEmpty(text) && text != "None";
		}
		catch
		{
			return false;
		}
	}

	private static object GetComponent(object item, string propName)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Item", propName);
		if (propertyInfo == null)
		{
			return null;
		}
		try
		{
			object value = propertyInfo.GetValue(item);
			return Refs.IsAlive(value) ? value : null;
		}
		catch
		{
			return null;
		}
	}

	private static bool HasComponent(object item, string propName)
	{
		return GetComponent(item, propName) != null;
	}

	private static string PrettyName(object item, string fallback)
	{
		try
		{
			UnityEngine.Object obj = item as UnityEngine.Object;
			if (obj != null && !string.IsNullOrEmpty(obj.name))
			{
				return obj.name;
			}
		}
		catch
		{
		}
		return fallback;
	}

	internal static IEnumerable<Entry> Search(string filter, Category? only, SortMode sort)
	{
		IEnumerable<Entry> source = All.AsEnumerable();
		if (only.HasValue)
		{
			source = source.Where((Entry e) => e.Kind == only.Value);
		}
		if (!string.IsNullOrEmpty(filter))
		{
			string f = filter.Trim().ToLowerInvariant();
			source = source.Where((Entry e) => e.Display.ToLowerInvariant().Contains(f) || e.Key.Contains(f) || e.Kind.ToString().ToLowerInvariant().Contains(f));
		}
		return sort switch
		{
			SortMode.Category => source.OrderBy((Entry e) => e.Kind.ToString(), StringComparer.OrdinalIgnoreCase).ThenBy((Entry e) => e.Display, StringComparer.OrdinalIgnoreCase), 
			SortMode.Id => source.OrderBy((Entry e) => e.Id).ThenBy((Entry e) => e.Display, StringComparer.OrdinalIgnoreCase), 
			_ => source.OrderBy((Entry e) => e.Display, StringComparer.OrdinalIgnoreCase), 
		};
	}

	internal static Dictionary<Category, int> Counts()
	{
		Dictionary<Category, int> dictionary = new Dictionary<Category, int>();
		foreach (Entry item in All)
		{
			dictionary.TryGetValue(item.Kind, out var value);
			dictionary[item.Kind] = value + 1;
		}
		return dictionary;
	}
}
