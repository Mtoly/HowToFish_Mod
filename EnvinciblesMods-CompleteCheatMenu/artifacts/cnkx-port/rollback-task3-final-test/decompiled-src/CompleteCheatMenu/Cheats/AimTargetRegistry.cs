using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class AimTargetRegistry
{
	internal sealed class Entry
	{
		internal readonly Creature Creature;
		internal readonly Transform Transform;
		internal string Name;
		internal bool IsAlbatross;
		internal bool IsSeagull;

		internal Entry(Creature creature)
		{
			Creature = creature;
			Transform = creature.transform;
			RefreshName(creature.name);
		}

		internal void RefreshName(string name)
		{
			Name = name ?? "Creature";
			IsAlbatross = Name.IndexOf("albatross", StringComparison.OrdinalIgnoreCase) >= 0;
			IsSeagull = Name.IndexOf("seagull", StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}

	private static readonly List<Entry> _creatures = new List<Entry>(128);
	private static bool _initialized;

	internal static List<Entry> Creatures
	{
		get
		{
			EnsureInitialized();
			return _creatures;
		}
	}

	internal static void EnsureInitialized()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		Creature[] existing = UnityEngine.Object.FindObjectsByType<Creature>();
		for (int i = 0; i < existing.Length; i++)
		{
			Register(existing[i]);
		}
	}

	internal static void Register(Creature creature)
	{
		if (creature == null)
		{
			return;
		}
		for (int i = 0; i < _creatures.Count; i++)
		{
			if (ReferenceEquals(_creatures[i].Creature, creature))
			{
				return;
			}
		}
		_creatures.Add(new Entry(creature));
	}
}
