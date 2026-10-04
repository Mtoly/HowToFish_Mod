using System.Collections.Generic;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.Targeting;

internal sealed class VelocityTracker
{
	private const int SampleCapacity = 4;

	private sealed class Track
	{
		internal readonly Vector3[] Positions = new Vector3[SampleCapacity];
		internal readonly float[] Times = new float[SampleCapacity];
		internal int Start;
		internal int Count;
		internal float LastSeen;
	}

	private readonly Dictionary<int, Track> _tracks = new Dictionary<int, Track>();
	private float _nextPrune;

	internal Vector3 Observe(int instanceId, Vector3 position, float now)
	{
		if (!_tracks.TryGetValue(instanceId, out var track))
		{
			track = new Track();
			_tracks[instanceId] = track;
		}
		track.LastSeen = now;
		if (track.Count > 0)
		{
			int newest = (track.Start + track.Count - 1) % SampleCapacity;
			float deltaTime = now - track.Times[newest];
			if (deltaTime < Mathf.Max(0.001f, CheatState.AimVelocityMinInterval))
			{
				return Estimate(track);
			}
			float distance = Vector3.Distance(position, track.Positions[newest]);
			float speed = distance / deltaTime;
			if (distance > Mathf.Max(0f, CheatState.AimTeleportDistance) || speed > Mathf.Max(0f, CheatState.AimMaxTargetSpeed))
			{
				Reset(track, position, now);
				return Vector3.zero;
			}
		}
		Append(track, position, now);
		if (now >= _nextPrune)
		{
			Prune(now);
			_nextPrune = now + 2f;
		}
		return Estimate(track);
	}

	internal void Clear()
	{
		_tracks.Clear();
		_nextPrune = 0f;
	}

	private static void Append(Track track, Vector3 position, float now)
	{
		int index;
		if (track.Count < SampleCapacity)
		{
			index = (track.Start + track.Count) % SampleCapacity;
			track.Count++;
		}
		else
		{
			index = track.Start;
			track.Start = (track.Start + 1) % SampleCapacity;
		}
		track.Positions[index] = position;
		track.Times[index] = now;
	}

	private static Vector3 Estimate(Track track)
	{
		if (track.Count < 2)
		{
			return Vector3.zero;
		}
		int oldest = track.Start;
		int newest = (track.Start + track.Count - 1) % SampleCapacity;
		float elapsed = track.Times[newest] - track.Times[oldest];
		if (elapsed <= 0.0001f)
		{
			return Vector3.zero;
		}
		Vector3 velocity = (track.Positions[newest] - track.Positions[oldest]) / elapsed;
		return BallisticPredictor.SanitizeVelocity(velocity, CheatState.AimMaxTargetSpeed);
	}

	private static void Reset(Track track, Vector3 position, float now)
	{
		track.Start = 0;
		track.Count = 1;
		track.Positions[0] = position;
		track.Times[0] = now;
	}

	private void Prune(float now)
	{
		List<int> stale = null;
		foreach (KeyValuePair<int, Track> pair in _tracks)
		{
			if (now - pair.Value.LastSeen > 5f)
			{
				if (stale == null)
				{
					stale = new List<int>();
				}
				stale.Add(pair.Key);
			}
		}
		if (stale == null)
		{
			return;
		}
		for (int i = 0; i < stale.Count; i++)
		{
			_tracks.Remove(stale[i]);
		}
	}
}
