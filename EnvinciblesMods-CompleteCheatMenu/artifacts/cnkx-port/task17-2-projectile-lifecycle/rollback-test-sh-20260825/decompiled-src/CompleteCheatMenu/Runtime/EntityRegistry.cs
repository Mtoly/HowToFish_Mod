using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Runtime;

internal static class EntityRegistry
{
	private static readonly List<EntityRecord> _creatures = new List<EntityRecord>(128);

	private static readonly List<EntityRecord> _players = new List<EntityRecord>(16);

	private static readonly List<EntityRecord> _items = new List<EntityRecord>(256);

	private static readonly List<EntityRecord> _containers = new List<EntityRecord>(32);

	private static bool _creaturesInitialized;

	private static float _nextPlayerRefresh;

	private static float _nextItemRefresh;

	private static float _nextContainerRefresh;

	private static int _playerGeneration;

	private static int _itemGeneration;

	private static int _containerGeneration;

	internal static IReadOnlyList<EntityRecord> Creatures
	{
		get
		{
			EnsureCreaturesInitialized();
			PruneDestroyed(_creatures);
			return _creatures;
		}
	}

	internal static IReadOnlyList<EntityRecord> Players
	{
		get
		{
			RefreshPlayers();
			return _players;
		}
	}

	internal static IReadOnlyList<EntityRecord> Items
	{
		get
		{
			RefreshItems();
			return _items;
		}
	}

	internal static IReadOnlyList<EntityRecord> Containers
	{
		get
		{
			RefreshContainers();
			return _containers;
		}
	}

	internal static void EnsureCreaturesInitialized()
	{
		if (_creaturesInitialized)
		{
			return;
		}
		_creaturesInitialized = true;
		Creature[] array = UnityEngine.Object.FindObjectsByType<Creature>();
		for (int i = 0; i < array.Length; i++)
		{
			RegisterCreature(array[i]);
		}
	}

	internal static EntityRecord RegisterCreature(Creature creature)
	{
		if (creature == null)
		{
			return null;
		}
		return Register(_creatures, creature, creature.transform, creature.name, EntityKind.Creature, 0);
	}

	internal static EntityRecord RegisterPlayer(Player player)
	{
		if (player == null)
		{
			return null;
		}
		return Register(_players, player, player.transform, TeleportCheats.PlayerName(player), EntityKind.Player, _playerGeneration);
	}

	internal static EntityRecord RegisterItem(Item item)
	{
		if (item == null)
		{
			return null;
		}
		return Register(_items, item, item.transform, item.name, EntityKind.Item, _itemGeneration);
	}

	internal static EntityRecord RegisterContainer(Component container)
	{
		if (container == null)
		{
			return null;
		}
		return Register(_containers, container, container.transform, container.name, EntityKind.Container, _containerGeneration);
	}

	private static EntityRecord Register(List<EntityRecord> records, object entity, Transform transform, string name, EntityKind kind, int generation)
	{
		if (!Refs.IsAlive(entity) || transform == null)
		{
			return null;
		}
		int instanceId = RuntimeHelpers.GetHashCode(entity);
		for (int i = 0; i < records.Count; i++)
		{
			EntityRecord entityRecord = records[i];
			if (ReferenceEquals(entityRecord.Entity, entity) || entityRecord.InstanceId == instanceId)
			{
				entityRecord.SeenGeneration = generation;
				entityRecord.RefreshName(name);
				return entityRecord;
			}
		}
		EntityRecord entityRecord2 = new EntityRecord(entity, transform, name, kind, generation);
		records.Add(entityRecord2);
		return entityRecord2;
	}

	internal static void RefreshPlayers(float interval = 0.1f)
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime < _nextPlayerRefresh)
		{
			return;
		}
		_nextPlayerRefresh = unscaledTime + Mathf.Max(0.05f, interval);
		_playerGeneration++;
		IEnumerable enumerable = ReadEnumerableMember("PlayerManager", "AlivePlayers");
		bool flag = enumerable != null;
		if (enumerable != null)
		{
			foreach (object item in enumerable)
			{
				if (item is Player player && player != null)
				{
					RegisterPlayer(player);
				}
			}
		}
		if (flag)
		{
			RemoveStale(_players, _playerGeneration);
		}
		else
		{
			PruneDestroyed(_players);
		}
	}

	internal static void RefreshItems(float interval = 0.5f)
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime < _nextItemRefresh)
		{
			return;
		}
		_nextItemRefresh = unscaledTime + Mathf.Max(0.1f, interval);
		_itemGeneration++;
		PropertyInfo propertyInfo = GameBinder.Property("ItemManager", "Items");
		bool flag = false;
		try
		{
			if (propertyInfo?.GetValue(null) is IDictionary dictionary)
			{
				flag = true;
				foreach (DictionaryEntry item in dictionary)
				{
					Item item2 = item.Value as Item;
					if (item2 == null && item.Key is Transform transform && transform != null)
					{
						item2 = transform.GetComponent<Item>();
					}
					if (item2 != null)
					{
						RegisterItem(item2);
					}
				}
			}
		}
		catch
		{
		}
		if (flag)
		{
			RemoveStale(_items, _itemGeneration);
		}
		else
		{
			PruneDestroyed(_items);
		}
	}

	internal static void RefreshContainers(float interval = 0.5f)
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime < _nextContainerRefresh)
		{
			return;
		}
		_nextContainerRefresh = unscaledTime + Mathf.Max(0.1f, interval);
		_containerGeneration++;
		DeadPlayer[] array = UnityEngine.Object.FindObjectsByType<DeadPlayer>();
		for (int i = 0; i < array.Length; i++)
		{
			RegisterContainer(array[i]);
		}
		RemoveStale(_containers, _containerGeneration);
	}

	internal static void PruneAllDestroyed()
	{
		PruneDestroyed(_creatures);
		PruneDestroyed(_players);
		PruneDestroyed(_items);
		PruneDestroyed(_containers);
	}

	private static void PruneDestroyed(List<EntityRecord> records)
	{
		for (int i = records.Count - 1; i >= 0; i--)
		{
			if (!records[i].IsAlive)
			{
				records.RemoveAt(i);
			}
		}
	}

	private static void RemoveStale(List<EntityRecord> records, int generation)
	{
		for (int i = records.Count - 1; i >= 0; i--)
		{
			EntityRecord entityRecord = records[i];
			if (!entityRecord.IsAlive || entityRecord.SeenGeneration != generation)
			{
				records.RemoveAt(i);
			}
		}
	}

	private static IEnumerable ReadEnumerableMember(string typeName, string memberName)
	{
		PropertyInfo propertyInfo = GameBinder.Property(typeName, memberName);
		try
		{
			if (propertyInfo?.GetValue(null) is IEnumerable enumerable)
			{
				return enumerable;
			}
		}
		catch
		{
		}
		FieldInfo fieldInfo = GameBinder.Field(typeName, memberName);
		try
		{
			return fieldInfo?.GetValue(null) as IEnumerable;
		}
		catch
		{
			return null;
		}
	}
}
