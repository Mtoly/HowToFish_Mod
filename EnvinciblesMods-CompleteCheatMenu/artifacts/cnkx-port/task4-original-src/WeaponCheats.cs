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

	private static float _nextTargetPreview;

	private static float _targetLockUntil;

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
			CurrentTarget = FindTarget(currentCamera);
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
			Target? target = (CurrentTarget = FindTarget(currentCamera));
			if (target.HasValue && target.Value.Transform != null)
			{
				forward = (target.Value.Transform.position + target.Value.Transform.up * CheatState.AimHeightOffset - firePoint.position).normalized;
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

	private static Target? FindTarget(Camera cam)
	{
		Vector3 position = cam.transform.position;
		Vector3 forward = cam.transform.forward;
		Target? best = null;
		Target? current = null;
		float bestScore = float.MaxValue;
		Transform currentTransform = CurrentTarget.HasValue ? CurrentTarget.Value.Transform : null;

		if (CheatState.AimAtCreatures)
		{
			List<AimTargetRegistry.Entry> creatures = AimTargetRegistry.Creatures;
			for (int i = creatures.Count - 1; i >= 0; i--)
			{
				AimTargetRegistry.Entry entry = creatures[i];
				Creature creature = entry.Creature;
				if (creature == null)
				{
					creatures.RemoveAt(i);
					continue;
				}
				string name = creature.name;
				if (entry.Name != name)
				{
					entry.RefreshName(name);
				}
				if (!creature.gameObject.activeInHierarchy || (CheatState.AimExcludeDead && creature.IsDead) || (CheatState.AimExcludeSeagulls && entry.IsSeagull))
				{
					continue;
				}
				Fish fish = creature as Fish;
				if (CheatState.AimExcludeHookedFish && fish != null && fish.AttachedRod != null)
				{
					continue;
				}
				EvaluateCandidate(position, forward, entry.Transform, entry.Name, entry.IsAlbatross, currentTransform, ref best, ref current, ref bestScore);
			}
		}

		if (CheatState.AimAtPlayers)
		{
			VisualCheats.Rescan(creatures: false, items: false, players: true, 0.15f);
			foreach (VisualCheats.EspTarget target in VisualCheats.Targets)
			{
				if (target.Kind == VisualCheats.EspKind.Player && target.Transform != null)
				{
					EvaluateCandidate(position, forward, target.Transform, target.Label, albatross: false, currentTransform, ref best, ref current, ref bestScore);
				}
			}
		}

		if (current.HasValue && Time.unscaledTime < _targetLockUntil)
		{
			return current;
		}
		if (best.HasValue && best.Value.Transform != currentTransform)
		{
			_targetLockUntil = Time.unscaledTime + CheatState.AimLockDuration;
		}
		return best;
	}

	private static void EvaluateCandidate(Vector3 origin, Vector3 forward, Transform transform, string name, bool albatross, Transform currentTransform, ref Target? best, ref Target? current, ref float bestScore)
	{
		Vector3 to = transform.position + transform.up * CheatState.AimHeightOffset - origin;
		float distance = to.magnitude;
		if (distance > CheatState.AimRange || distance < 0.5f)
		{
			return;
		}
		float angle = Vector3.Angle(forward, to);
		if (angle > CheatState.AimFov || (CheatState.AimRequireLineOfSight && !HasLineOfSight(origin, transform)))
		{
			return;
		}
		Target candidate = new Target { Transform = transform, Name = name, Angle = angle, Distance = distance };
		if (transform == currentTransform)
		{
			current = candidate;
		}
		float score = CheatState.AimPrioritizeAlbatross && albatross
			? angle * 0.2f
			: distance * CheatState.AimDistanceWeight + angle * CheatState.AimAngleWeight;
		if (currentTransform != null && transform != currentTransform)
		{
			score += CheatState.AimSwitchPenalty;
		}
		if (score < bestScore)
		{
			bestScore = score;
			best = candidate;
		}
	}

	private static bool HasLineOfSight(Vector3 from, Transform target)
	{
		Vector3 vector = target.position + target.up * CheatState.AimHeightOffset - from;
		float magnitude = vector.magnitude;
		if (!Physics.Raycast(from, vector.normalized, out var hitInfo, magnitude))
		{
			return true;
		}
		if (!(hitInfo.transform == target) && !hitInfo.transform.IsChildOf(target))
		{
			return target.IsChildOf(hitInfo.transform);
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
