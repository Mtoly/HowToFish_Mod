using System.Reflection;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Cheats;

internal static class MoneyCheats
{
	private static object Manager => Refs.FindInScene("MoneyManager");

	internal static bool Available
	{
		get
		{
			if (GameBinder.Method("MoneyManager", "AddMoney", 2) != null)
			{
				return Refs.LocalPlayer != null;
			}
			return false;
		}
	}

	internal static int Balance
	{
		get
		{
			object manager = Manager;
			if (manager == null)
			{
				return 0;
			}
			if (!GameBinder.TryReadSyncVar<int>(GameBinder.Field("MoneyManager", "_money"), manager, out var value))
			{
				return 0;
			}
			return value;
		}
	}

	internal static bool Add(int amount)
	{
		if (amount == 0)
		{
			return true;
		}
		MethodInfo methodInfo = GameBinder.Method("MoneyManager", "AddMoney", 2);
		object localPlayer = Refs.LocalPlayer;
		if (methodInfo == null || localPlayer == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(methodInfo, null, amount, localPlayer);
	}

	internal static bool Remove(int amount)
	{
		if (amount == 0)
		{
			return true;
		}
		MethodInfo methodInfo = GameBinder.Method("MoneyManager", "RemoveMoney", 2);
		object localPlayer = Refs.LocalPlayer;
		if (methodInfo == null || localPlayer == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(methodInfo, null, amount, localPlayer);
	}

	internal static bool SetTo(int target)
	{
		int num = target - Balance;
		if (num == 0)
		{
			return true;
		}
		if (num <= 0)
		{
			return Remove(-num);
		}
		return Add(num);
	}
}
