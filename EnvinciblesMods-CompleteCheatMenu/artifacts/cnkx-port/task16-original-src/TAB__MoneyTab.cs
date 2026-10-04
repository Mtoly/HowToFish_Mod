using System;
using System.Globalization;
using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class MoneyTab
{
	internal const string Title = "Money";

	internal static void Draw()
	{
		if (!Widgets.RequireHost(CheatGate.BlockReason()))
		{
			return;
		}
		if (!MoneyCheats.Available)
		{
			Widgets.Note("MoneyManager.AddMoney(int, Player) did not resolve. See Diagnostics.");
			return;
		}
		Widgets.Section("Balance", delegate
		{
			GUILayout.Label($"{MoneyCheats.Balance:N0}", Theme.H1);
			CheatState.MoneyAmount = Widgets.FieldRow("Amount", CheatState.MoneyAmount, 80f);
			int amount = Parse(CheatState.MoneyAmount);
			Widgets.Note("Accepts shorthand: 50k, 2.5m, 1b.");
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Add", () => MoneyCheats.Add(amount), $"+{amount:N0}", Theme.BtnAccent);
			Widgets.ActionButton("Remove", () => MoneyCheats.Remove(amount), $"-{amount:N0}");
			Widgets.ActionButton("Set to", () => MoneyCheats.SetTo(amount), $"Balance set to {amount:N0}");
			GUILayout.EndHorizontal();
		});
		Widgets.Section("Quick add", delegate
		{
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("+1k", () => MoneyCheats.Add(1000), "+1,000");
			Widgets.ActionButton("+10k", () => MoneyCheats.Add(10000), "+10,000");
			Widgets.ActionButton("+100k", () => MoneyCheats.Add(100000), "+100,000");
			Widgets.ActionButton("+1M", () => MoneyCheats.Add(1000000), "+1,000,000");
			GUILayout.EndHorizontal();
		});
	}

	internal static int Parse(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 0;
		}
		s = s.Trim().Replace(",", "").Replace("_", "");
		double num = 1.0;
		if (s.Length > 1)
		{
			switch (char.ToLowerInvariant(s[s.Length - 1]))
			{
			case 'k':
				num = 1000.0;
				s = s.Substring(0, s.Length - 1);
				break;
			case 'm':
				num = 1000000.0;
				s = s.Substring(0, s.Length - 1);
				break;
			case 'b':
				num = 1000000000.0;
				s = s.Substring(0, s.Length - 1);
				break;
			}
		}
		if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return 0;
		}
		double val = result * num;
		return (int)Math.Max(-2147483648.0, Math.Min(2147483647.0, val));
	}
}
