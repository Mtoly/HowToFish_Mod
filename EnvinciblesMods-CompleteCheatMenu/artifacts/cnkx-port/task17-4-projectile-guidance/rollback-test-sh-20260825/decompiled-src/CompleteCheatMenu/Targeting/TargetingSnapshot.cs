using System;
using System.Collections.Generic;
using CompleteCheatMenu.Runtime;

namespace CompleteCheatMenu.Targeting;

internal sealed class TargetingSnapshot
{
	private static readonly IReadOnlyList<EntityRecord> Empty = Array.Empty<EntityRecord>();

	internal IReadOnlyList<EntityRecord> Creatures { get; private set; } = Empty;
	internal IReadOnlyList<EntityRecord> Players { get; private set; } = Empty;

	internal void Refresh(bool creatures, bool players)
	{
		Creatures = (creatures ? EntityRegistry.Creatures : Empty);
		Players = (players ? EntityRegistry.Players : Empty);
	}
}
