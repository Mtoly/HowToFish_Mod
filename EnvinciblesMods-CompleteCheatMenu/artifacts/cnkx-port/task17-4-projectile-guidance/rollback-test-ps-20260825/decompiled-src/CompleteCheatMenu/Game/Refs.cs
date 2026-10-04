using System;
using System.Reflection;
using UnityEngine;

namespace CompleteCheatMenu.Game;

internal static class Refs
{
	private static MethodInfo _findAny;

	private static bool _findAnyResolved;

	internal static object LocalPlayer
	{
		get
		{
			FieldInfo fieldInfo = GameBinder.Field("Player", "LocalPlayer");
			if (fieldInfo == null)
			{
				return null;
			}
			object value = fieldInfo.GetValue(null);
			if (!IsAlive(value))
			{
				return null;
			}
			return value;
		}
	}

	internal static object LocalMovement => GetPlayerPart("Movement");

	internal static object LocalVitals => GetPlayerPart("Vitals");

	internal static object LocalCamera => GetPlayerPart("Camera");

	internal static object LocalHolding => GetPlayerPart("Holding");

	internal static object LocalInventory => GetPlayerPart("Inventory");

	internal static object Server => StaticInstance("Server", "Instance");

	internal static object ServerSettings => StaticInstance("ServerSettings", "Instance");

	internal static object BoatManager => StaticInstance("BoatManager", "Instance");

	internal static bool IsHost
	{
		get
		{
			object server = Server;
			if (server == null)
			{
				return false;
			}
			PropertyInfo property = server.GetType().GetProperty("IsServerInitialized", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null)
			{
				return false;
			}
			try
			{
				return (bool)property.GetValue(server);
			}
			catch
			{
				return false;
			}
		}
	}

	internal static bool InGame => LocalPlayer != null;

	internal static Camera CurrentCamera
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("GameInfo", "CurCamera");
			if (propertyInfo != null)
			{
				try
				{
					if (propertyInfo.GetValue(null) is Camera camera && camera != null)
					{
						return camera;
					}
				}
				catch
				{
				}
			}
			return Camera.main;
		}
	}

	private static object GetPlayerPart(string propName)
	{
		object localPlayer = LocalPlayer;
		if (localPlayer == null)
		{
			return null;
		}
		PropertyInfo propertyInfo = GameBinder.Property("Player", propName);
		if (propertyInfo == null)
		{
			return null;
		}
		try
		{
			return propertyInfo.GetValue(localPlayer);
		}
		catch
		{
			return null;
		}
	}

	private static object StaticInstance(string typeName, string memberName)
	{
		PropertyInfo propertyInfo = GameBinder.Type(typeName)?.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (propertyInfo != null)
		{
			try
			{
				object value = propertyInfo.GetValue(null);
				if (IsAlive(value))
				{
					return value;
				}
			}
			catch
			{
			}
		}
		FieldInfo fieldInfo = GameBinder.Type(typeName)?.GetField(memberName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (fieldInfo != null)
		{
			try
			{
				object value2 = fieldInfo.GetValue(null);
				if (IsAlive(value2))
				{
					return value2;
				}
			}
			catch
			{
			}
		}
		return FindInScene(typeName);
	}

	internal static object FindInScene(string typeName)
	{
		Type type = GameBinder.Type(typeName);
		if (type == null || !typeof(UnityEngine.Object).IsAssignableFrom(type))
		{
			return null;
		}
		try
		{
			UnityEngine.Object obj = SceneFind(type);
			return IsAlive(obj) ? obj : null;
		}
		catch
		{
			return null;
		}
	}

	private static UnityEngine.Object SceneFind(Type t)
	{
		if (!_findAnyResolved)
		{
			_findAnyResolved = true;
			_findAny = typeof(UnityEngine.Object).GetMethod("FindAnyObjectByType", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(Type) }, null);
		}
		if (_findAny != null)
		{
			try
			{
				return (UnityEngine.Object)_findAny.Invoke(null, new object[1] { t });
			}
			catch
			{
			}
		}
		return UnityEngine.Object.FindObjectOfType(t);
	}

	internal static bool IsAlive(object o)
	{
		if (o == null)
		{
			return false;
		}
		if (o is UnityEngine.Object obj)
		{
			return obj != null;
		}
		return true;
	}
}
