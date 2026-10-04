using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class ItemCheats
{
	internal enum Target
	{
		Held,
		Inventory,
		World
	}

	internal static object HeldItem
	{
		get
		{
			object localHolding = Refs.LocalHolding;
			if (localHolding == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = GameBinder.Property("PlayerHolding", "HeldItem");
			if (propertyInfo == null)
			{
				return null;
			}
			try
			{
				object value = propertyInfo.GetValue(localHolding);
				return Refs.IsAlive(value) ? value : null;
			}
			catch
			{
				return null;
			}
		}
	}

	internal static string ItemName(object item)
	{
		return (item as UnityEngine.Object)?.name ?? "item";
	}

	internal static List<object> WorldItems()
	{
		List<object> list = new List<object>();
		PropertyInfo propertyInfo = GameBinder.Property("ItemManager", "Items");
		if (propertyInfo == null)
		{
			return list;
		}
		try
		{
			if (propertyInfo.GetValue(null) is IDictionary dictionary)
			{
				foreach (DictionaryEntry item in dictionary)
				{
					if (Refs.IsAlive(item.Value))
					{
						list.Add(item.Value);
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	internal static List<object> Resolve(Target target)
	{
		switch (target)
		{
		case Target.Held:
		{
			object heldItem = HeldItem;
			if (heldItem != null)
			{
				return new List<object> { heldItem };
			}
			return new List<object>();
		}
		case Target.World:
			return WorldItems();
		default:
			return new List<object>();
		}
	}

	internal static int Worth(object item)
	{
		if (!GameBinder.TryGet<int>(GameBinder.Field("Item", "_worth"), item, out var value))
		{
			return 0;
		}
		return value;
	}

	internal static int TotalWorth(object item)
	{
		return ReadIntProp(item, "TotalWorth");
	}

	internal static float Cookness(object item)
	{
		return ReadFloatProp(item, "Cookness");
	}

	internal static float Weight(object item)
	{
		return ReadFloatProp(item, "RandomizedWeight");
	}

	internal static float KillScoreMulti(object item)
	{
		return ReadFloatProp(item, "KillScoreMultiplier");
	}

	internal static float BettingMulti(object item)
	{
		return ReadFloatProp(item, "BettingMultiplier");
	}

	private static int ReadIntProp(object item, string name)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Item", name);
		if (propertyInfo == null || item == null)
		{
			return 0;
		}
		try
		{
			return (int)propertyInfo.GetValue(item);
		}
		catch
		{
			return 0;
		}
	}

	private static float ReadFloatProp(object item, string name)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Item", name);
		if (propertyInfo == null || item == null)
		{
			return 0f;
		}
		try
		{
			return (float)propertyInfo.GetValue(item);
		}
		catch
		{
			return 0f;
		}
	}

	internal static bool SetWorth(object item, int worth)
	{
		return GameBinder.TrySet(GameBinder.Field("Item", "_worth"), item, worth);
	}

	internal static bool SetWeight(object item, float weight)
	{
		return GameBinder.TryWriteSyncVar(GameBinder.Field("Item", "_syncedRandomWeight"), item, weight);
	}

	internal static bool SetCookness(object item, float cookness)
	{
		return GameBinder.TryWriteSyncVar(GameBinder.Field("Item", "_cookness"), item, cookness);
	}

	internal static bool SetBettingMulti(object item, float multi)
	{
		return GameBinder.TryWriteSyncVar(GameBinder.Field("Item", "_bettingMultiplier"), item, multi);
	}

	internal static bool SetKillScoreMulti(object item, float multi)
	{
		MethodInfo methodInfo = GameBinder.Method("Item", "SetKillscoreMultiplier", 1);
		if (methodInfo != null && GameBinder.TryInvoke(methodInfo, item, multi))
		{
			return true;
		}
		return GameBinder.TryWriteSyncVar(GameBinder.Field("Item", "_killScoreMultiplier"), item, multi);
	}

	internal static bool SetSkin(object item, byte skinIndex)
	{
		return GameBinder.TryInvoke(GameBinder.Method("Server", "SetItemSkin", 2), Refs.Server, item, skinIndex);
	}

	internal static int ApplyToAll(Target target, Func<object, bool> action)
	{
		int num = 0;
		foreach (object item in Resolve(target))
		{
			if (action(item))
			{
				num++;
			}
		}
		return num;
	}
}
