using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class BoatCheats
{
	private static bool _flyActive;

	private static readonly Dictionary<object, float> _motorDefaults = new Dictionary<object, float>();

	internal static readonly string[] Fields = new string[6] { "_antiRollOverForce", "_steerSmoothing", "_linearDampAboveWater", "_angularDampAboveWater", "_underwaterLinearDamp", "_underwaterAngularDamp" };

	internal static readonly string[] CosmeticFields = new string[2] { "_maxSteerAngle", "_maxThrottleAngle" };

	private static readonly Dictionary<string, float> _defaults = new Dictionary<string, float>();

	internal static object Boat
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("BoatManager", "Boat");
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

	internal static bool Available => Boat != null;

	internal static bool Unlocked => ReadBool("BoatUnlocked");

	internal static bool RadarUnlocked => ReadBool("BoatRadarUnlocked");

	internal static byte Motor
	{
		get
		{
			object boat = Boat;
			if (boat == null)
			{
				return 0;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Boat", "MotorIndex");
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

	internal static bool FlyActive => _flyActive;

	internal static bool FlyAvailable => Rig != null;

	private static Rigidbody Rig
	{
		get
		{
			object boat = Boat;
			if (boat == null)
			{
				return null;
			}
			string[] array = new string[2] { "HiddenPhysicsRig", "VisualPhysicsRig" };
			foreach (string propName in array)
			{
				PropertyInfo propertyInfo = GameBinder.Property("Boat", propName);
				if (propertyInfo == null)
				{
					continue;
				}
				try
				{
					if (propertyInfo.GetValue(boat) is Rigidbody rigidbody && rigidbody != null)
					{
						return rigidbody;
					}
				}
				catch
				{
				}
			}
			return null;
		}
	}

	private static IEnumerable<object> Motors
	{
		get
		{
			object boat = Boat;
			FieldInfo fieldInfo = GameBinder.Field("Boat", "_motors");
			if (boat == null || fieldInfo == null)
			{
				yield break;
			}
			object value;
			try
			{
				value = fieldInfo.GetValue(boat);
			}
			catch
			{
				yield break;
			}
			if (!(value is IEnumerable enumerable))
			{
				yield break;
			}
			foreach (object item in enumerable)
			{
				if (Refs.IsAlive(item))
				{
					yield return item;
				}
			}
		}
	}

	internal static float MotorForce
	{
		get
		{
			foreach (object motor in Motors)
			{
				PropertyInfo propertyInfo = GameBinder.Property("BoatMotor", "Force");
				if (propertyInfo == null)
				{
					continue;
				}
				try
				{
					float num = (float)propertyInfo.GetValue(motor);
					if (!_motorDefaults.ContainsKey(motor))
					{
						_motorDefaults[motor] = num;
					}
					return num;
				}
				catch
				{
				}
			}
			return 0f;
		}
	}

	internal static float MotorForceDefault
	{
		get
		{
			using (Dictionary<object, float>.Enumerator enumerator = _motorDefaults.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current.Value;
				}
			}
			return 0f;
		}
	}

	internal static float MaxSpeed
	{
		get
		{
			Rigidbody rig = Rig;
			if (!(rig != null))
			{
				return Get("_maxVelocity");
			}
			return rig.maxLinearVelocity;
		}
	}

	private static bool ReadBool(string prop)
	{
		object boat = Boat;
		if (boat == null)
		{
			return false;
		}
		PropertyInfo propertyInfo = GameBinder.Property("Boat", prop);
		if (propertyInfo == null)
		{
			return false;
		}
		try
		{
			return (bool)propertyInfo.GetValue(boat);
		}
		catch
		{
			return false;
		}
	}

	internal static bool UnlockBoat()
	{
		return GameBinder.TryInvoke(GameBinder.Method("Boat", "UnlockBoat", 0), Boat);
	}

	internal static bool UnlockRadar()
	{
		return GameBinder.TryInvoke(GameBinder.Method("Boat", "UnlockBoatRadar", 0), Boat);
	}

	internal static bool SetMotor(byte tier)
	{
		object boat = Boat;
		if (boat == null)
		{
			return false;
		}
		if (tier > Motor)
		{
			return GameBinder.TryInvoke(GameBinder.Method("Boat", "SetMotor", 1), boat, tier);
		}
		if (tier == Motor)
		{
			return true;
		}
		return GameBinder.TryWriteSyncVar(GameBinder.Field("Boat", "_motorIndex"), boat, tier);
	}

	internal static bool ToggleFly(bool on)
	{
		Rigidbody rig = Rig;
		if (rig == null)
		{
			return false;
		}
		_flyActive = on;
		rig.useGravity = !on;
		return true;
	}

	internal static void FlyStep(float verticalSpeed)
	{
		Rigidbody rig = Rig;
		if (!(rig == null))
		{
			rig.useGravity = false;
			object boat = Boat;
			if (boat != null)
			{
				GameBinder.TrySet(GameBinder.Field("Boat", "_propellerInWater"), boat, true);
			}
			float num = 0f;
			if (Input.GetKey(KeyCode.Space))
			{
				num += verticalSpeed;
			}
			if (Input.GetKey(KeyCode.LeftControl))
			{
				num -= verticalSpeed;
			}
			Vector3 linearVelocity = rig.linearVelocity;
			linearVelocity.y = num;
			rig.linearVelocity = linearVelocity;
		}
	}

	internal static bool SetMotorForce(float force)
	{
		FieldInfo fieldInfo = GameBinder.Field("BoatMotor", "<Force>k__BackingField");
		if (fieldInfo == null)
		{
			return false;
		}
		bool result = false;
		foreach (object motor in Motors)
		{
			if (!_motorDefaults.ContainsKey(motor) && GameBinder.TryGet<float>(fieldInfo, motor, out var value))
			{
				_motorDefaults[motor] = value;
			}
			if (GameBinder.TrySet(fieldInfo, motor, force))
			{
				result = true;
			}
		}
		return result;
	}

	internal static bool SetMaxSpeed(float value)
	{
		Rigidbody rig = Rig;
		if (rig != null)
		{
			rig.maxLinearVelocity = value;
		}
		Set("_maxVelocity", value);
		return rig != null;
	}

	internal static float Get(string field)
	{
		object boat = Boat;
		FieldInfo fieldInfo = GameBinder.Field("Boat", field);
		if (boat == null || fieldInfo == null)
		{
			return 0f;
		}
		if (GameBinder.TryGet<float>(fieldInfo, boat, out var value))
		{
			if (!_defaults.ContainsKey(field))
			{
				_defaults[field] = value;
			}
			return value;
		}
		return 0f;
	}

	internal static bool Set(string field, float value)
	{
		object boat = Boat;
		FieldInfo fieldInfo = GameBinder.Field("Boat", field);
		if (boat == null || fieldInfo == null)
		{
			return false;
		}
		if (!_defaults.ContainsKey(field) && GameBinder.TryGet<float>(fieldInfo, boat, out var value2))
		{
			_defaults[field] = value2;
		}
		return GameBinder.TrySet(fieldInfo, boat, value);
	}

	internal static float Default(string field)
	{
		if (!_defaults.TryGetValue(field, out var value))
		{
			return 0f;
		}
		return value;
	}

	internal static void ResetAll()
	{
		foreach (KeyValuePair<string, float> @default in _defaults)
		{
			Set(@default.Key, @default.Value);
		}
	}

	internal static bool BringToPlayer()
	{
		object boatManager = Refs.BoatManager;
		Camera currentCamera = Refs.CurrentCamera;
		if (boatManager == null || currentCamera == null)
		{
			return false;
		}
		Vector3 vector = currentCamera.transform.position + currentCamera.transform.forward * 8f;
		vector.y += 2f;
		return GameBinder.TryInvoke(GameBinder.Method("BoatManager", "TryMoveBoat", 2), boatManager, vector, Quaternion.identity);
	}
}
