using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using CompleteCheatMenu.Targeting;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class WeaponCheats
{
	private static readonly WeaponStateStore _stateStore = new WeaponStateStore();

	private static object _lastWeapon;

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
		get { return _stateStore.HasOriginals(HeldWeapon); }
	}

	internal static void Tick()
	{
		object heldWeapon = HeldWeapon;
		RestorePreviousWeapon(heldWeapon);
		RestoreDisabledFields();
		if (heldWeapon != null)
		{
			if (CheatState.InfiniteAmmo)
			{
				ApplyInfiniteAmmo(heldWeapon);
			}
			if (CheatState.NoSpread)
			{
				SetExact(heldWeapon, "_spread", 0f);
			}
			if (CheatState.ZeroRecoil)
			{
				SetExact(heldWeapon, "_recoilKnockback", 0);
			}
			if (CheatState.RapidFire)
			{
				SetExact(heldWeapon, "_timeBetweenShots", Mathf.Max(0.01f, CheatState.FireInterval));
			}
			if (CheatState.MultiShot)
			{
				SetExact(heldWeapon, "_projectileCountPerShot", Mathf.RoundToInt(CheatState.ProjectileCount));
			}
			if (CheatState.InstantHit)
			{
				SetExact(heldWeapon, "_projSpeed", Mathf.Max(1f, CheatState.InstantHitProjectileSpeed));
			}
			else if (CheatState.FastBullets)
			{
				SetExact(heldWeapon, "_projSpeed", Mathf.Max(1f, CheatState.ProjectileSpeed));
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

	internal static void PrepareShot()
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
		if (CheatState.FireWhereLooking)
		{
			AlignFirePointToView(currentCamera, firePoint);
		}
		if (CheatState.Aimbot)
		{
			TargetSolution solution;
			if (ShotRedirector.TryRedirect(_targetingSystem, currentCamera, firePoint, ResolveProjectileSpeed(HeldWeapon), out solution))
			{
				CurrentTarget = solution;
			}
		}
	}

	internal static void VisibleAimTick(float deltaTime)
	{
		if (!CheatState.AimVisibleAssist || !CheatState.Aimbot)
		{
			return;
		}
		VisibleAimController.Step(HeldWeapon, Refs.LocalCamera, Refs.CurrentCamera, CurrentTarget, deltaTime);
	}

	private static void AlignFirePointToView(Camera camera, Transform firePoint)
	{
		Vector3 forward = camera.transform.forward;
		if (forward.sqrMagnitude > 0.0001f)
		{
			firePoint.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
	}

	private static float ResolveProjectileSpeed(object weapon)
	{
		if (CheatState.InstantHit)
		{
			return CheatState.InstantHitProjectileSpeed;
		}
		if (CheatState.FastBullets)
		{
			return CheatState.ProjectileSpeed;
		}
		if (weapon != null && GameBinder.TryGet<float>(GameBinder.Field("Weapon", "_projSpeed"), weapon, out var speed))
		{
			return speed;
		}
		return CheatState.ProjectileSpeed;
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

	private static bool SetExact(object target, string fieldName, object value)
	{
		FieldInfo field = GameBinder.Field("Weapon", fieldName);
		return _stateStore.Set(target, field, value);
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

	private static void RestorePreviousWeapon(object currentWeapon)
	{
		if (_lastWeapon != null && !ReferenceEquals(_lastWeapon, currentWeapon))
		{
			_stateStore.RestoreWeapon(_lastWeapon);
		}
		_lastWeapon = currentWeapon;
	}

	private static void RestoreDisabledFields()
	{
		if (!CheatState.NoSpread)
		{
			_stateStore.RestoreFieldAll(GameBinder.Field("Weapon", "_spread"));
		}
		if (!CheatState.ZeroRecoil)
		{
			_stateStore.RestoreFieldAll(GameBinder.Field("Weapon", "_recoilKnockback"));
		}
		if (!CheatState.RapidFire)
		{
			_stateStore.RestoreFieldAll(GameBinder.Field("Weapon", "_timeBetweenShots"));
		}
		if (!CheatState.MultiShot)
		{
			_stateStore.RestoreFieldAll(GameBinder.Field("Weapon", "_projectileCountPerShot"));
		}
		if (!CheatState.FastBullets && !CheatState.InstantHit)
		{
			_stateStore.RestoreFieldAll(GameBinder.Field("Weapon", "_projSpeed"));
		}
	}

	internal static bool ResetHeldWeapon()
	{
		object heldWeapon = HeldWeapon;
		return _stateStore.RestoreWeapon(heldWeapon);
	}

	internal static void RestoreAllWeapons()
	{
		_stateStore.RestoreAll();
		_lastWeapon = null;
	}
}
