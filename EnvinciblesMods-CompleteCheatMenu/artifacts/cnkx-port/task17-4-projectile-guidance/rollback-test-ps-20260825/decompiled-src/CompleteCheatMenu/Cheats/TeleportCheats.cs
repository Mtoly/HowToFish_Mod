using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class TeleportCheats
{
	internal static readonly Dictionary<string, Vector3> Waypoints = new Dictionary<string, Vector3>();

	internal static byte CurrentIsland => ReadByte("CurIsland");

	internal static byte MaxUnlocked => ReadByte("MaxIslandUnlocked");

	internal static int TotalIslands
	{
		get
		{
			if (!GameBinder.TryGet<int>(GameBinder.Field("IslandManager", "TotalIslands"), null, out var value))
			{
				return 0;
			}
			return value;
		}
	}

	private static byte ReadByte(string prop)
	{
		PropertyInfo propertyInfo = GameBinder.Property("OnlineIslandManager", prop);
		if (propertyInfo == null)
		{
			return 0;
		}
		try
		{
			return (byte)propertyInfo.GetValue(null);
		}
		catch
		{
			return 0;
		}
	}

	internal static bool GoToIsland(byte index)
	{
		return GameBinder.TryInvoke(GameBinder.Method("OnlineIslandManager", "TpToSpecificIsland", 1), null, index);
	}

	internal static bool NextIsland(bool previous = false)
	{
		return GameBinder.TryInvoke(GameBinder.Method("OnlineIslandManager", "TpToNextIsland", 1), null, previous);
	}

	internal static bool UnlockIsland(byte index)
	{
		object instance = Refs.FindInScene("OnlineIslandManager");
		return GameBinder.TryInvoke(GameBinder.Method("OnlineIslandManager", "UnlockIsland", 1), instance, index);
	}

	internal static bool UnlockAllIslands()
	{
		int num = Mathf.Max(TotalIslands, 6);
		bool result = true;
		for (byte b = 0; b < num; b++)
		{
			if (!UnlockIsland(b))
			{
				result = false;
			}
		}
		return result;
	}

	internal static List<object> AlivePlayers()
	{
		List<object> list = new List<object>();
		object obj = null;
		PropertyInfo propertyInfo = GameBinder.Property("PlayerManager", "AlivePlayers");
		if (propertyInfo != null)
		{
			try
			{
				obj = propertyInfo.GetValue(null);
			}
			catch
			{
			}
		}
		if (obj == null)
		{
			FieldInfo fieldInfo = GameBinder.Field("PlayerManager", "AlivePlayers");
			if (fieldInfo != null)
			{
				try
				{
					obj = fieldInfo.GetValue(null);
				}
				catch
				{
				}
			}
		}
		if (obj is IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				if (Refs.IsAlive(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	internal static string PlayerName(object player)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Player", "SteamName");
		if (propertyInfo != null)
		{
			try
			{
				string text = propertyInfo.GetValue(player) as string;
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
			catch
			{
			}
		}
		return (player as Object)?.name ?? "player";
	}

	internal static Vector3 PlayerPosition(object player)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Player", "Transform");
		if (propertyInfo == null)
		{
			return Vector3.zero;
		}
		try
		{
			return (propertyInfo.GetValue(player) as Transform)?.position ?? Vector3.zero;
		}
		catch
		{
			return Vector3.zero;
		}
	}

	internal static bool TeleportPlayerTo(object player, Vector3 pos, float yaw = 0f)
	{
		return GameBinder.TryInvoke(GameBinder.Method("Server", "TeleportPlayer", 3), Refs.Server, player, pos, yaw);
	}

	internal static bool TeleportSelfTo(Vector3 pos, float yaw = 0f)
	{
		object localPlayer = Refs.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}
		if (TeleportPlayerTo(localPlayer, pos, yaw))
		{
			return true;
		}
		return GameBinder.TryInvoke(GameBinder.Method("Player", "LocalTeleport", 3), localPlayer, pos, yaw, true);
	}

	internal static bool GoToPlayer(object other)
	{
		if (other == null)
		{
			return false;
		}
		Vector3 vector = PlayerPosition(other);
		if (vector == Vector3.zero)
		{
			Plugin.Log.LogWarning((object)"Target player has no usable position.");
			return false;
		}
		return TeleportSelfTo(vector + Vector3.up * 1.5f);
	}

	internal static bool BringPlayer(object other)
	{
		object localPlayer = Refs.LocalPlayer;
		if (other == null || localPlayer == null)
		{
			return false;
		}
		Vector3 vector = PlayerPosition(localPlayer);
		if (vector == Vector3.zero)
		{
			Plugin.Log.LogWarning((object)"Bring failed: own position unavailable.");
			return false;
		}
		Vector3 pos = vector + Vector3.up * 1.5f + Forward(localPlayer) * 1.5f;
		if (TeleportPlayerTo(other, pos))
		{
			return true;
		}
		Plugin.Log.LogWarning((object)"Server.TeleportPlayer failed; trying the movement component directly.");
		return TeleportViaMovement(other, pos);
	}

	private static Vector3 Forward(object player)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Player", "Transform");
		try
		{
			return (propertyInfo?.GetValue(player) as Transform)?.forward ?? Vector3.forward;
		}
		catch
		{
			return Vector3.forward;
		}
	}

	private static bool TeleportViaMovement(object player, Vector3 pos)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Player", "Movement");
		object obj = null;
		try
		{
			obj = propertyInfo?.GetValue(player);
		}
		catch
		{
		}
		if (obj == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("PlayerMovement", "Teleport", 2), obj, pos, true);
	}

	internal static bool GoToBoat()
	{
		object boat = BoatCheats.Boat;
		if (boat == null)
		{
			return false;
		}
		PropertyInfo propertyInfo = GameBinder.Property("Boat", "VisualBoat");
		try
		{
			Transform transform = propertyInfo?.GetValue(boat) as Transform;
			if (transform == null)
			{
				return false;
			}
			return TeleportSelfTo(transform.position + Vector3.up * 3f);
		}
		catch
		{
			return false;
		}
	}

	internal static bool SaveWaypoint(string name)
	{
		object localPlayer = Refs.LocalPlayer;
		if (localPlayer == null || string.IsNullOrEmpty(name))
		{
			return false;
		}
		Waypoints[name] = PlayerPosition(localPlayer);
		return true;
	}

	internal static bool GoToWaypoint(string name)
	{
		if (Waypoints.TryGetValue(name, out var value))
		{
			return TeleportSelfTo(value);
		}
		return false;
	}
}
