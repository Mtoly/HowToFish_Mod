using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.Targeting;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class WeaponCheats
{
	private static readonly Dictionary<object, Dictionary<string, object>> _originals = new Dictionary<object, Dictionary<string, object>>();

	private static float _nextTargetPreview;

	private static readonly TargetingSystem _targetingSystem = new TargetingSystem();

	internal static object HeldWeapon
	{
		get
		{
			object heldItem = ItemCheats.HeldItem;
			if (heldItem == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Item", "Weapon");
			try
			{
				object obj = propertyInfo?.GetValue(heldItem);
				return Refs.IsAlive(obj) ? obj : null;
			}
			catch
			{
				return null;
			}
		}
	}

	internal static bool HoldingWeapon => HeldWeapon != null;

	internal static string WeaponName => (ItemCheats.HeldItem as Object)?.name ?? "-";

	private static object Attachments
	{
		get
		{
			object heldWeapon = HeldWeapon;
			if (heldWeapon == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Weapon", "Attachments");
			try
			{
				object obj = propertyInfo?.GetValue(heldWeapon);
				return Refs.IsAlive(obj) ? obj : null;
			}
			catch
			{
				return null;
			}
		}
	}

	internal static int Ammo
	{
		get
		{
			object heldWeapon = HeldWeapon;
			if (heldWeapon == null)
			{
				return 0;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Weapon", "Ammo");
			try
			{
				return (!(propertyInfo == null)) ? ((int)propertyInfo.GetValue(heldWeapon)) : 0;
			}
			catch
			{
				return 0;
			}
		}
	}

	internal static TargetSolution CurrentTarget { get; private set; }

	private static Transform FirePoint
	{
		get
		{
			object attachments = Attachments;
			if (attachments == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Attachments", "FirePoint");
			try
			{
				return propertyInfo?.GetValue(attachments) as Transform;
			}
			catch
			{
				return null;
			}
		}
	}

	internal static bool HasOriginals
	{
		get
		{
			if (HeldWeapon != null && _originals.TryGetValue(HeldWeapon, out var value))
			{
				return value.Count > 0;
			}
			return false;
		}
	}

	internal static void Tick()
	{
		object heldWeapon = HeldWeapon;
		if (heldWeapon != null)
		{
			if (CheatState.InfiniteAmmo)
			{
				ApplyInfiniteAmmo(heldWeapon);
			}
			if (CheatState.NoSpread)
			{
				Set(heldWeapon, "_spread", 0f);
			}
			if (CheatState.ZeroRecoil)
			{
				Set(heldWeapon, "_recoilKnockback", 0);
			}
			if (CheatState.RapidFire)
			{
				Set(heldWeapon, "_timeBetweenShots", Mathf.Max(0.01f, CheatState.FireInterval));
			}
			if (CheatState.MultiShot)
			{
				Set(heldWeapon, "_projectileCountPerShot", Mathf.RoundToInt(CheatState.ProjectileCount));
			}
			if (CheatState.FastBullets)
			{
				Set(heldWeapon, "_projSpeed", CheatState.ProjectileSpeed);
			}
			if (!CheatState.Aimbot)
			{
				CurrentTarget = null;
				_targetingSystem.Clear();
			}
			else
			{
				UpdateTargetPreview();
			}
		}
	}

	private static void UpdateTargetPreview()
	{
		if (Time.unscaledTime < _nextTargetPreview)
		{
			return;
		}
		_nextTargetPreview = Time.unscaledTime + 0.05f;
		Camera currentCamera = Refs.CurrentCamera;
		if (currentCamera == null)
		{
			CurrentTarget = null;
		}
		else
		{
			CurrentTarget = _targetingSystem.Acquire(currentCamera);
		}
	}

	internal static void AimBeforeShot()
	{
		if (!CheatState.Aimbot && !CheatState.FireWhereLooking)
		{
			return;
		}
		Camera currentCamera = Refs.CurrentCamera;
		Transform firePoint = FirePoint;
		if (currentCamera == null || firePoint == null)
		{
			return;
		}
		Vector3 forward = currentCamera.transform.forward;
		if (CheatState.Aimbot)
		{
			TargetSolution target = (CurrentTarget = _targetingSystem.Acquire(currentCamera));
			if (target != null && target.Transform != null)
			{
				forward = (target.AimPoint - firePoint.position).normalized;
			}
		}
		if (forward.sqrMagnitude > 0.0001f)
		{
			firePoint.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
	}

	private static void ApplyInfiniteAmmo(object weapon)
	{
		FieldInfo fieldInfo = GameBinder.Field("Weapon", "<Ammo>k__BackingField");
		if (!(fieldInfo == null))
		{
			int num = Mathf.Max(1, CheatState.AmmoTopUp);
			if (GameBinder.TryGet<int>(fieldInfo, weapon, out var value) && value < num)
			{
				GameBinder.TrySet(fieldInfo, weapon, num);
			}
		}
	}

	private static void Set(object target, string field, object value, string typeName = "Weapon")
	{
		if (typeName == "Weapon")
		{
			Remember(target, field);
		}
		FieldInfo fieldInfo = GameBinder.Field(typeName, field);
		if (fieldInfo != null)
		{
			GameBinder.TrySet(fieldInfo, target, value);
		}
	}

	internal static bool RefillAmmo()
	{
		object heldWeapon = HeldWeapon;
		if (heldWeapon == null)
		{
			return false;
		}
		FieldInfo fieldInfo = GameBinder.Field("Weapon", "<Ammo>k__BackingField");
		if (fieldInfo != null)
		{
			return GameBinder.TrySet(fieldInfo, heldWeapon, Mathf.Max(1, CheatState.AmmoTopUp));
		}
		return false;
	}

	internal static bool BuyBulletUpgrade()
	{
		object heldWeapon = HeldWeapon;
		if (heldWeapon == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("Server", "BuyBulletUpgrade", 1), Refs.Server, heldWeapon);
	}

	internal static bool BuyAttachment(byte index)
	{
		object heldWeapon = HeldWeapon;
		if (heldWeapon == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("Server", "BuyAttachment", 2), Refs.Server, heldWeapon, index);
	}

	private static void Remember(object weapon, string field)
	{
		if (!_originals.TryGetValue(weapon, out var value))
		{
			value = (_originals[weapon] = new Dictionary<string, object>());
		}
		if (value.ContainsKey(field))
		{
			return;
		}
		FieldInfo fieldInfo = GameBinder.Field("Weapon", field);
		if (fieldInfo == null)
		{
			return;
		}
		try
		{
			value[field] = fieldInfo.GetValue(weapon);
		}
		catch
		{
		}
	}

	internal static bool ResetHeldWeapon()
	{
		object heldWeapon = HeldWeapon;
		if (heldWeapon == null)
		{
			return false;
		}
		if (!_originals.TryGetValue(heldWeapon, out var value) || value.Count == 0)
		{
			return false;
		}
		foreach (KeyValuePair<string, object> item in value)
		{
			FieldInfo fieldInfo = GameBinder.Field("Weapon", item.Key);
			if (fieldInfo != null)
			{
				GameBinder.TrySet(fieldInfo, heldWeapon, item.Value);
			}
		}
		value.Clear();
		return true;
	}
}
