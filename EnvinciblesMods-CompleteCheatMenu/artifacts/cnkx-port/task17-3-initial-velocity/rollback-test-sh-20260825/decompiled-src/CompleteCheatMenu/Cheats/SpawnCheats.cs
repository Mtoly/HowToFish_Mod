using System;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class SpawnCheats
{
	internal static bool Available
	{
		get
		{
			if (GameBinder.Method("GameInfo", "GetSpawnable", 1, new Type[1] { typeof(string) }) != null)
			{
				return Refs.Server != null;
			}
			return false;
		}
	}

	internal static bool Spawn(string key, int count = 1, bool asDead = false, bool asDrip = false)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		if (CheatGate.PageBlockReason() != null)
		{
			return false;
		}
		bool result = true;
		for (int i = 0; i < Mathf.Clamp(count, 1, 100); i++)
		{
			if (!SpawnOne(key, asDead, asDrip))
			{
				result = false;
			}
		}
		return result;
	}

	private static bool SpawnOne(string key, bool asDead, bool asDrip)
	{
		try
		{
			UnityEngine.Object obj = GameBinder.InvokeResult(GameBinder.Method("GameInfo", "GetSpawnable", 1, new Type[1] { typeof(string) }), null, key) as UnityEngine.Object;
			if (!Refs.IsAlive(obj))
			{
				Plugin.Log.LogWarning((object)("No spawnable named '" + key + "'."));
				return false;
			}
			Camera currentCamera = Refs.CurrentCamera;
			if (currentCamera == null)
			{
				return false;
			}
			Vector3 position = currentCamera.transform.position + currentCamera.transform.forward * 2f;
			UnityEngine.Object obj2 = UnityEngine.Object.Instantiate(obj, position, Quaternion.identity);
			if (!Refs.IsAlive(obj2))
			{
				return false;
			}
			object creature = GetCreature(obj2);
			if (asDrip && creature != null)
			{
				GameBinder.TryInvoke(GameBinder.Method("Creature", "SetDrip", 0), creature);
			}
			if (asDead && creature != null)
			{
				GameBinder.TryInvoke(GameBinder.Method("Creature", "ServerKillOnSpawn", 0), creature);
			}
			return NetworkSpawn(obj2);
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Spawn '" + key + "' failed: " + ex.Message));
			return false;
		}
	}

	private static object GetCreature(object item)
	{
		PropertyInfo propertyInfo = GameBinder.Property("Item", "Creature");
		if (propertyInfo == null)
		{
			return null;
		}
		try
		{
			object value = propertyInfo.GetValue(item);
			return Refs.IsAlive(value) ? value : null;
		}
		catch
		{
			return null;
		}
	}

	private static bool NetworkSpawn(UnityEngine.Object clone)
	{
		object server = Refs.Server;
		if (server == null)
		{
			return false;
		}
		GameObject gameObject = GetGameObject(clone);
		if (gameObject == null)
		{
			return false;
		}
		MethodInfo[] methods = server.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name != "Spawn")
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 0 && !(parameters[0].ParameterType != typeof(GameObject)))
			{
				object[] array = new object[parameters.Length];
				array[0] = gameObject;
				for (int j = 1; j < parameters.Length; j++)
				{
					array[j] = (parameters[j].HasDefaultValue ? parameters[j].DefaultValue : (parameters[j].ParameterType.IsValueType ? Activator.CreateInstance(parameters[j].ParameterType) : null));
				}
				if (GameBinder.TryInvoke(methodInfo, server, array))
				{
					return true;
				}
			}
		}
		BindingReport.Record("Server.Spawn(GameObject)", ok: false);
		return false;
	}

	private static GameObject GetGameObject(UnityEngine.Object o)
	{
		if (o is GameObject result)
		{
			return result;
		}
		if (o is Component component)
		{
			return component.gameObject;
		}
		PropertyInfo property = o.GetType().GetProperty("gameObject", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		try
		{
			return property?.GetValue(o) as GameObject;
		}
		catch
		{
			return null;
		}
	}
}
