using System.Collections.Generic;
using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal static class EspSnapshotBuilder
{
	private static readonly EspSnapshot _current = new EspSnapshot();
	private static float _nextRefresh;
	private static bool _lastCreatures;
	private static bool _lastItems;
	private static bool _lastPlayers;
	private static bool _hasOptions;

	internal static EspSnapshot Current => _current;

	internal static EspSnapshot Refresh(bool creatures, bool items, bool players, float interval = 0.5f)
	{
		float unscaledTime = Time.unscaledTime;
		bool flag = !_hasOptions || creatures != _lastCreatures || items != _lastItems || players != _lastPlayers;
		if (!flag && unscaledTime < _nextRefresh)
		{
			return _current;
		}
		_hasOptions = true;
		_lastCreatures = creatures;
		_lastItems = items;
		_lastPlayers = players;
		_nextRefresh = unscaledTime + Mathf.Max(0.1f, interval);
		_current.Clear();
		if (creatures)
		{
			AddAlive(EntityRegistry.Creatures, _current.Creatures);
		}
		if (players)
		{
			AddAlive(EntityRegistry.Players, _current.Players);
		}
		if (items)
		{
			AddAlive(EntityRegistry.Items, _current.Items);
			AddAlive(EntityRegistry.Containers, _current.Containers);
		}
		_current.CompleteRefresh();
		return _current;
	}

	private static void AddAlive(IReadOnlyList<EntityRecord> source, List<EntityRecord> destination)
	{
		for (int i = 0; i < source.Count; i++)
		{
			EntityRecord entityRecord = source[i];
			if (entityRecord.IsAlive)
			{
				destination.Add(entityRecord);
			}
		}
	}
}
