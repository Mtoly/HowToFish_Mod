using CompleteCheatMenu.Game;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal enum EntityKind
{
	Creature,
	Player,
	Item,
	Container
}

internal sealed class EntityRecord
{
	internal readonly object Entity;

	internal readonly Transform Transform;

	internal readonly int InstanceId;

	internal readonly EntityKind Kind;

	internal readonly string TypeName;

	internal string Name;

	internal int SeenGeneration;

	internal bool IsAlive => Refs.IsAlive(Entity) && Transform != null;

	internal EntityRecord(object entity, Transform transform, string name, EntityKind kind, int generation)
	{
		Entity = entity;
		Transform = transform;
		Kind = kind;
		SeenGeneration = generation;
		InstanceId = RuntimeHelpers.GetHashCode(entity);
		TypeName = entity.GetType().Name;
		RefreshName(name);
	}

	internal void RefreshName(string name)
	{
		Name = (string.IsNullOrEmpty(name) ? TypeName : name);
	}
}
