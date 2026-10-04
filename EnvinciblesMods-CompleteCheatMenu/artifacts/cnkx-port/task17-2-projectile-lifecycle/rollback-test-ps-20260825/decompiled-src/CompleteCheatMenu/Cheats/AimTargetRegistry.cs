using System;
using System.Collections.Generic;
using CompleteCheatMenu.Runtime;
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
	internal static List<Entry> Creatures
	{
		get
		{
			Synchronize();
			return _creatures;
		}
	}

	internal static void EnsureInitialized()
	{
		EntityRegistry.EnsureCreaturesInitialized();
		Synchronize();
	}

	internal static void Register(Creature creature)
	{
		if (EntityRegistry.RegisterCreature(creature) != null)
		{
			Synchronize();
		}
	}

	private static void Synchronize()
	{
		IReadOnlyList<EntityRecord> creatures = EntityRegistry.Creatures;
		for (int i = _creatures.Count - 1; i >= 0; i--)
		{
			Entry entry = _creatures[i];
			bool flag = false;
			for (int j = 0; j < creatures.Count; j++)
			{
				if (ReferenceEquals(creatures[j].Entity, entry.Creature))
				{
					flag = true;
					entry.RefreshName(creatures[j].Name);
					break;
				}
			}
			if (!flag)
			{
				_creatures.RemoveAt(i);
			}
		}
		for (int k = 0; k < creatures.Count; k++)
		{
			Creature creature = creatures[k].Entity as Creature;
			if (creature == null)
			{
				continue;
			}
			bool flag2 = false;
			for (int l = 0; l < _creatures.Count; l++)
			{
				if (ReferenceEquals(_creatures[l].Creature, creature))
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				_creatures.Add(new Entry(creature));
			}
		}
	}
}
