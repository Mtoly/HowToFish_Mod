using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class ProjectileGuidance
{
	private sealed class GuidanceState
	{
		internal float StartTime;
		internal float NextUpdate;
		internal Vector3 AimOffset;
		internal TargetSolution Target;
	}

	private static readonly Dictionary<Projectile, GuidanceState> States = new Dictionary<Projectile, GuidanceState>();

	private static readonly VelocityTracker TargetVelocity = new VelocityTracker();

	internal static int ActiveCount => States.Count;

	internal static long GuidedUpdateCount { get; private set; }

	internal static long ExpiredCount { get; private set; }

	internal static long IntervalSkipCount { get; private set; }

	internal static bool Step(Projectile projectile, float now, float deltaTime)
	{
		if (projectile == null)
		{
			return false;
		}
		if (!CheatState.Aimbot || CheatState.AimProjectileTrackingTime <= 0f)
		{
			Forget(projectile);
			return false;
		}
		if (!ProjectileTracker.TryGetTarget(projectile, out TargetSolution target))
		{
			Forget(projectile);
			return false;
		}
		if (!States.TryGetValue(projectile, out GuidanceState state) || !ReferenceEquals(state.Target, target))
		{
			state = new GuidanceState
			{
				StartTime = now,
				NextUpdate = now,
				AimOffset = target.AimPoint - target.Transform.position,
				Target = target
			};
			States[projectile] = state;
		}
		if (now - state.StartTime > Mathf.Max(0f, CheatState.AimProjectileTrackingTime))
		{
			ExpiredCount++;
			ProjectileTracker.Unregister(projectile);
			Forget(projectile);
			return false;
		}
		if (now + 0.0001f < state.NextUpdate)
		{
			IntervalSkipCount++;
			return false;
		}
		state.NextUpdate = now + Mathf.Max(0f, CheatState.AimProjectileUpdateInterval);
		Vector3 point = target.Transform.position + state.AimOffset;
		Vector3 targetVelocity = ResolveTargetVelocity(target, point, now);
		if (!ProjectileGuidanceMath.TrySteer(projectile.Position, projectile.Velocity, point, targetVelocity, projectile.GravityForce, CheatState.AimPrediction, CheatState.AimGravityCompensation, CheatState.AimMaxPredictionTime, CheatState.AimProjectileTurnSpeed, deltaTime, out Vector3 steered))
		{
			return false;
		}
		projectile.Velocity = steered;
		GuidedUpdateCount++;
		return true;
	}

	internal static void Forget(Projectile projectile)
	{
		if (projectile != null)
		{
			States.Remove(projectile);
		}
	}

	internal static void Clear()
	{
		States.Clear();
		TargetVelocity.Clear();
	}

	private static Vector3 ResolveTargetVelocity(TargetSolution target, Vector3 point, float now)
	{
		try
		{
			Rigidbody rigidbody = target.Transform.GetComponentInParent<Rigidbody>();
			if (rigidbody != null)
			{
				return BallisticPredictor.SanitizeVelocity(rigidbody.GetPointVelocity(point), CheatState.AimMaxTargetSpeed);
			}
		}
		catch
		{
		}
		return TargetVelocity.Observe(RuntimeHelpers.GetHashCode(target.Entity), point, now);
	}
}
