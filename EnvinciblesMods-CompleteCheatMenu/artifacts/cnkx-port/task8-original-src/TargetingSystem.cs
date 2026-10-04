using System;
using System.Collections.Generic;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal sealed class TargetingSystem
{
	private readonly TargetingSnapshot _snapshot = new TargetingSnapshot();
	private readonly VisibilityCache _visibility = new VisibilityCache();
	private readonly List<TargetCandidate> _candidates = new List<TargetCandidate>(64);
	private TargetSolution _current;
	private float _lockUntil;

	internal TargetSolution Current => IsSolutionAlive(_current) ? _current : null;

	internal TargetSolution Acquire(Camera camera)
	{
		if (camera == null)
		{
			Clear();
			return null;
		}
		TargetingSettings settings = TargetingSettings.FromCheatState();
		_snapshot.Refresh(CheatState.AimAtCreatures, CheatState.AimAtPlayers);
		_candidates.Clear();
		Vector3 origin = camera.transform.position;
		Vector3 forward = camera.transform.forward;
		Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
		float now = Time.unscaledTime;

		if (CheatState.AimAtCreatures)
		{
			CollectCreatures(camera, origin, forward, screenCenter, settings, now);
		}
		if (CheatState.AimAtPlayers)
		{
			CollectPlayers(camera, origin, forward, screenCenter, settings, now);
		}

		TargetCandidate currentCandidate = FindCurrentCandidate();
		if (currentCandidate == null)
		{
			_current = null;
			_lockUntil = 0f;
		}
		else if (now < _lockUntil)
		{
			TargetScorer.ScoreEntity(currentCandidate, settings, currentCandidate.Entity);
			_current = TargetSolution.FromCandidate(currentCandidate, now);
			return _current;
		}

		object currentEntity = currentCandidate?.Entity;
		TargetCandidate best = TargetScorer.SelectBest(_candidates, settings, currentEntity);
		if (best == null)
		{
			_current = null;
			_lockUntil = 0f;
			return null;
		}
		if (currentEntity == null || !ReferenceEquals(best.Entity, currentEntity))
		{
			_lockUntil = now + Mathf.Max(0f, CheatState.AimLockDuration);
		}
		_current = TargetSolution.FromCandidate(best, now);
		return _current;
	}

	internal void Clear()
	{
		_current = null;
		_lockUntil = 0f;
		_candidates.Clear();
		_visibility.Clear();
	}

	private void CollectCreatures(Camera camera, Vector3 origin, Vector3 forward, Vector2 screenCenter, TargetingSettings settings, float now)
	{
		IReadOnlyList<EntityRecord> creatures = _snapshot.Creatures;
		for (int i = 0; i < creatures.Count; i++)
		{
			EntityRecord record = creatures[i];
			Creature creature = record.Entity as Creature;
			if (creature == null || !record.IsAlive || !creature.gameObject.activeInHierarchy)
			{
				continue;
			}
			string name = creature.name;
			if (record.Name != name)
			{
				record.RefreshName(name);
			}
			bool seagull = record.Name.IndexOf("seagull", StringComparison.OrdinalIgnoreCase) >= 0;
			bool albatross = record.Name.IndexOf("albatross", StringComparison.OrdinalIgnoreCase) >= 0;
			if ((CheatState.AimExcludeDead && creature.IsDead) || (CheatState.AimExcludeSeagulls && seagull))
			{
				continue;
			}
			Fish fish = creature as Fish;
			if (CheatState.AimExcludeHookedFish && fish != null && fish.AttachedRod != null)
			{
				continue;
			}
			AddCandidate(record, player: false, priority: CheatState.AimPrioritizeAlbatross && albatross, camera, origin, forward, screenCenter, settings, now);
		}
	}

	private void CollectPlayers(Camera camera, Vector3 origin, Vector3 forward, Vector2 screenCenter, TargetingSettings settings, float now)
	{
		IReadOnlyList<EntityRecord> players = _snapshot.Players;
		object localPlayer = Refs.LocalPlayer;
		for (int i = 0; i < players.Count; i++)
		{
			EntityRecord record = players[i];
			if (ReferenceEquals(record.Entity, localPlayer) || !record.IsAlive || !record.Transform.gameObject.activeInHierarchy)
			{
				continue;
			}
			AddCandidate(record, player: true, priority: false, camera, origin, forward, screenCenter, settings, now);
		}
	}

	private void AddCandidate(EntityRecord record, bool player, bool priority, Camera camera, Vector3 origin, Vector3 forward, Vector2 screenCenter, TargetingSettings settings, float now)
	{
		List<AimPoint> points = BoneResolver.Resolve(record.Transform, player, CheatState.AimHeightOffset);
		AimPoint point = AimPoint.SelectClosestProjected(points, camera, screenCenter);
		if (point == null)
		{
			return;
		}
		Vector3 direction = point.Position - origin;
		float distance = direction.magnitude;
		if (distance < 0.5f || distance > settings.MaxDistance)
		{
			return;
		}
		float angle = Vector3.Angle(forward, direction);
		if (angle > settings.MaxAngle || !TargetScorer.InsideScreenRadius(point.ScreenPosition, screenCenter, settings.ScreenRadius))
		{
			return;
		}
		bool visible = !settings.RequireLineOfSight || _visibility.IsVisible(record.InstanceId, record.Transform, point, origin, now);
		if (settings.RequireLineOfSight && !visible)
		{
			return;
		}
		_candidates.Add(new TargetCandidate
		{
			Record = record,
			Entity = record.Entity,
			Transform = record.Transform,
			AimPoint = point.Position,
			Bone = point.Bone,
			Distance = distance,
			Angle = angle,
			ScreenPosition = point.ScreenPosition,
			ScreenDistance = point.ScreenDistance,
			Visible = visible,
			Priority = priority,
			BoneScore = point.ScreenDistance
		});
	}

	private TargetCandidate FindCurrentCandidate()
	{
		if (!IsSolutionAlive(_current))
		{
			return null;
		}
		for (int i = 0; i < _candidates.Count; i++)
		{
			if (ReferenceEquals(_candidates[i].Entity, _current.Entity))
			{
				return _candidates[i];
			}
		}
		return null;
	}

	private static bool IsSolutionAlive(TargetSolution solution)
	{
		return solution != null && Refs.IsAlive(solution.Entity) && solution.Transform != null;
	}
}
