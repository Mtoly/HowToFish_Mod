using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Cheats;

internal static class SkinCheats
{
	internal struct SkinEntry
	{
		internal byte Index;

		internal string Name;

		internal string Rarity;
	}

	internal static readonly string[] CharacterUnlocks = new string[14]
	{
		"UnlockLighthouseKeeper", "UnlockSwampMan", "UnlockSwampLady", "UnlockKioskLady", "UnlockTourist", "UnlockGrillmaster", "UnlockAndrei", "UnlockJacob", "UnlockGunStoreClerc", "UnlockScaredGuyInShorts",
		"UnlockStoreGradma", "UnlockMilitary", "UnlockScientist", "UnlockBean"
	};

	internal static byte BoatSkin
	{
		get
		{
			object boat = BoatCheats.Boat;
			if (boat == null)
			{
				return 0;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Boat", "CurSkin");
			if (propertyInfo == null)
			{
				return 0;
			}
			try
			{
				return (byte)propertyInfo.GetValue(boat);
			}
			catch
			{
				return 0;
			}
		}
	}

	internal static List<object> SkinnableItems()
	{
		List<object> list = new List<object>();
		PropertyInfo propertyInfo = GameBinder.Property("GameInfo", "ItemWithSkinsforCommands");
		if (propertyInfo == null)
		{
			return list;
		}
		try
		{
			if (propertyInfo.GetValue(null) is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					if (Refs.IsAlive(item))
					{
						list.Add(item);
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	internal static bool UnlockAll()
	{
		List<object> list = SkinnableItems();
		if (list.Count == 0)
		{
			return false;
		}
		MethodInfo methodInfo = GameBinder.Method("SaveManager", "UnlockSkin", 2);
		if (methodInfo == null)
		{
			return false;
		}
		bool result = true;
		foreach (object item in list)
		{
			byte b = ItemId(item);
			foreach (SkinEntry item2 in SkinsFor(item))
			{
				if (!GameBinder.TryInvoke(methodInfo, null, b, item2.Index))
				{
					result = false;
				}
			}
		}
		return result;
	}

	internal static bool LockAll()
	{
		return GameBinder.TryInvoke(GameBinder.Method("SaveManager", "LockAllSkins", 0), null);
	}

	internal static bool UnlockSkin(object item, byte skinIndex)
	{
		return GameBinder.TryInvoke(GameBinder.Method("SaveManager", "UnlockSkin", 2), null, ItemId(item), skinIndex);
	}

	internal static byte ItemId(object item)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Item", "ID");
		if (propertyInfo == null || item == null)
		{
			return 0;
		}
		try
		{
			return (byte)propertyInfo.GetValue(item);
		}
		catch
		{
			return 0;
		}
	}

	internal static List<SkinEntry> SkinsFor(object item)
	{
		List<SkinEntry> list = new List<SkinEntry>();
		if (item == null)
		{
			return list;
		}
		try
		{
			object obj = GameBinder.Property("Item", "SkinPreset")?.GetValue(item);
			if (!Refs.IsAlive(obj))
			{
				return list;
			}
			if (!(GameBinder.Property("SkinPreset", "Skins")?.GetValue(obj) is IEnumerable enumerable))
			{
				return list;
			}
			byte b = 0;
			foreach (object item2 in enumerable)
			{
				list.Add(new SkinEntry
				{
					Index = b,
					Name = (ReadSkinString(item2, "Name") ?? $"Skin {b}"),
					Rarity = (ReadSkinString(item2, "Rarity") ?? "")
				});
				b++;
			}
		}
		catch
		{
		}
		return list;
	}

	private static string ReadSkinString(object skin, string propName)
	{
		PropertyInfo propertyInfo = GameBinder.Property("ItemSkin", propName);
		if (propertyInfo == null || skin == null)
		{
			return null;
		}
		try
		{
			return propertyInfo.GetValue(skin)?.ToString();
		}
		catch
		{
			return null;
		}
	}

	internal static List<SkinEntry> BoatSkins()
	{
		List<SkinEntry> list = new List<SkinEntry>();
		object boat = BoatCheats.Boat;
		if (boat == null)
		{
			return list;
		}
		try
		{
			object obj = GameBinder.Property("Boat", "SkinPreset")?.GetValue(boat);
			if (!Refs.IsAlive(obj))
			{
				return list;
			}
			if (!(GameBinder.Property("SkinPreset", "Skins")?.GetValue(obj) is IEnumerable enumerable))
			{
				return list;
			}
			byte b = 0;
			foreach (object item in enumerable)
			{
				list.Add(new SkinEntry
				{
					Index = b,
					Name = (ReadSkinString(item, "Name") ?? $"Skin {b}"),
					Rarity = (ReadSkinString(item, "Rarity") ?? "")
				});
				b++;
			}
		}
		catch
		{
		}
		return list;
	}

	internal static bool SetBoatSkin(byte index)
	{
		return GameBinder.TryInvoke(GameBinder.Method("Server", "SetBoatSkin", 1), Refs.Server, index);
	}

	internal static string PrettyCharacter(string method)
	{
		if (!method.StartsWith("Unlock"))
		{
			return method;
		}
		return Regex.Replace(method.Substring(6), "(?<!^)([A-Z])", " $1");
	}

	internal static bool UnlockCharacter(string method)
	{
		return GameBinder.TryInvoke(GameBinder.Method("SkinManager", method, 0), null);
	}

	internal static bool UnlockAllCharacters()
	{
		bool result = true;
		string[] characterUnlocks = CharacterUnlocks;
		for (int i = 0; i < characterUnlocks.Length; i++)
		{
			if (!UnlockCharacter(characterUnlocks[i]))
			{
				result = false;
			}
		}
		return result;
	}
}
