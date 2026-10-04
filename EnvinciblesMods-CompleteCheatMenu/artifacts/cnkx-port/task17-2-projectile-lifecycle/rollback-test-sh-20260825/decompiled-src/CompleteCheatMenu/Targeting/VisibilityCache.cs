using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal sealed class VisibilityCache
{
	private struct CacheKey : IEquatable<CacheKey>
	{
		internal int InstanceId;
		internal string Point;

		public bool Equals(CacheKey other)
		{
			return InstanceId == other.InstanceId && string.Equals(Point, other.Point, StringComparison.Ordinal);
		}

		public override bool Equals(object obj)
		{
			return obj is CacheKey && Equals((CacheKey)obj);
		}

		public override int GetHashCode()
		{
			return InstanceId * 397 ^ (Point?.GetHashCode() ?? 0);
		}
	}

	private struct CacheEntry
	{
		internal bool Visible;
		internal float ExpiresAt;
	}

	private readonly Dictionary<CacheKey, CacheEntry> _entries = new Dictionary<CacheKey, CacheEntry>();
	private readonly float _ttl;
	private float _nextPrune;

	internal VisibilityCache(float ttl = 0.05f)
	{
		_ttl = Mathf.Max(0.01f, ttl);
	}

	internal bool IsVisible(int instanceId, Transform target, AimPoint point, Vector3 origin, float now)
	{
		if (target == null || point == null)
		{
			return false;
		}
		CacheKey key = new CacheKey
		{
			InstanceId = instanceId,
			Point = point.Bone + ":" + point.Source
		};
		if (_entries.TryGetValue(key, out var cached) && now <= cached.ExpiresAt)
		{
			return cached.Visible;
		}
		bool visible = RaycastVisible(origin, point.Position, target);
		_entries[key] = new CacheEntry
		{
			Visible = visible,
			ExpiresAt = now + _ttl
		};
		if (now >= _nextPrune)
		{
			Prune(now);
			_nextPrune = now + 1f;
		}
		return visible;
	}

	internal void Clear()
	{
		_entries.Clear();
		_nextPrune = 0f;
	}

	private static bool RaycastVisible(Vector3 origin, Vector3 point, Transform target)
	{
		Vector3 direction = point - origin;
		float distance = direction.magnitude;
		if (distance <= 0.0001f || !Physics.Raycast(origin, direction / distance, out var hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
		{
			return true;
		}
		Transform hitTransform = hit.transform;
		if (hitTransform == null)
		{
			return false;
		}
		return hitTransform == target || hitTransform.IsChildOf(target) || target.IsChildOf(hitTransform);
	}

	private void Prune(float now)
	{
		List<CacheKey> expired = null;
		foreach (KeyValuePair<CacheKey, CacheEntry> pair in _entries)
		{
			if (pair.Value.ExpiresAt < now)
			{
				if (expired == null)
				{
					expired = new List<CacheKey>();
				}
				expired.Add(pair.Key);
			}
		}
		if (expired == null)
		{
			return;
		}
		for (int i = 0; i < expired.Count; i++)
		{
			_entries.Remove(expired[i]);
		}
	}
}
