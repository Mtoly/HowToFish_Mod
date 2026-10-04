using System.Reflection;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Cheats;

internal static class ProgressionCheats
{
	internal static byte ExtraSlots
	{
		get
		{
			object localInventory = Refs.LocalInventory;
			if (localInventory == null)
			{
				return 0;
			}
			PropertyInfo propertyInfo = GameBinder.Property("PlayerInventory", "ExtraSlots");
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

	private static void Guard()
	{
		if (Plugin.BackupSaves != null && Plugin.BackupSaves.Value)
		{
			CheatGate.BackupSavesOnce();
		}
	}

	internal static bool UnlockAchievements()
	{
		Guard();
		return GameBinder.TryInvoke(GameBinder.Method("AchievementManager", "ToggleAllAchievements", 1), null, true);
	}

	internal static bool LockAchievements()
	{
		Guard();
		return GameBinder.TryInvoke(GameBinder.Method("AchievementManager", "ToggleAllAchievements", 1), null, false);
	}

	internal static bool FinishGame()
	{
		Guard();
		return GameBinder.TryInvoke(GameBinder.Method("DazedCommands", "UseFinishGameCommand", 0), null);
	}

	internal static bool ShowKillScores()
	{
		return GameBinder.TryInvoke(GameBinder.Method("DazedCommands", "UseShowKillScoresCommand", 0), null);
	}

	internal static bool UnlockPocket(byte index)
	{
		Guard();
		object localInventory = Refs.LocalInventory;
		if (localInventory == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("PlayerInventory", "UnlockExtraPocket", 1), localInventory, index);
	}

	internal static bool UnlockAllPockets()
	{
		bool result = true;
		for (byte b = 1; b <= 4; b++)
		{
			if (!UnlockPocket(b))
			{
				result = false;
			}
		}
		return result;
	}

	internal static bool SaveNow()
	{
		return GameBinder.TryInvoke(GameBinder.Method("SaveManager", "SaveServer", 1), null, false);
	}

	internal static bool UnlockEverything()
	{
		Guard();
		bool result = true;
		if (!SkinCheats.UnlockAll())
		{
			result = false;
		}
		if (!SkinCheats.UnlockAllCharacters())
		{
			result = false;
		}
		if (!TeleportCheats.UnlockAllIslands())
		{
			result = false;
		}
		if (!BoatCheats.UnlockBoat())
		{
			result = false;
		}
		if (!BoatCheats.UnlockRadar())
		{
			result = false;
		}
		if (!WorldCheats.UnlockGrill())
		{
			result = false;
		}
		if (!UnlockAllPockets())
		{
			result = false;
		}
		return result;
	}
}
