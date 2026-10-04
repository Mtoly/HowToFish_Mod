using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class InventoryCheats
{
	internal static bool Available => Refs.LocalInventory != null;

	internal static byte ExtraSlots => ProgressionCheats.ExtraSlots;

	internal static int DuplicateHeld(int count)
	{
		object heldItem = ItemCheats.HeldItem;
		if (heldItem == null)
		{
			return 0;
		}
		string text = SpawnCatalogEntryFor(SkinCheats.ItemId(heldItem));
		if (text == null)
		{
			Plugin.Log.LogWarning((object)"Held item is not in the spawnable catalogue; cannot duplicate.");
			return 0;
		}
		int num = 0;
		for (int i = 0; i < Mathf.Clamp(count, 1, 100); i++)
		{
			if (SpawnCheats.Spawn(text))
			{
				num++;
			}
		}
		return num;
	}

	private static string SpawnCatalogEntryFor(byte id)
	{
		foreach (SpawnCatalog.Entry item in SpawnCatalog.All)
		{
			if (item.Id == id)
			{
				return item.Key;
			}
		}
		return null;
	}

	internal static bool DropAll()
	{
		object localInventory = Refs.LocalInventory;
		object localPlayer = Refs.LocalPlayer;
		if (localInventory == null || localPlayer == null)
		{
			return false;
		}
		Vector3 vector = TeleportCheats.PlayerPosition(localPlayer);
		return GameBinder.TryInvoke(GameBinder.Method("PlayerInventory", "ServerDropAll", 2), localInventory, vector, Quaternion.identity);
	}

	internal static bool SaveInventory()
	{
		object localInventory = Refs.LocalInventory;
		if (localInventory == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("PlayerInventory", "SaveInventory", 1), localInventory, false);
	}
}
