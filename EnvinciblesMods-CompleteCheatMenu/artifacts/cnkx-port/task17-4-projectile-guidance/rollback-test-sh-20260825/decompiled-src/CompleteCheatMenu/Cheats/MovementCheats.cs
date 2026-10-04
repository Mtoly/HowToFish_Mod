using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class MovementCheats
{
	internal static readonly string[] Fields = new string[8] { "_walkSpeed", "_sprintSpeed", "_crouchWalkSpeedMulti", "_jumpForce", "_extraGravityForce", "_acceleration", "_decceleration", "_sinkSpeed" };

	private static readonly Dictionary<string, float> _defaults = new Dictionary<string, float>();

	private static bool _flyActive;

	private static float _savedGravity;

	private static bool _hasSavedGravity;

	internal static bool Available => Refs.LocalMovement != null;

	private static MethodInfo SetVel => GameBinder.Method("PlayerMovement", "SetVel", 1);

	internal static bool FlyAvailable
	{
		get
		{
			if (Refs.LocalMovement != null)
			{
				return GameBinder.Field("PlayerMovement", "_curMoveSpeed") != null;
			}
			return false;
		}
	}

	private static Rigidbody Rigidbody
	{
		get
		{
			object localPlayer = Refs.LocalPlayer;
			if (localPlayer == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Player", "Rigidbody");
			try
			{
				return propertyInfo?.GetValue(localPlayer) as Rigidbody;
			}
			catch
			{
				return null;
			}
		}
	}

	internal static float Get(string field)
	{
		object localMovement = Refs.LocalMovement;
		FieldInfo fieldInfo = GameBinder.Field("PlayerMovement", field);
		if (localMovement == null || fieldInfo == null)
		{
			return 0f;
		}
		if (GameBinder.TryGet<float>(fieldInfo, localMovement, out var value))
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
		object localMovement = Refs.LocalMovement;
		FieldInfo fieldInfo = GameBinder.Field("PlayerMovement", field);
		if (localMovement == null || fieldInfo == null)
		{
			return false;
		}
		if (!_defaults.ContainsKey(field) && GameBinder.TryGet<float>(fieldInfo, localMovement, out var value2))
		{
			_defaults[field] = value2;
		}
		return GameBinder.TrySet(fieldInfo, localMovement, value);
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

	internal static void StopFly()
	{
		if (_flyActive)
		{
			_flyActive = false;
			object localMovement = Refs.LocalMovement;
			if (localMovement != null && _hasSavedGravity)
			{
				GameBinder.TrySet(GameBinder.Field("PlayerMovement", "_extraGravityForce"), localMovement, _savedGravity);
			}
			SetRigidbodyGravity(on: true);
			_hasSavedGravity = false;
		}
	}

	internal static void FlyStep(float horizontalSpeed, float verticalSpeed)
	{
		object localMovement = Refs.LocalMovement;
		if (localMovement != null)
		{
			if (!_flyActive)
			{
				_flyActive = true;
				_hasSavedGravity = GameBinder.TryGet<float>(GameBinder.Field("PlayerMovement", "_extraGravityForce"), localMovement, out _savedGravity);
				SetRigidbodyGravity(on: false);
			}
			GameBinder.TrySet(GameBinder.Field("PlayerMovement", "_curMoveSpeed"), localMovement, horizontalSpeed);
			GameBinder.TrySet(GameBinder.Field("PlayerMovement", "_targetMoveSpeed"), localMovement, horizontalSpeed);
			GameBinder.TrySet(GameBinder.Field("PlayerMovement", "_extraGravityForce"), localMovement, 0f);
			float num = 0f;
			if (Input.GetKey(KeyCode.Space))
			{
				num += verticalSpeed;
			}
			if (Input.GetKey(KeyCode.LeftControl))
			{
				num -= verticalSpeed;
			}
			Rigidbody rigidbody = Rigidbody;
			if (rigidbody != null)
			{
				Vector3 linearVelocity = rigidbody.linearVelocity;
				linearVelocity.y = num;
				rigidbody.linearVelocity = linearVelocity;
			}
		}
	}

	private static void SetRigidbodyGravity(bool on)
	{
		Rigidbody rigidbody = Rigidbody;
		if (rigidbody != null)
		{
			rigidbody.useGravity = on;
		}
	}

	internal static void FreezePlayer()
	{
		object localMovement = Refs.LocalMovement;
		if (localMovement != null)
		{
			GameBinder.TrySet(GameBinder.Field("PlayerMovement", "_curMoveSpeed"), localMovement, 0f);
			GameBinder.TrySet(GameBinder.Field("PlayerMovement", "_targetMoveSpeed"), localMovement, 0f);
			Rigidbody rigidbody = Rigidbody;
			if (rigidbody != null)
			{
				rigidbody.linearVelocity = Vector3.zero;
			}
		}
	}
}
