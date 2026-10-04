using System.Reflection;
using CompleteCheatMenu.Game;

namespace CompleteCheatMenu.Cheats;

internal static class WorldCheats
{
	internal static int TotalCreatures
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("GameInfo", "AllCreatureCount");
			if (propertyInfo == null)
			{
				return 0;
			}
			try
			{
				return (int)propertyInfo.GetValue(null);
			}
			catch
			{
				return 0;
			}
		}
	}

	internal static object Boss
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("BossManager", "Boss");
			if (propertyInfo == null)
			{
				return null;
			}
			try
			{
				object value = propertyInfo.GetValue(null);
				return Refs.IsAlive(value) ? value : null;
			}
			catch
			{
				return null;
			}
		}
	}

	internal static bool BossActive => Boss != null;

	internal static int BossHp
	{
		get
		{
			object boss = Boss;
			if (boss == null)
			{
				return 0;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Creature", "Hp");
			if (propertyInfo == null)
			{
				return 0;
			}
			try
			{
				return (int)propertyInfo.GetValue(boss);
			}
			catch
			{
				return 0;
			}
		}
	}

	internal static int BossMaxHp
	{
		get
		{
			FieldInfo fieldInfo = GameBinder.Field("BossManager", "BossMaxHp");
			if (fieldInfo != null && GameBinder.TryGet<int>(fieldInfo, null, out var value))
			{
				return value;
			}
			PropertyInfo propertyInfo = GameBinder.Property("BossManager", "BossMaxHp");
			if (propertyInfo == null)
			{
				return 0;
			}
			try
			{
				return (int)propertyInfo.GetValue(null);
			}
			catch
			{
				return 0;
			}
		}
	}

	internal static bool Immortal
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("BossManager", "IsImmortal");
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

	internal static int KilledCount(bool drip)
	{
		object obj = GameBinder.InvokeResult(GameBinder.Method("GameInfo", "KilledCreaturesCount", 1), null, drip);
		if (obj is int)
		{
			return (int)obj;
		}
		return 0;
	}

	internal static bool AllKilled(bool drip)
	{
		object obj = GameBinder.InvokeResult(GameBinder.Method("GameInfo", "HasKilledAllCreatures", 1), null, drip);
		bool flag = default(bool);
		int num;
		if (obj is bool)
		{
			flag = (bool)obj;
			num = 1;
		}
		else
		{
			num = 0;
		}
		return (byte)((uint)num & (flag ? 1u : 0u)) != 0;
	}

	internal static bool SetAllCreaturesKilled(bool to, bool drip)
	{
		return GameBinder.TryInvoke(GameBinder.Method("GameInfo", "ToggleAllCreaturesKilled", 2), null, to, drip);
	}

	internal static bool SetBossImmortal(bool to)
	{
		return GameBinder.TryInvoke(GameBinder.Method("BossManager", "ToggleImmortal", 1), null, to);
	}

	internal static bool HealBoss(int amount)
	{
		return GameBinder.TryInvoke(GameBinder.Method("Creature", "HealBoss", 1), Boss, amount);
	}

	internal static bool KillBoss()
	{
		object boss = Boss;
		if (boss == null)
		{
			return false;
		}
		if (Immortal)
		{
			SetBossImmortal(to: false);
		}
		int bossHp = BossHp;
		if (bossHp <= 0)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("Creature", "ServerChangeHp", 1), boss, bossHp);
	}

	internal static bool UnlockGrill()
	{
		return GameBinder.TryInvoke(GameBinder.Method("NPCManager", "UnlockGrill", 0), null);
	}
}
