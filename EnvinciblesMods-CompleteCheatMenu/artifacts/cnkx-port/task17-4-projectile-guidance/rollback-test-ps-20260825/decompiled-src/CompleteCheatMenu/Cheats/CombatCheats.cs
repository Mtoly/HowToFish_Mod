using System.Reflection;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Cheats;

internal static class CombatCheats
{
	internal static bool GodMode
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("PlayerManager", "InGodMode");
			if (propertyInfo == null)
			{
				return false;
			}
			try
			{
				return (bool)propertyInfo.GetValue(null);
			}
			catch
			{
				return false;
			}
		}
	}

	internal static bool OneShot
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("ServerSettings", "OneShotEnabled");
			if (propertyInfo == null)
			{
				return false;
			}
			try
			{
				return (bool)propertyInfo.GetValue(null);
			}
			catch
			{
				return false;
			}
		}
	}

	internal static bool FriendlyFire
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("ServerSettings", "UseFriendlyFire");
			if (propertyInfo == null)
			{
				return true;
			}
			try
			{
				return (bool)propertyInfo.GetValue(null);
			}
			catch
			{
				return true;
			}
		}
	}

	internal static int Health => ReadVital("Health");

	internal static int Fullness => ReadVital("Fullness");

	internal static bool SetGodMode(bool to)
	{
		if (GodMode == to)
		{
			return true;
		}
		return GameBinder.TryInvoke(GameBinder.Method("PlayerManager", "ToggleGodMode", 0), null);
	}

	internal static bool SetOneShot(bool to)
	{
		if (OneShot == to)
		{
			return true;
		}
		return GameBinder.TryInvoke(GameBinder.Method("ServerSettings", "ToggleOneShot", 0), Refs.ServerSettings);
	}

	internal static bool SetFriendlyFire(bool to)
	{
		return GameBinder.TryInvoke(GameBinder.Method("ServerSettings", "ToggleFriendlyFire", 1), Refs.ServerSettings, to);
	}

	private static int ReadVital(string prop)
	{
		object localVitals = Refs.LocalVitals;
		if (localVitals == null)
		{
			return 0;
		}
		PropertyInfo propertyInfo = GameBinder.Property("PlayerVitals", prop);
		if (propertyInfo == null)
		{
			return 0;
		}
		try
		{
			return (int)propertyInfo.GetValue(localVitals);
		}
		catch
		{
			return 0;
		}
	}

	internal static bool Heal(int amount)
	{
		return GameBinder.TryInvoke(GameBinder.Method("PlayerVitals", "Heal", 1), Refs.LocalVitals, amount);
	}

	internal static bool RestoreFullness(int amount)
	{
		return GameBinder.TryInvoke(GameBinder.Method("PlayerVitals", "RestoreFullness", 1), Refs.LocalVitals, amount);
	}

	internal static bool ResetVitals()
	{
		return GameBinder.TryInvoke(GameBinder.Method("PlayerVitals", "ServerResetVitals", 0), Refs.LocalVitals);
	}
}
