using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Cheats;

internal static class FishingCheats
{
	internal struct BaitEntry
	{
		internal byte Index;

		internal string Name;

		internal int Cost;
	}

	internal static bool Available => Refs.LocalInventory != null;

	internal static byte CurrentBait
	{
		get
		{
			object localInventory = Refs.LocalInventory;
			if (localInventory == null)
			{
				return 0;
			}
			PropertyInfo propertyInfo = GameBinder.Property("PlayerInventory", "CurBait");
			if (propertyInfo == null)
			{
				return 0;
			}
			try
			{
				return (byte)propertyInfo.GetValue(localInventory);
			}
			catch
			{
				return 0;
			}
		}
	}

	internal static List<BaitEntry> AllBaits()
	{
		List<BaitEntry> list = new List<BaitEntry>();
		PropertyInfo propertyInfo = GameBinder.Property("GameInfo", "AllBaits");
		if (propertyInfo == null)
		{
			return list;
		}
		try
		{
			if (!(propertyInfo.GetValue(null) is IEnumerable enumerable))
			{
				return list;
			}
			byte b = 0;
			foreach (object item in enumerable)
			{
				list.Add(new BaitEntry
				{
					Index = b,
					Name = BaitName(item, b),
					Cost = BaitCost(item)
				});
				b++;
			}
		}
		catch
		{
		}
		return list;
	}

	private static string BaitName(object bait, byte index)
	{
		PropertyInfo propertyInfo = GameBinder.Property("BaitInfo", "NameLocalized");
		if (propertyInfo != null)
		{
			try
			{
				string text = propertyInfo.GetValue(bait) as string;
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
			catch
			{
			}
		}
		return $"Bait {index}";
	}

	private static int BaitCost(object bait)
	{
		PropertyInfo propertyInfo = GameBinder.Property("BaitInfo", "Cost");
		if (propertyInfo == null)
		{
			return 0;
		}
		try
		{
			return (int)propertyInfo.GetValue(bait);
		}
		catch
		{
			return 0;
		}
	}

	internal static bool GrantBait(byte index)
	{
		object localInventory = Refs.LocalInventory;
		if (localInventory == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("PlayerInventory", "ServerBoughtBait", 1), localInventory, index);
	}

	internal static bool GrantAllBaits()
	{
		List<BaitEntry> list = AllBaits();
		if (list.Count == 0)
		{
			return false;
		}
		bool result = true;
		foreach (BaitEntry item in list)
		{
			if (!GrantBait(item.Index))
			{
				result = false;
			}
		}
		return result;
	}

	internal static bool SelectBait(byte index)
	{
		object localInventory = Refs.LocalInventory;
		if (localInventory == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("PlayerInventory", "ServerSetCurBait", 1), localInventory, index);
	}
}
