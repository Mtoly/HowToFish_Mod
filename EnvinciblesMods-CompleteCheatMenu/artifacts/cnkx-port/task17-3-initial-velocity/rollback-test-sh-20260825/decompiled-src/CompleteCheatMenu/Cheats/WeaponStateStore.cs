using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CompleteCheatMenu.Cheats;

internal sealed class WeaponStateStore
{
	private sealed class ReferenceComparer : IEqualityComparer<object>
	{
		internal static readonly ReferenceComparer Instance = new ReferenceComparer();

		public new bool Equals(object x, object y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private readonly Dictionary<object, Dictionary<FieldInfo, object>> _originals = new Dictionary<object, Dictionary<FieldInfo, object>>(ReferenceComparer.Instance);

	internal bool HasOriginals(object weapon)
	{
		return weapon != null && _originals.TryGetValue(weapon, out var fields) && fields.Count > 0;
	}

	internal bool Set(object weapon, FieldInfo field, object value)
	{
		if (weapon == null || field == null || field.IsStatic || !field.DeclaringType.IsInstanceOfType(weapon))
		{
			return false;
		}
		if (!_originals.TryGetValue(weapon, out var fields))
		{
			fields = new Dictionary<FieldInfo, object>();
			_originals[weapon] = fields;
		}
		try
		{
			if (!fields.ContainsKey(field))
			{
				fields[field] = field.GetValue(weapon);
			}
			object converted = ConvertValue(value, field.FieldType);
			field.SetValue(weapon, converted);
			return true;
		}
		catch
		{
			if (fields.Count == 0)
			{
				_originals.Remove(weapon);
			}
			return false;
		}
	}

	internal bool RestoreWeapon(object weapon)
	{
		if (weapon == null || !_originals.TryGetValue(weapon, out var fields))
		{
			return false;
		}
		foreach (KeyValuePair<FieldInfo, object> pair in fields)
		{
			TryRestore(weapon, pair.Key, pair.Value);
		}
		_originals.Remove(weapon);
		return true;
	}

	internal int RestoreFieldAll(FieldInfo field)
	{
		if (field == null)
		{
			return 0;
		}
		int restored = 0;
		List<object> empty = null;
		foreach (KeyValuePair<object, Dictionary<FieldInfo, object>> weaponEntry in _originals)
		{
			if (weaponEntry.Value.TryGetValue(field, out var original))
			{
				TryRestore(weaponEntry.Key, field, original);
				weaponEntry.Value.Remove(field);
				restored++;
			}
			if (weaponEntry.Value.Count == 0)
			{
				if (empty == null)
				{
					empty = new List<object>();
				}
				empty.Add(weaponEntry.Key);
			}
		}
		if (empty != null)
		{
			for (int i = 0; i < empty.Count; i++)
			{
				_originals.Remove(empty[i]);
			}
		}
		return restored;
	}

	internal int RestoreAll()
	{
		int restoredWeapons = _originals.Count;
		foreach (KeyValuePair<object, Dictionary<FieldInfo, object>> weaponEntry in _originals)
		{
			foreach (KeyValuePair<FieldInfo, object> fieldEntry in weaponEntry.Value)
			{
				TryRestore(weaponEntry.Key, fieldEntry.Key, fieldEntry.Value);
			}
		}
		_originals.Clear();
		return restoredWeapons;
	}

	private static void TryRestore(object weapon, FieldInfo field, object value)
	{
		try
		{
			field.SetValue(weapon, value);
		}
		catch
		{
		}
	}

	private static object ConvertValue(object value, Type targetType)
	{
		if (value == null || targetType.IsInstanceOfType(value))
		{
			return value;
		}
		return Convert.ChangeType(value, targetType);
	}
}
