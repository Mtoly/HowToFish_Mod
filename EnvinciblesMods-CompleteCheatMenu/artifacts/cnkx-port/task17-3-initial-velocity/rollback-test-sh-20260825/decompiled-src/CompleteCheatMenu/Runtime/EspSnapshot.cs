using System.Collections.Generic;

namespace CompleteCheatMenu.Runtime;

internal sealed class EspSnapshot
{
	internal readonly List<EntityRecord> Creatures = new List<EntityRecord>(128);
	internal readonly List<EntityRecord> Players = new List<EntityRecord>(16);
	internal readonly List<EntityRecord> Items = new List<EntityRecord>(256);
	internal readonly List<EntityRecord> Containers = new List<EntityRecord>(32);

	internal int Revision { get; private set; }

	internal int TotalCount => Creatures.Count + Players.Count + Items.Count + Containers.Count;

	internal void Clear()
	{
		Creatures.Clear();
		Players.Clear();
		Items.Clear();
		Containers.Clear();
	}

	internal void CompleteRefresh()
	{
		Revision++;
	}
}
