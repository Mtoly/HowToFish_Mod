using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Targeting;
using CompleteCheatMenu.UI;
using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal static class EspRendererV2
{
	private sealed class GeometryEntry
	{
		internal Renderer[] Renderers;
		internal Animator Animator;
		internal float ExpiresAt;
	}

	private struct VisibilityEntry
	{
		internal bool Visible;
		internal float ExpiresAt;
	}

	private static readonly Color OccludedColor = new Color(0.72f, 0.34f, 0.78f, 0.95f);
	private static readonly Color ItemColor = new Color(0.85f, 0.78f, 0.35f, 0.95f);
	private static readonly Color ContainerColor = new Color(0.35f, 0.78f, 0.92f, 0.95f);
	private static readonly Color TargetColor = new Color(1f, 0.84f, 0.15f, 1f);
	private static readonly string[] CurrentHealthNames = { "Health", "CurrentHealth", "CurHealth", "_health", "_currentHealth", "_curHealth" };
	private static readonly string[] MaxHealthNames = { "MaxHealth", "MaximumHealth", "_maxHealth", "_maximumHealth" };
	private static readonly Vector3[] CornerBuffer = new Vector3[8];
	private static readonly HumanBodyBones[,] HumanoidSegments = new HumanBodyBones[,]
	{
		{ HumanBodyBones.Head, HumanBodyBones.Neck },
		{ HumanBodyBones.Neck, HumanBodyBones.Chest },
		{ HumanBodyBones.Chest, HumanBodyBones.Hips },
		{ HumanBodyBones.Chest, HumanBodyBones.LeftUpperArm },
		{ HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm },
		{ HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand },
		{ HumanBodyBones.Chest, HumanBodyBones.RightUpperArm },
		{ HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm },
		{ HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand },
		{ HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg },
		{ HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg },
		{ HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot },
		{ HumanBodyBones.Hips, HumanBodyBones.RightUpperLeg },
		{ HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg },
		{ HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot }
	};
	private static readonly Dictionary<int, GeometryEntry> GeometryCache = new Dictionary<int, GeometryEntry>();
	private static readonly Dictionary<int, VisibilityEntry> EspVisibilityCache = new Dictionary<int, VisibilityEntry>();
	private static GUIStyle _label;
	private static float _nextCachePrune;

	internal static void Draw()
	{
		if (Event.current == null || Event.current.type != EventType.Repaint)
		{
			return;
		}
		Camera camera = Refs.CurrentCamera;
		if (camera == null)
		{
			return;
		}
		EnsureStyle();
		EspSnapshot snapshot = EspSnapshotBuilder.Refresh(CheatState.EspCreatures, CheatState.EspItems, CheatState.EspPlayers);
		if (CheatState.EspAimCircle && CheatState.Aimbot)
		{
			GuiPrimitives.DrawCircle(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), Mathf.Max(1f, CheatState.AimScreenRadius), new Color(1f, 1f, 1f, 0.55f), 1f);
		}
		if (!CheatState.EspEnabled)
		{
			return;
		}
		float maxDistanceSquared = CheatState.EspDistance * CheatState.EspDistance;
		DrawList(snapshot.Creatures, camera, maxDistanceSquared);
		DrawList(snapshot.Players, camera, maxDistanceSquared);
		DrawList(snapshot.Items, camera, maxDistanceSquared);
		DrawList(snapshot.Containers, camera, maxDistanceSquared);
		PruneCaches();
	}

	private static void EnsureStyle()
	{
		if (_label == null)
		{
			_label = new GUIStyle(GUI.skin.label)
			{
				fontSize = 11,
				alignment = TextAnchor.MiddleCenter,
				wordWrap = false
			};
		}
	}

	private static void DrawList(IReadOnlyList<EntityRecord> records, Camera camera, float maxDistanceSquared)
	{
		for (int i = 0; i < records.Count; i++)
		{
			EntityRecord record = records[i];
			if (!record.IsAlive || ReferenceEquals(record.Entity, Refs.LocalPlayer))
			{
				continue;
			}
			Vector3 delta = record.Transform.position - camera.transform.position;
			if (delta.sqrMagnitude > maxDistanceSquared)
			{
				continue;
			}
			DrawRecord(record, camera, delta.magnitude);
		}
	}

	private static void DrawRecord(EntityRecord record, Camera camera, float distance)
	{
		GeometryEntry geometry = GetGeometry(record);
		Bounds bounds;
		bool hasBounds = TryResolveCachedBounds(geometry, out bounds);
		Rect rect;
		if (!(hasBounds ? TryProjectBounds(camera, bounds, out rect) : TryProjectFallback(camera, record.Transform, out rect)))
		{
			return;
		}
		if (!OverlapsScreen(rect))
		{
			return;
		}
		Vector3 focus = hasBounds ? bounds.center : record.Transform.position + record.Transform.up * 0.8f;
		bool visible = IsVisible(record, camera, focus);
		bool currentTarget = IsCurrentTarget(record);
		Color color = currentTarget ? TargetColor : ColorFor(record.Kind, visible);
		if (CheatState.EspBoxes)
		{
			GuiPrimitives.DrawBox(rect, color, currentTarget ? 3f : 1.5f);
		}
		if (CheatState.EspTracers)
		{
			GuiPrimitives.DrawLine(new Vector2(Screen.width * 0.5f, Screen.height), new Vector2(rect.center.x, rect.yMax), color, currentTarget ? 2f : 1f);
		}
		if (CheatState.EspSkeletons && (record.Kind == EntityKind.Player || record.Kind == EntityKind.Creature))
		{
			try
			{
				DrawSkeleton(camera, record.Transform, geometry.Animator, record.Kind == EntityKind.Player, color);
			}
			catch
			{
				DrawResolvedSkeleton(camera, record.Transform, record.Kind == EntityKind.Player, color);
			}
		}
		if (CheatState.EspHealthBars && TryReadHealth(record.Entity, out var currentHealth, out var maxHealth))
		{
			GuiPrimitives.DrawBar(new Rect(rect.x - 7f, rect.y, 4f, rect.height), currentHealth / maxHealth, Color.Lerp(Color.red, Color.green, Mathf.Clamp01(currentHealth / maxHealth)), new Color(0f, 0f, 0f, 0.75f));
		}
		if (CheatState.EspLabels)
		{
			string label = BuildLabel(record, distance, visible);
			GuiPrimitives.DrawLabel(new Rect(rect.center.x - 180f, rect.y - 20f, 360f, 18f), label, _label, color);
		}
	}

	internal static bool TryProjectBounds(Camera camera, Bounds bounds, out Rect rect)
	{
		rect = default(Rect);
		Vector3[] corners = GetCorners(bounds);
		float minX = float.PositiveInfinity;
		float minY = float.PositiveInfinity;
		float maxX = float.NegativeInfinity;
		float maxY = float.NegativeInfinity;
		for (int i = 0; i < corners.Length; i++)
		{
			Vector3 projected = camera.WorldToScreenPoint(corners[i]);
			if (projected.z <= 0.01f || !IsFinite(projected))
			{
				return false;
			}
			float guiY = Screen.height - projected.y;
			minX = Mathf.Min(minX, projected.x);
			maxX = Mathf.Max(maxX, projected.x);
			minY = Mathf.Min(minY, guiY);
			maxY = Mathf.Max(maxY, guiY);
		}
		if (maxX - minX < 1f || maxY - minY < 1f)
		{
			return false;
		}
		rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
		return true;
	}

	internal static Vector3[] GetCorners(Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		CornerBuffer[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
		CornerBuffer[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
		CornerBuffer[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
		CornerBuffer[3] = center + new Vector3(-extents.x, extents.y, extents.z);
		CornerBuffer[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
		CornerBuffer[5] = center + new Vector3(extents.x, -extents.y, extents.z);
		CornerBuffer[6] = center + new Vector3(extents.x, extents.y, -extents.z);
		CornerBuffer[7] = center + new Vector3(extents.x, extents.y, extents.z);
		return CornerBuffer;
	}

	internal static bool TryProjectFallback(Camera camera, Transform root, out Rect rect)
	{
		rect = default(Rect);
		if (root == null)
		{
			return false;
		}
		Vector3 bottom = camera.WorldToScreenPoint(root.position);
		Vector3 top = camera.WorldToScreenPoint(root.position + Vector3.up * 1.8f);
		if (bottom.z <= 0.01f || top.z <= 0.01f || !IsFinite(bottom) || !IsFinite(top))
		{
			return false;
		}
		float bottomY = Screen.height - bottom.y;
		float topY = Screen.height - top.y;
		float height = Mathf.Abs(bottomY - topY);
		if (height < 1f)
		{
			return false;
		}
		float width = height * 0.5f;
		float centerX = (bottom.x + top.x) * 0.5f;
		rect = new Rect(centerX - width * 0.5f, Mathf.Min(bottomY, topY), width, height);
		return true;
	}

	private static bool OverlapsScreen(Rect rect)
	{
		return rect.xMax >= 0f && rect.yMax >= 0f && rect.x <= Screen.width && rect.y <= Screen.height;
	}

	private static bool IsVisible(EntityRecord record, Camera camera, Vector3 point)
	{
		float now = Time.unscaledTime;
		if (EspVisibilityCache.TryGetValue(record.InstanceId, out var cached) && now <= cached.ExpiresAt)
		{
			return cached.Visible;
		}
		Transform target = record.Transform;
		Vector3 origin = camera.transform.position;
		Vector3 direction = point - origin;
		float distance = direction.magnitude;
		if (distance <= 0.01f)
		{
			return CacheVisibility(record.InstanceId, true, now);
		}
		if (!Physics.Raycast(origin, direction / distance, out var hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
		{
			return CacheVisibility(record.InstanceId, true, now);
		}
		Transform hitTransform = hit.transform;
		bool visible = hitTransform == target || (hitTransform != null && (hitTransform.IsChildOf(target) || target.IsChildOf(hitTransform)));
		return CacheVisibility(record.InstanceId, visible, now);
	}

	private static bool CacheVisibility(int instanceId, bool visible, float now)
	{
		EspVisibilityCache[instanceId] = new VisibilityEntry
		{
			Visible = visible,
			ExpiresAt = now + 0.05f
		};
		return visible;
	}

	private static bool IsCurrentTarget(EntityRecord record)
	{
		TargetSolution current = WeaponCheats.CurrentTarget;
		if (current == null)
		{
			return false;
		}
		return ReferenceEquals(current.Entity, record.Entity) || current.Transform == record.Transform || (current.Transform != null && (current.Transform.IsChildOf(record.Transform) || record.Transform.IsChildOf(current.Transform)));
	}

	private static Color ColorFor(EntityKind kind, bool visible)
	{
		if (!visible)
		{
			return OccludedColor;
		}
		return kind switch
		{
			EntityKind.Creature => Theme.Accent,
			EntityKind.Player => Theme.Danger,
			EntityKind.Container => ContainerColor,
			_ => ItemColor
		};
	}

	private static void DrawSkeleton(Camera camera, Transform root, Animator animator, bool player, Color color)
	{
		if (player && animator != null && animator.isHuman && animator.avatar != null && animator.avatar.isValid)
		{
			DrawHumanoidSkeleton(camera, animator, color);
			return;
		}
		DrawResolvedSkeleton(camera, root, player, color);
	}

	private static void DrawResolvedSkeleton(Camera camera, Transform root, bool player, Color color)
	{
		List<AimPoint> points = BoneResolver.Resolve(root, player, 0.8f);
		Vector3? head = FindPoint(points, "head");
		Vector3? chest = FindPoint(points, "chest");
		Vector3? pelvis = FindPoint(points, "pelvis");
		if (head.HasValue && chest.HasValue)
		{
			DrawWorldLine(camera, head.Value, chest.Value, color);
		}
		if (chest.HasValue && pelvis.HasValue)
		{
			DrawWorldLine(camera, chest.Value, pelvis.Value, color);
		}
		if (pelvis.HasValue)
		{
			DrawWorldLine(camera, pelvis.Value, root.position, color);
		}
	}

	private static void DrawHumanoidSkeleton(Camera camera, Animator animator, Color color)
	{
		for (int i = 0; i < HumanoidSegments.GetLength(0); i++)
		{
			Transform from = animator.GetBoneTransform(HumanoidSegments[i, 0]);
			Transform to = animator.GetBoneTransform(HumanoidSegments[i, 1]);
			if (from != null && to != null)
			{
				DrawWorldLine(camera, from.position, to.position, color);
			}
		}
	}

	private static Vector3? FindPoint(List<AimPoint> points, string token)
	{
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].Bone.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return points[i].Position;
			}
		}
		return null;
	}

	private static void DrawWorldLine(Camera camera, Vector3 from, Vector3 to, Color color)
	{
		Vector3 screenFrom = camera.WorldToScreenPoint(from);
		Vector3 screenTo = camera.WorldToScreenPoint(to);
		if (screenFrom.z <= 0.01f || screenTo.z <= 0.01f)
		{
			return;
		}
		GuiPrimitives.DrawLine(new Vector2(screenFrom.x, Screen.height - screenFrom.y), new Vector2(screenTo.x, Screen.height - screenTo.y), color, 1f);
	}

	private static bool TryReadHealth(object entity, out float current, out float maximum)
	{
		current = 0f;
		maximum = 0f;
		if (entity == null)
		{
			return false;
		}
		object source = ReadMember(entity, "Vitals") ?? entity;
		return TryReadNumber(source, CurrentHealthNames, out current) && TryReadNumber(source, MaxHealthNames, out maximum) && maximum > 0f && current >= 0f;
	}

	private static string BuildLabel(EntityRecord record, float distance, bool visible)
	{
		string label = record.Name;
		if (CheatState.EspTypeInfo && !string.Equals(record.Name, record.TypeName, StringComparison.OrdinalIgnoreCase))
		{
			label += " [" + record.TypeName + "]";
		}
		if (CheatState.EspDistanceLabels)
		{
			label += $"  {distance:0.0}m";
		}
		if (CheatState.EspVisibilityInfo)
		{
			label += visible ? "  可见" : "  遮挡";
		}
		string details = BuildDetails(record);
		return string.IsNullOrEmpty(details) ? label : label + "  " + details;
	}

	private static string BuildDetails(EntityRecord record)
	{
		try
		{
			if (record.Kind == EntityKind.Player && CheatState.EspHeldItemInfo)
			{
				object holding = ReadMember(record.Entity, "Holding");
				object held = ReadMember(holding, "HeldItem");
				if (Refs.IsAlive(held))
				{
					return "手持: " + ObjectName(held);
				}
			}
			if (record.Kind == EntityKind.Item && CheatState.EspItemValueInfo && record.Entity is Item)
			{
				int worth = ItemCheats.TotalWorth(record.Entity);
				if (worth <= 0)
				{
					worth = ItemCheats.Worth(record.Entity);
				}
				if (worth > 0)
				{
					return "价值: " + worth;
				}
			}
			if (record.Kind == EntityKind.Container && CheatState.EspContainerInfo)
			{
				object contents = ReadMember(record.Entity, "Items") ?? ReadMember(record.Entity, "Inventory") ?? ReadMember(record.Entity, "Contents");
				if (contents is ICollection collection)
				{
					return "容器: " + collection.Count + " 项";
				}
			}
		}
		catch
		{
		}
		return string.Empty;
	}

	private static string ObjectName(object value)
	{
		if (value is UnityEngine.Object obj && !string.IsNullOrEmpty(obj.name))
		{
			return obj.name;
		}
		return value?.GetType().Name ?? string.Empty;
	}

	private static object ReadMember(object instance, string name)
	{
		if (instance == null)
		{
			return null;
		}
		Type type = instance.GetType();
		try
		{
			PropertyInfo property = type.GetProperty(name, GameBinder.Any);
			if (property != null && property.GetIndexParameters().Length == 0)
			{
				return property.GetValue(instance);
			}
			FieldInfo field = type.GetField(name, GameBinder.Any);
			return field?.GetValue(instance);
		}
		catch
		{
			return null;
		}
	}

	private static bool TryReadNumber(object instance, string[] names, out float value)
	{
		value = 0f;
		for (int i = 0; i < names.Length; i++)
		{
			object raw = ReadMember(instance, names[i]);
			if (raw == null)
			{
				continue;
			}
			object unwrapped = ReadMember(raw, "Value") ?? raw;
			try
			{
				value = Convert.ToSingle(unwrapped);
				return !float.IsNaN(value) && !float.IsInfinity(value);
			}
			catch
			{
			}
		}
		return false;
	}

	private static bool IsFinite(Vector3 value)
	{
		return !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
	}

	private static GeometryEntry GetGeometry(EntityRecord record)
	{
		float now = Time.unscaledTime;
		if (GeometryCache.TryGetValue(record.InstanceId, out var entry) && now <= entry.ExpiresAt)
		{
			return entry;
		}
		Renderer[] all = record.Transform.GetComponentsInChildren<Renderer>(includeInactive: false);
		List<Renderer> usable = null;
		for (int i = 0; i < all.Length; i++)
		{
			Renderer renderer = all[i];
			if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy || IsEffectRenderer(renderer))
			{
				continue;
			}
			if (usable == null)
			{
				usable = new List<Renderer>(all.Length);
			}
			usable.Add(renderer);
		}
		entry = new GeometryEntry
		{
			Renderers = usable?.ToArray() ?? Array.Empty<Renderer>(),
			Animator = record.Kind == EntityKind.Player ? record.Transform.GetComponentInChildren<Animator>(includeInactive: false) : null,
			ExpiresAt = now + 1f
		};
		GeometryCache[record.InstanceId] = entry;
		return entry;
	}

	private static bool IsEffectRenderer(Renderer renderer)
	{
		string typeName = renderer.GetType().Name;
		return typeName == "ParticleSystemRenderer" || typeName == "TrailRenderer" || typeName == "LineRenderer";
	}

	private static bool TryResolveCachedBounds(GeometryEntry geometry, out Bounds bounds)
	{
		bounds = default(Bounds);
		bool found = false;
		Renderer[] renderers = geometry.Renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer renderer = renderers[i];
			if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
			{
				continue;
			}
			Bounds current = renderer.bounds;
			if (!IsFinite(current.center) || !IsFinite(current.size) || current.size.sqrMagnitude <= 0.000001f)
			{
				continue;
			}
			if (!found)
			{
				bounds = current;
				found = true;
			}
			else
			{
				bounds.Encapsulate(current);
			}
		}
		return found;
	}

	private static void PruneCaches()
	{
		float now = Time.unscaledTime;
		if (now < _nextCachePrune)
		{
			return;
		}
		_nextCachePrune = now + 2f;
		List<int> expired = null;
		foreach (KeyValuePair<int, GeometryEntry> pair in GeometryCache)
		{
			if (now > pair.Value.ExpiresAt + 2f)
			{
				(expired ??= new List<int>()).Add(pair.Key);
			}
		}
		if (expired != null)
		{
			for (int i = 0; i < expired.Count; i++)
			{
				GeometryCache.Remove(expired[i]);
				EspVisibilityCache.Remove(expired[i]);
			}
		}
	}
}
