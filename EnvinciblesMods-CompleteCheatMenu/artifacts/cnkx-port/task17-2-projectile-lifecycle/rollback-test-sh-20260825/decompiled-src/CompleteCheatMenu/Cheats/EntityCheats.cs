using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class EntityCheats
{
	internal struct Group
	{
		internal SpawnCatalog.Category Kind;

		internal int Count;
	}

	internal static bool Available => GameBinder.Property("ItemManager", "Items") != null;

	internal static int TotalCount => LiveItems().Count;

	internal static List<KeyValuePair<SpawnCatalog.Category, object>> LiveItems()
	{
		List<KeyValuePair<SpawnCatalog.Category, object>> list = new List<KeyValuePair<SpawnCatalog.Category, object>>();
		foreach (object item in ItemCheats.WorldItems())
		{
			if (!IsHeldOrStored(item))
			{
				list.Add(new KeyValuePair<SpawnCatalog.Category, object>(CategoryOf(item), item));
			}
		}
		return list;
	}

	private static bool IsHeldOrStored(object item)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Item", "SyncedHolder");
		if (propertyInfo == null)
		{
			return false;
		}
		try
		{
			return Refs.IsAlive(propertyInfo.GetValue(item));
		}
		catch
		{
			return false;
		}
	}

	private static SpawnCatalog.Category CategoryOf(object item)
	{
		SpawnCatalog.Entry entry = SpawnCatalog.All.FirstOrDefault((SpawnCatalog.Entry e) => string.Equals(e.Id.ToString(), SkinCheats.ItemId(item).ToString(), StringComparison.Ordinal));
		if (entry.Display != null)
		{
			return entry.Kind;
		}
		return SpawnCatalog.Category.Other;
	}

	internal static List<Group> Summary()
	{
		Dictionary<SpawnCatalog.Category, int> dictionary = new Dictionary<SpawnCatalog.Category, int>();
		foreach (KeyValuePair<SpawnCatalog.Category, object> item in LiveItems())
		{
			dictionary.TryGetValue(item.Key, out var value);
			dictionary[item.Key] = value + 1;
		}
		return (from g in dictionary
			orderby g.Value descending
			select new Group
			{
				Kind = g.Key,
				Count = g.Value
			}).ToList();
	}

	private static bool Destroy(object item)
	{
		return GameBinder.TryInvoke(GameBinder.Method("Item", "DestroyItem", 2), item, (byte)0, byte.MaxValue);
	}

	internal static int DespawnAll()
	{
		int num = 0;
		foreach (KeyValuePair<SpawnCatalog.Category, object> item in LiveItems())
		{
			if (Destroy(item.Value))
			{
				num++;
			}
		}
		return num;
	}

	internal static int DespawnCategory(SpawnCatalog.Category kind)
	{
		int num = 0;
		foreach (KeyValuePair<SpawnCatalog.Category, object> item in LiveItems())
		{
			if (item.Key == kind && Destroy(item.Value))
			{
				num++;
			}
		}
		return num;
	}

	internal static int DespawnNearby(float radius)
	{
		object localPlayer = Refs.LocalPlayer;
		if (localPlayer == null)
		{
			return 0;
		}
		Vector3 vector = TeleportCheats.PlayerPosition(localPlayer);
		float num = radius * radius;
		int num2 = 0;
		foreach (KeyValuePair<SpawnCatalog.Category, object> item in LiveItems())
		{
			Transform transform = (item.Value as Component)?.transform;
			if (!(transform == null) && !((transform.position - vector).sqrMagnitude > num) && Destroy(item.Value))
			{
				num2++;
			}
		}
		return num2;
	}

	internal static int DespawnCreatures()
	{
		int num = 0;
		foreach (KeyValuePair<SpawnCatalog.Category, object> item in LiveItems())
		{
			if ((item.Key == SpawnCatalog.Category.Creature || item.Key == SpawnCatalog.Category.Fish || item.Key == SpawnCatalog.Category.Bird || item.Key == SpawnCatalog.Category.Boss) && Destroy(item.Value))
			{
				num++;
			}
		}
		return num;
	}
}
