using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal static class ProjectileGuidance
{
	private enum GuidanceMode
	{
		Tracking,
		Returning,
		NaturalFlight
	}

	private sealed class GuidanceState
	{
		internal float StartTime;
		internal float NextUpdate;
		internal float InvalidatedAt;
		internal Vector3 AimOffset;
		internal Vector3 RawVelocity;
		internal Vector3 LaunchVelocity;
		internal Vector3 ReturnStartVelocity;
		internal TargetSolution Target;
		internal GuidanceMode Mode;
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
		if (!States.TryGetValue(projectile, out GuidanceState state))
		{
			state = new GuidanceState
			{
				StartTime = now,
				NextUpdate = now,
				Mode = GuidanceMode.Tracking,
				RawVelocity = projectile.Velocity,
				LaunchVelocity = projectile.Velocity
			};
			if (ProjectileTracker.TryGetLaunchData(projectile, out ProjectileLaunchData launchData))
			{
				state.RawVelocity = launchData.RawVelocity;
				state.LaunchVelocity = launchData.LaunchVelocity;
			}
			States[projectile] = state;
		}
		if (state.Mode == GuidanceMode.Returning)
		{
			return StepReturn(projectile, state, now, deltaTime);
		}
		if (state.Mode == GuidanceMode.NaturalFlight)
		{
			Forget(projectile);
			return false;
		}
		if (!ProjectileTracker.TryGetTarget(projectile, out TargetSolution target))
		{
			return Invalidate(projectile, state, now, deltaTime);
		}
		if (!ReferenceEquals(state.Target, target))
		{
			state.AimOffset = target.AimPoint - target.Transform.position;
			state.Target = target;
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

	private static bool Invalidate(Projectile projectile, GuidanceState state, float now, float deltaTime)
	{
		state.InvalidatedAt = now;
		state.Target = null;
		if (CheatState.AimReturnToLaunchVelocity && CheatState.AimReturnDuration > 0f && state.LaunchVelocity.sqrMagnitude > 0.0001f)
		{
			state.Mode = GuidanceMode.Returning;
			state.ReturnStartVelocity = projectile.Velocity;
			ProjectileTracker.Unregister(projectile);
			return StepReturn(projectile, state, now, deltaTime);
		}
		state.Mode = GuidanceMode.NaturalFlight;
		ProjectileTracker.Unregister(projectile);
		Forget(projectile);
		return false;
	}

	private static bool StepReturn(Projectile projectile, GuidanceState state, float now, float deltaTime)
	{
		float duration = Mathf.Max(0.0001f, CheatState.AimReturnDuration);
		if (now - state.InvalidatedAt >= duration)
		{
			projectile.Velocity = state.LaunchVelocity;
			Forget(projectile);
			return true;
		}
		float turnSpeed = Mathf.Max(0f, CheatState.AimReturnTurnSpeed);
		float radians = turnSpeed * Mathf.Deg2Rad * Mathf.Max(0f, deltaTime);
		Vector3 targetVelocity = state.LaunchVelocity;
		Vector3 currentVelocity = projectile.Velocity;
		if (currentVelocity.sqrMagnitude <= 0.0001f || targetVelocity.sqrMagnitude <= 0.0001f)
		{
			projectile.Velocity = targetVelocity;
		}
		else
		{
			Vector3 steered = turnSpeed > 0f ? Vector3.RotateTowards(currentVelocity, targetVelocity, radians, 0f) : currentVelocity;
			projectile.Velocity = steered.normalized * targetVelocity.magnitude;
		}
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
