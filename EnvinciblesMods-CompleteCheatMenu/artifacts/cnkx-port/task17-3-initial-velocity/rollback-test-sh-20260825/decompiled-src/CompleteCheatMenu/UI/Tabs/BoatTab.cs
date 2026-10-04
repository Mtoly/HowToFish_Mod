using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class BoatTab
{
	private struct Row
	{
		internal string Field;

		internal string Label;

		internal float Min;

		internal float Max;
	}

	internal const string Title = "Boat";

	private static readonly Row[] Rows = new Row[6]
	{
		new Row
		{
			Field = "_antiRollOverForce",
			Label = "Anti-rollover",
			Min = 0f,
			Max = 200f
		},
		new Row
		{
			Field = "_steerSmoothing",
			Label = "Steer smoothing",
			Min = 0f,
			Max = 30f
		},
		new Row
		{
			Field = "_linearDampAboveWater",
			Label = "Air drag",
			Min = 0f,
			Max = 5f
		},
		new Row
		{
			Field = "_angularDampAboveWater",
			Label = "Air spin drag",
			Min = 0f,
			Max = 5f
		},
		new Row
		{
			Field = "_underwaterLinearDamp",
			Label = "Water drag",
			Min = 0f,
			Max = 5f
		},
		new Row
		{
			Field = "_underwaterAngularDamp",
			Label = "Water spin drag",
			Min = 0f,
			Max = 5f
		}
	};

	internal static void Draw()
	{
		if (!Widgets.RequireAvailable(CheatGate.PageBlockReason()))
		{
			return;
		}
		if (!BoatCheats.Available)
		{
			Widgets.Note("No boat in the scene yet. Load into a save with a boat spawned.");
			return;
		}
		Widgets.Section("Unlocks", delegate
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(BoatCheats.Unlocked ? "Boat unlocked" : "Boat locked", Theme.Body);
			GUILayout.FlexibleSpace();
			GUILayout.Label(BoatCheats.RadarUnlocked ? "Radar unlocked" : "Radar locked", Theme.Muted);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Unlock boat", BoatCheats.UnlockBoat, "Boat unlocked", Theme.BtnAccent);
			Widgets.ActionButton("Unlock radar", BoatCheats.UnlockRadar, "Radar unlocked", Theme.BtnAccent);
			Widgets.ActionButton("Bring boat to me", BoatCheats.BringToPlayer, "Boat moved");
			GUILayout.EndHorizontal();
		});
		Widgets.Section("Motor", delegate
		{
			GUILayout.Label($"Current tier: {BoatCheats.Motor}", Theme.Body);
			GUILayout.BeginHorizontal();
			for (byte b = 0; b <= 4; b++)
			{
				byte t = b;
				Widgets.ActionButton($"{t}", () => BoatCheats.SetMotor(t), $"Motor set to {t}", (t == BoatCheats.Motor) ? Theme.BtnAccent : Theme.Btn, 44f);
			}
			GUILayout.EndHorizontal();
			Widgets.Note("The game's own SetMotor only ever upgrades, so dropping to a lower tier writes the value directly instead.");
		});
		Widgets.Section("Flight", delegate
		{
			if (!BoatCheats.FlyAvailable)
			{
				Widgets.Note("No boat rigidbody resolved — flight unavailable.");
			}
			else
			{
				CheatState.BoatFly = Widgets.ToggleRow("Boat flight", CheatState.BoatFly, CheatState.BoatFly ? "active" : "off");
				CheatState.BoatFlySpeed = Widgets.SliderRow("Climb speed", CheatState.BoatFlySpeed, 1f, 100f, "0.#", 15f);
				Widgets.Note("Turns off the boat's gravity and gives Space and Left Ctrl control of its height. Steer normally while airborne. Gravity comes back when you switch it off — so switch it off over water.");
			}
		});
		Widgets.Section("Speed", delegate
		{
			float motorForce = BoatCheats.MotorForce;
			float num = Widgets.SliderRow("Engine power", motorForce, 0f, 20000f, "0", BoatCheats.MotorForceDefault);
			if (!Mathf.Approximately(motorForce, num))
			{
				BoatCheats.SetMotorForce(num);
			}
			float maxSpeed = BoatCheats.MaxSpeed;
			float num2 = Widgets.SliderRow("Speed limit", maxSpeed, 1f, 400f, "0");
			if (!Mathf.Approximately(maxSpeed, num2))
			{
				BoatCheats.SetMaxSpeed(num2);
			}
			Widgets.Note("Engine power is the thrust the motor applies and is the real speed control. The limit is the rigidbody's velocity cap — the game only reads its own value at startup, so this writes the cap directly.");
		});
		Widgets.Section("Handling", delegate
		{
			Row[] rows = Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				float num = BoatCheats.Get(row.Field);
				float num2 = Widgets.SliderRow(row.Label, num, row.Min, row.Max, "0.##", BoatCheats.Default(row.Field));
				if (!Mathf.Approximately(num, num2))
				{
					BoatCheats.Set(row.Field, num2);
				}
			}
			GUILayout.Space(4f);
			Widgets.ActionButton("Reset handling", delegate
			{
				BoatCheats.ResetAll();
				return true;
			}, "Boat handling reset", Theme.BtnAccent);
			Widgets.Note("Steering and throttle angle were removed — they only rotate the wheel and lever props and never touched how the boat handles.");
		});
	}
}
