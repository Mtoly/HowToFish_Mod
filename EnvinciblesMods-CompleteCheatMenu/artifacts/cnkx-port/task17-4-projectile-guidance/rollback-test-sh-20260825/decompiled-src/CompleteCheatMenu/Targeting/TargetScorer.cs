using System.Collections.Generic;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class TargetScorer
{
	internal static bool InsideScreenRadius(Vector2 position, Vector2 center, float radius)
	{
		if (radius < 0f)
		{
			return false;
		}
		Vector2 delta = position - center;
		return delta.sqrMagnitude <= radius * radius;
	}

	internal static float ScoreEntity(TargetCandidate candidate, TargetingSettings settings, object currentEntity)
	{
		if (candidate == null || settings == null)
		{
			return float.PositiveInfinity;
		}
		float score = candidate.Distance * Mathf.Max(0f, settings.DistanceWeight)
			+ candidate.Angle * Mathf.Max(0f, settings.AngleWeight)
			+ candidate.ScreenDistance * Mathf.Max(0f, settings.ScreenDistanceWeight);
		if (currentEntity != null && !ReferenceEquals(candidate.Entity, currentEntity))
		{
			score += Mathf.Max(0f, settings.SwitchPenalty);
		}
		if (candidate.Priority)
		{
			score -= Mathf.Clamp(settings.PriorityBonus, 0f, settings.PriorityBonusLimit);
		}
		candidate.EntityScore = score;
		return score;
	}

	internal static TargetCandidate SelectBest(IReadOnlyList<TargetCandidate> candidates, TargetingSettings settings, object currentEntity)
	{
		if (candidates == null || settings == null)
		{
			return null;
		}
		TargetCandidate best = null;
		float bestScore = float.PositiveInfinity;
		for (int i = 0; i < candidates.Count; i++)
		{
			TargetCandidate candidate = candidates[i];
			if (!IsEligible(candidate, settings))
			{
				continue;
			}
			float score = ScoreEntity(candidate, settings, currentEntity);
			if (score < bestScore)
			{
				bestScore = score;
				best = candidate;
			}
		}
		return best;
	}

	private static bool IsEligible(TargetCandidate candidate, TargetingSettings settings)
	{
		if (candidate == null || candidate.Entity == null || candidate.Transform == null)
		{
			return false;
		}
		if (candidate.Distance < 0f || candidate.Distance > settings.MaxDistance)
		{
			return false;
		}
		if (candidate.Angle < 0f || candidate.Angle > settings.MaxAngle)
		{
			return false;
		}
		if (settings.RequireLineOfSight && !candidate.Visible)
		{
			return false;
		}
		return candidate.ScreenDistance <= settings.ScreenRadius;
	}
}
