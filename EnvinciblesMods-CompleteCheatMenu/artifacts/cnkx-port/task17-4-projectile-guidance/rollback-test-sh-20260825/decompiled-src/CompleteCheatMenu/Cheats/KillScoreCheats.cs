using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;

namespace CompleteCheatMenu.Cheats;

internal static class KillScoreCheats
{
	internal struct BonusOption
	{
		internal string Name;

		internal float Worth;

		internal bool Selected;
	}

	private static List<BonusOption> _catalogue;

	internal static bool Available
	{
		get
		{
			if (GameBinder.Method("PlayerKillScore", "AddKillScore", 3) != null)
			{
				return GameBinder.Type("Bonus") != null;
			}
			return false;
		}
	}

	internal static List<BonusOption> Catalogue
	{
		get
		{
			if (_catalogue != null)
			{
				return _catalogue;
			}
			_catalogue = new List<BonusOption>();
			MethodInfo methodInfo = GameBinder.Method("KillScoreCalculator", "GetAllBonuses", 1);
			for (int i = 0; i < 3; i++)
			{
				if (!(methodInfo != null))
				{
					break;
				}
				if (!(GameBinder.InvokeResult(methodInfo, null, i) is IEnumerable enumerable))
				{
					continue;
				}
				foreach (object item in enumerable)
				{
					string name = ReadName(item);
					float worth = ReadWorth(item);
					if (!string.IsNullOrEmpty(name) && !_catalogue.Any((BonusOption b) => b.Name == name))
					{
						_catalogue.Add(new BonusOption
						{
							Name = name,
							Worth = worth
						});
					}
				}
			}
			_catalogue = _catalogue.OrderByDescending((BonusOption b) => b.Worth).ToList();
			Plugin.Log.LogInfo((object)$"Kill score catalogue: {_catalogue.Count} bonuses.");
			return _catalogue;
		}
	}

	internal static IEnumerable<BonusOption> Selected => Catalogue.Where((BonusOption b) => b.Selected);

	internal static float PreviewMultiplier
	{
		get
		{
			float num = CheatState.KillFlatMultiplier;
			foreach (BonusOption item in Selected)
			{
				num *= item.Worth;
			}
			return num;
		}
	}

	internal static bool ShouldOverride
	{
		get
		{
			if (CheatState.KillScoreOverride)
			{
				if (!Selected.Any())
				{
					return Math.Abs(CheatState.KillFlatMultiplier - 1f) > 0.001f;
				}
				return true;
			}
			return false;
		}
	}

	private static string ReadName(object bonus)
	{
		FieldInfo fieldInfo = GameBinder.Field("Bonus", "Name");
		try
		{
			return fieldInfo?.GetValue(bonus) as string;
		}
		catch
		{
			return null;
		}
	}

	private static float ReadWorth(object bonus)
	{
		if (!GameBinder.TryGet<float>(GameBinder.Field("Bonus", "Worth"), bonus, out var value))
		{
			return 1f;
		}
		return value;
	}

	internal static void Toggle(int index)
	{
		if (index >= 0 && index < Catalogue.Count)
		{
			BonusOption value = _catalogue[index];
			value.Selected = !value.Selected;
			_catalogue[index] = value;
		}
	}

	internal static void ClearSelection()
	{
		for (int i = 0; i < Catalogue.Count; i++)
		{
			BonusOption value = _catalogue[i];
			value.Selected = false;
			_catalogue[i] = value;
		}
	}

	internal static object BuildBonusList()
	{
		Type type = GameBinder.Type("Bonus");
		if (type == null)
		{
			return null;
		}
		IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
		foreach (BonusOption item in Selected)
		{
			list.Add(MakeBonus(type, item.Name, item.Worth));
		}
		float killFlatMultiplier = CheatState.KillFlatMultiplier;
		if (Math.Abs(killFlatMultiplier - 1f) > 0.001f)
		{
			list.Add(MakeBonus(type, CheatState.KillBonusLabel, killFlatMultiplier));
		}
		return list;
	}

	private static object MakeBonus(Type bonusType, string name, float worth)
	{
		ConstructorInfo constructor = bonusType.GetConstructor(new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		if (constructor != null)
		{
			return constructor.Invoke(new object[2] { name, worth });
		}
		object obj = Activator.CreateInstance(bonusType);
		GameBinder.Field("Bonus", "Name")?.SetValue(obj, name);
		GameBinder.Field("Bonus", "Worth")?.SetValue(obj, worth);
		return obj;
	}
}
