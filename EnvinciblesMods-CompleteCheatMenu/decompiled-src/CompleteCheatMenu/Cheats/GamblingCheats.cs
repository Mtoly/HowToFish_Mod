using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class GamblingCheats
{
	internal enum Mode
	{
		Off,
		Odds,
		ForceColour
	}

	internal static Mode RigMode = Mode.Off;

	internal static float WinChance = 50f;

	internal static string ForcedColour = "Red";

	internal static int SpinsSeen;

	internal static int SpinsWon;

	internal static int SpinsLost;

	internal static string LastOutcome = "-";

	internal static string CurrentTarget = "-";

	internal static string LastResolvedColour = "-";

	private static bool _wasBetting;

	private static string _target;

	private const float MaxStepDegrees = 4f;

	private static object Casino => Refs.FindInScene("CasinoManager");

	private static object LocalCasino => Refs.FindInScene("LocalCasino");

	internal static bool Available => LocalCasino != null;

	internal static string[] Colours => GameBinder.EnumNames("BetColor");

	internal static string BetColour
	{
		get
		{
			FieldInfo fieldInfo = GameBinder.Field("CasinoManager", "_curBetColor");
			if (fieldInfo == null)
			{
				return "-";
			}
			try
			{
				return fieldInfo.GetValue(null)?.ToString() ?? "-";
			}
			catch
			{
				return "-";
			}
		}
	}

	internal static bool IsBetting
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("CasinoManager", "IsBetting");
			if (propertyInfo == null)
			{
				return false;
			}
			try
			{
				return (bool)propertyInfo.GetValue(null);
			}
			catch
			{
				return false;
			}
		}
	}

	internal static int PotWorth
	{
		get
		{
			object casino = Casino;
			if (casino == null)
			{
				return 0;
			}
			PropertyInfo propertyInfo = GameBinder.Property("CasinoManager", "TotalWorth");
			if (propertyInfo == null)
			{
				return 0;
			}
			try
			{
				return (int)propertyInfo.GetValue(casino);
			}
			catch
			{
				return 0;
			}
		}
	}

	private static bool IsPlaying
	{
		get
		{
			bool value;
			return GameBinder.TryGet<bool>(GameBinder.Field("LocalCasino", "_isPlaying"), LocalCasino, out value) && value;
		}
	}

	private static float SlotSize
	{
		get
		{
			if (!GameBinder.TryGet<float>(GameBinder.Field("LocalCasino", "_slotSize"), LocalCasino, out var value) || !(value > 0.01f))
			{
				return 9.72973f;
			}
			return value;
		}
	}

	private static int SlotCount => Mathf.Max(1, Mathf.RoundToInt(360f / SlotSize));

	private static Transform Wheel => GetTransform("_wheel");

	private static Transform BallAngleObject => GetTransform("_ballAngleObject");

	internal static string CurrentBallColour
	{
		get
		{
			if (!TryGetSlot(out var slot))
			{
				return "-";
			}
			return ColourOfSlot(slot);
		}
	}

	internal static bool SlotsAvailable => GameBinder.Method("SlotMachineManager", "RollRandom", 1) != null;

	internal static string LastArmed { get; private set; } = "nothing";

	internal static bool SlotRigArmed
	{
		get
		{
			if (GameBinder.TryGet<byte>(GameBinder.Field("SlotMachineManager", "_cheatSkinIndex"), null, out var value))
			{
				return value != byte.MaxValue;
			}
			return false;
		}
	}

	private static Transform GetTransform(string field)
	{
		FieldInfo fieldInfo = GameBinder.Field("LocalCasino", field);
		object localCasino = LocalCasino;
		if (fieldInfo == null || localCasino == null)
		{
			return null;
		}
		try
		{
			return fieldInfo.GetValue(localCasino) as Transform;
		}
		catch
		{
			return null;
		}
	}

	private static bool TryGetSlot(out int slot)
	{
		slot = -1;
		Transform wheel = Wheel;
		Transform ballAngleObject = BallAngleObject;
		if (wheel == null || ballAngleObject == null)
		{
			return false;
		}
		Vector3 vector = wheel.position - ballAngleObject.position;
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		float y = wheel.eulerAngles.y;
		float num2 = Mathf.Repeat(num - y, 360f);
		slot = Mathf.FloorToInt(num2 / SlotSize);
		return true;
	}

	internal static string ColourOfSlot(int slot)
	{
		if (slot == 0)
		{
			return "Green";
		}
		if (slot % 2 <= 0)
		{
			return "Red";
		}
		return "Black";
	}

	private static int NearestSlotOfColour(int from, string colour)
	{
		int slotCount = SlotCount;
		if (colour == "Green")
		{
			return 0;
		}
		for (int i = 0; i <= slotCount / 2 + 1; i++)
		{
			int num = ((from + i) % slotCount + slotCount) % slotCount;
			if (ColourOfSlot(num) == colour)
			{
				return num;
			}
			int num2 = ((from - i) % slotCount + slotCount) % slotCount;
			if (ColourOfSlot(num2) == colour)
			{
				return num2;
			}
		}
		return from;
	}

	internal static bool OverrideRouletteResult(object[] args)
	{
		if (args == null || args.Length == 0 || RigMode != Mode.ForceColour || !IsBetting)
		{
			return false;
		}
		string colour = CanonicalColour(ForcedColour);
		object value = GameBinder.EnumValue("BetColor", colour);
		if (value == null)
		{
			return false;
		}
		args[0] = value;
		LastResolvedColour = colour;
		return true;
	}

	internal static string CanonicalColour(string colour)
	{
		return colour switch
		{
			"红色" => "Red",
			"黑色" => "Black",
			"绿色" => "Green",
			_ => colour
		};
	}

	private static void OnSpinStarted()
	{
		SpinsSeen++;
		LastResolvedColour = "-";
		if (RigMode == Mode.Off)
		{
			_target = null;
			CurrentTarget = "not rigged";
			return;
		}
		if (RigMode == Mode.ForceColour)
		{
			_target = CanonicalColour(ForcedColour);
			CurrentTarget = _target;
			return;
		}
		string betColour = BetColour;
		bool flag = UnityEngine.Random.Range(0f, 100f) < WinChance;
		if (flag)
		{
			_target = betColour;
		}
		else
		{
			_target = ((betColour == "Red") ? "Black" : "Red");
		}
		CurrentTarget = _target + " (" + (flag ? "win" : "loss") + ")";
	}

	internal static void FixedStep()
	{
		bool isBetting = IsBetting;
		if (isBetting && !_wasBetting)
		{
			OnSpinStarted();
		}
		if (!isBetting && _wasBetting)
		{
			OnSpinEnded();
		}
		_wasBetting = isBetting;
		if (isBetting && _target != null && IsPlaying && TryGetSlot(out var slot) && !(ColourOfSlot(slot) == _target))
		{
			int num = NearestSlotOfColour(slot, _target);
			float num2 = (float)(slot - num) * SlotSize;
			if (num2 > 180f)
			{
				num2 -= 360f;
			}
			if (num2 < -180f)
			{
				num2 += 360f;
			}
			float num3 = Mathf.Clamp(num2, -4f, 4f);
			Transform wheel = Wheel;
			if (!(wheel == null))
			{
				Vector3 localEulerAngles = wheel.localEulerAngles;
				localEulerAngles.y += num3;
				wheel.localEulerAngles = localEulerAngles;
			}
		}
	}

	private static void OnSpinEnded()
	{
		string currentBallColour = CurrentBallColour;
		string betColour = BetColour;
		string resolvedColour = (LastResolvedColour == "-" || string.IsNullOrEmpty(LastResolvedColour)) ? currentBallColour : LastResolvedColour;
		bool flag = resolvedColour == betColour;
		if (flag)
		{
			SpinsWon++;
		}
		else
		{
			SpinsLost++;
		}
		LastOutcome = (resolvedColour == currentBallColour) ? currentBallColour + " — " + (flag ? "won" : "lost") : resolvedColour + " — " + (flag ? "won" : "lost") + " (球位 " + currentBallColour + ")";
		LastResolvedColour = "-";
		_target = null;
		CurrentTarget = "-";
	}

	internal static void ResetStats()
	{
		SpinsSeen = (SpinsWon = (SpinsLost = 0));
		LastOutcome = "-";
		LastResolvedColour = "-";
	}

	internal static bool SettleNow()
	{
		return GameBinder.TrySet(GameBinder.Field("LocalCasino", "_timeInSameSlot"), LocalCasino, 2.5f);
	}

	internal static bool StartSpin(string colourName)
	{
		object obj = GameBinder.EnumValue("BetColor", colourName);
		if (obj == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("CasinoManager", "ServerStartBet", 1), Casino, obj);
	}

	internal static List<object> BetItems()
	{
		List<object> list = new List<object>();
		FieldInfo fieldInfo = GameBinder.Field("CasinoManager", "_itemsToBet");
		if (fieldInfo == null)
		{
			return list;
		}
		try
		{
			if (fieldInfo.GetValue(null) is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					if (Refs.IsAlive(item))
					{
						list.Add(item);
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	internal static bool BoostBetItems(float multiplier)
	{
		List<object> list = BetItems();
		if (list.Count == 0)
		{
			return false;
		}
		bool result = true;
		foreach (object item in list)
		{
			if (!ItemCheats.SetBettingMulti(item, multiplier))
			{
				result = false;
			}
		}
		return result;
	}

	internal static bool SetSlotSkin(object item, byte skinIndex)
	{
		return GameBinder.TryInvoke(GameBinder.Method("SlotMachineManager", "SetCheatSkin", 2), null, item, skinIndex);
	}

	internal static bool ClearSlotSkin()
	{
		return GameBinder.TryInvoke(GameBinder.Method("SlotMachineManager", "SetCheatSkin", 2), null, null, byte.MaxValue);
	}

	internal static bool RollSlots()
	{
		object localPlayer = Refs.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}
		return GameBinder.TryInvoke(GameBinder.Method("SlotMachineManager", "RollRandom", 1), null, localPlayer);
	}

	internal static List<object> SkinTargets()
	{
		return SkinCheats.SkinnableItems();
	}

	internal static string TargetName(object item)
	{
		object obj;
		if (item != null)
		{
			obj = (item as UnityEngine.Object)?.name;
			if (obj == null)
			{
				return "item";
			}
		}
		else
		{
			obj = "Boat";
		}
		return (string)obj;
	}

	internal static List<string> RaritiesFor(object item)
	{
		return (from s in (item == null) ? SkinCheats.BoatSkins() : SkinCheats.SkinsFor(item)
			select s.Rarity into r
			where !string.IsNullOrEmpty(r)
			select r).Distinct().ToList();
	}

	internal static List<SkinCheats.SkinEntry> SkinsFor(object item)
	{
		if (item != null)
		{
			return SkinCheats.SkinsFor(item);
		}
		return SkinCheats.BoatSkins();
	}

	internal static bool ArmRandomOfRarity(object item, string rarity)
	{
		List<SkinCheats.SkinEntry> list = (from s in SkinsFor(item)
			where string.Equals(s.Rarity, rarity, StringComparison.OrdinalIgnoreCase)
			select s).ToList();
		if (list.Count == 0)
		{
			return false;
		}
		SkinCheats.SkinEntry skinEntry = list[UnityEngine.Random.Range(0, list.Count)];
		LastArmed = TargetName(item) + " — " + skinEntry.Name + " (" + skinEntry.Rarity + ")";
		return SetSlotSkin(item, skinEntry.Index);
	}

	internal static bool ArmSkin(object item, SkinCheats.SkinEntry skin)
	{
		LastArmed = TargetName(item) + " — " + skin.Name + " (" + skin.Rarity + ")";
		return SetSlotSkin(item, skin.Index);
	}

	internal static bool ClearArmed()
	{
		LastArmed = "nothing";
		return ClearSlotSkin();
	}
}



