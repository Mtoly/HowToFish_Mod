using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class WeaponCheats
{
	internal struct Target
	{
		internal Transform Transform;

		internal string Name;

		internal float Angle;

		internal float Distance;
	}

	private static readonly Dictionary<object, Dictionary<string, object>> _originals = new Dictionary<object, Dictionary<string, object>>();

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

	internal static string WeaponName
	{
		get
		{
			object heldItem = ItemCheats.HeldItem;
			object obj = ((heldItem is Object) ? heldItem : null);
			return ((obj != null) ? ((Object)obj).name : null) ?? "-";
		}
	}

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

	internal static Target? CurrentTarget { get; private set; }

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
				object? obj = propertyInfo?.GetValue(attachments);
				return (Transform)((obj is Transform) ? obj : null);
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
			}
			else
			{
				UpdateTargetPreview();
			}
		}
	}

	private static void UpdateTargetPreview()
	{
		Camera currentCamera = Refs.CurrentCamera;
		if ((Object)(object)currentCamera == (Object)null)
		{
			CurrentTarget = null;
		}
		else
		{
			CurrentTarget = FindTarget(currentCamera);
		}
	}

	internal static void AimBeforeShot()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!CheatState.Aimbot && !CheatState.FireWhereLooking)
		{
			return;
		}
		Camera currentCamera = Refs.CurrentCamera;
		Transform firePoint = FirePoint;
		if ((Object)(object)currentCamera == (Object)null || (Object)(object)firePoint == (Object)null)
		{
			return;
		}
		Vector3 val = ((Component)currentCamera).transform.forward;
		if (CheatState.Aimbot)
		{
			Target? target = (CurrentTarget = FindTarget(currentCamera));
			if (target.HasValue && (Object)(object)target.Value.Transform != (Object)null)
			{
				Vector3 val2 = target.Value.Transform.position + Vector3.up * CheatState.AimHeightOffset - firePoint.position;
				val = ((Vector3)(ref val2)).normalized;
			}
		}
		if (((Vector3)(ref val)).sqrMagnitude > 0.0001f)
		{
			firePoint.rotation = Quaternion.LookRotation(val, Vector3.up);
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

	private static Target? FindTarget(Camera cam)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		VisualCheats.Rescan(CheatState.AimAtCreatures, items: false, CheatState.AimAtPlayers, 0.15f);
		Vector3 position = ((Component)cam).transform.position;
		Vector3 forward = ((Component)cam).transform.forward;
		Target? result = null;
		float num = CheatState.AimFov;
		foreach (VisualCheats.EspTarget target in VisualCheats.Targets)
		{
			if ((Object)(object)target.Transform == (Object)null || target.Kind == VisualCheats.EspKind.Item || (target.Kind == VisualCheats.EspKind.Creature && !CheatState.AimAtCreatures) || (target.Kind == VisualCheats.EspKind.Player && !CheatState.AimAtPlayers))
			{
				continue;
			}
			Vector3 val = target.Transform.position + Vector3.up * CheatState.AimHeightOffset - position;
			float magnitude = ((Vector3)(ref val)).magnitude;
			if (!(magnitude > CheatState.AimRange) && !(magnitude < 0.5f))
			{
				float num2 = Vector3.Angle(forward, val);
				if (!(num2 > num) && (!CheatState.AimRequireLineOfSight || HasLineOfSight(position, target.Transform)))
				{
					num = num2;
					result = new Target
					{
						Transform = target.Transform,
						Name = target.Label,
						Angle = num2,
						Distance = magnitude
					};
				}
			}
		}
		return result;
	}

	private static bool HasLineOfSight(Vector3 from, Transform target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = target.position + Vector3.up * CheatState.AimHeightOffset - from;
		float magnitude = ((Vector3)(ref val)).magnitude;
		RaycastHit val2 = default(RaycastHit);
		if (!Physics.Raycast(from, ((Vector3)(ref val)).normalized, ref val2, magnitude))
		{
			return true;
		}
		if (!((Object)(object)((RaycastHit)(ref val2)).transform == (Object)(object)target) && !((RaycastHit)(ref val2)).transform.IsChildOf(target))
		{
			return target.IsChildOf(((RaycastHit)(ref val2)).transform);
		}
		return true;
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
