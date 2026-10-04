using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CompleteCheatMenu.Game;

internal static class GameBinder
{
	internal const BindingFlags Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

	private static readonly Dictionary<string, MemberInfo> _members = new Dictionary<string, MemberInfo>();

	private static readonly Dictionary<string, string> Disambiguate = new Dictionary<string, string> { { "Player", "LocalPlayer" } };

	internal static Type Type(string name)
	{
		if (_types.TryGetValue(name, out var value))
		{
			return value;
		}
		string value2;
		Type type = (Disambiguate.TryGetValue(name, out value2) ? ResolveAmbiguous(name, value2) : AccessTools.TypeByName(name));
		_types[name] = type;
		BindingReport.Record("type " + name, type != null);
		return type;
	}

	private static Type ResolveAmbiguous(string name, string marker)
	{
		List<Type> list = (from x in AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes)
			where x.Name == name
			select x).ToList();
		foreach (Type item in list)
		{
			if (item.GetField(marker, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null || item.GetProperty(marker, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null)
			{
				if (list.Count > 1)
				{
					Plugin.Log.LogInfo((object)($"Type '{name}' was ambiguous ({list.Count} matches); " + "picked " + item.FullName + " by '" + marker + "'."));
				}
				return item;
			}
		}
		return list.FirstOrDefault() ?? AccessTools.TypeByName(name);
	}

	private static IEnumerable<Type> SafeTypes(Assembly asm)
	{
		try
		{
			return asm.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type t) => t != null);
		}
		catch
		{
			return Enumerable.Empty<Type>();
		}
	}

	internal static FieldInfo Field(string typeName, string fieldName)
	{
		string key = "F:" + typeName + "." + fieldName;
		if (_members.TryGetValue(key, out var value))
		{
			return (FieldInfo)value;
		}
		FieldInfo fieldInfo = Type(typeName)?.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		_members[key] = fieldInfo;
		BindingReport.Record(typeName + "." + fieldName, fieldInfo != null);
		return fieldInfo;
	}

	internal static PropertyInfo Property(string typeName, string propName)
	{
		string key = "P:" + typeName + "." + propName;
		if (_members.TryGetValue(key, out var value))
		{
			return (PropertyInfo)value;
		}
		PropertyInfo propertyInfo = Type(typeName)?.GetProperty(propName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		_members[key] = propertyInfo;
		BindingReport.Record(typeName + "." + propName, propertyInfo != null);
		return propertyInfo;
	}

	internal static MethodInfo Method(string typeName, string methodName, int argCount = -1, Type[] paramTypes = null)
	{
		string key = string.Format("M:{0}.{1}/{2}/{3}", typeName, methodName, argCount, (paramTypes == null) ? "" : string.Join(",", paramTypes.Select((Type t) => t.Name)));
		if (_members.TryGetValue(key, out var value))
		{
			return (MethodInfo)value;
		}
		Type type = Type(typeName);
		MethodInfo methodInfo = null;
		if (type != null)
		{
			IEnumerable<MethodInfo> source = from x in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where x.Name == methodName
				select x;
			if (paramTypes != null)
			{
				source = source.Where((MethodInfo x) => (from p in x.GetParameters()
					select p.ParameterType).SequenceEqual(paramTypes));
			}
			else if (argCount >= 0)
			{
				source = source.Where((MethodInfo x) => x.GetParameters().Length == argCount);
			}
			methodInfo = source.FirstOrDefault();
		}
		_members[key] = methodInfo;
		BindingReport.Record(typeName + "." + methodName + "()", methodInfo != null);
		return methodInfo;
	}

	internal static bool TryGet<T>(FieldInfo f, object instance, out T value)
	{
		value = default(T);
		if (f == null)
		{
			return false;
		}
		try
		{
			value = (T)Convert.ChangeType(f.GetValue(instance), typeof(T));
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool TrySet(FieldInfo f, object instance, object value)
	{
		if (f == null)
		{
			return false;
		}
		try
		{
			f.SetValue(instance, Convert.ChangeType(value, f.FieldType));
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Set " + f.Name + " failed: " + ex.Message));
			return false;
		}
	}

	internal static bool TryInvoke(MethodInfo m, object instance, params object[] args)
	{
		if (m == null)
		{
			return false;
		}
		try
		{
			m.Invoke(m.IsStatic ? null : instance, args);
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Invoke " + m.Name + " failed: " + (ex.InnerException?.Message ?? ex.Message)));
			return false;
		}
	}

	internal static object InvokeResult(MethodInfo m, object instance, params object[] args)
	{
		if (m == null)
		{
			return null;
		}
		try
		{
			return m.Invoke(m.IsStatic ? null : instance, args);
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Invoke " + m.Name + " failed: " + (ex.InnerException?.Message ?? ex.Message)));
			return null;
		}
	}

	internal static bool TryReadSyncVar<T>(FieldInfo syncVarField, object instance, out T value)
	{
		value = default(T);
		if (syncVarField == null)
		{
			return false;
		}
		try
		{
			object value2 = syncVarField.GetValue(instance);
			if (value2 == null)
			{
				return false;
			}
			PropertyInfo property = value2.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
			if (property == null)
			{
				return false;
			}
			value = (T)Convert.ChangeType(property.GetValue(value2), typeof(T));
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool TryWriteSyncVar(FieldInfo syncVarField, object instance, object value)
	{
		if (syncVarField == null)
		{
			return false;
		}
		try
		{
			object value2 = syncVarField.GetValue(instance);
			if (value2 == null)
			{
				return false;
			}
			PropertyInfo property = value2.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
			if (property == null || !property.CanWrite)
			{
				return false;
			}
			property.SetValue(value2, Convert.ChangeType(value, property.PropertyType));
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("SyncVar write on " + syncVarField.Name + " failed: " + ex.Message));
			return false;
		}
	}

	internal static object EnumValue(string typeName, string memberName)
	{
		Type type = Type(typeName);
		if (type == null || !type.IsEnum)
		{
			BindingReport.Record("enum " + typeName, ok: false);
			return null;
		}
		try
		{
			return Enum.Parse(type, memberName, ignoreCase: true);
		}
		catch
		{
			BindingReport.Record("enum " + typeName + "." + memberName, ok: false);
			return null;
		}
	}

	internal static string[] EnumNames(string typeName)
	{
		Type type = Type(typeName);
		if (type == null || !type.IsEnum)
		{
			return new string[0];
		}
		try
		{
			return Enum.GetNames(type);
		}
		catch
		{
			return new string[0];
		}
	}
}
