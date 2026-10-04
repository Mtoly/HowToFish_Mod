using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class MovementTab
{
	private struct Row
	{
		internal string Field;

		internal string Label;

		internal float Min;

		internal float Max;
	}

	internal const string Title = "Movement";

	private static readonly Row[] Rows = new Row[8]
	{
		new Row
		{
			Field = "_walkSpeed",
			Label = "Walk speed",
			Min = 0f,
			Max = 60f
		},
		new Row
		{
			Field = "_sprintSpeed",
			Label = "Sprint speed",
			Min = 0f,
			Max = 100f
		},
		new Row
		{
			Field = "_crouchWalkSpeedMulti",
			Label = "Crouch multi",
			Min = 0f,
			Max = 5f
		},
		new Row
		{
			Field = "_jumpForce",
			Label = "Jump force",
			Min = 0f,
			Max = 60f
		},
		new Row
		{
			Field = "_extraGravityForce",
			Label = "Extra gravity",
			Min = 0f,
			Max = 60f
		},
		new Row
		{
			Field = "_acceleration",
			Label = "Acceleration",
			Min = 0f,
			Max = 200f
		},
		new Row
		{
			Field = "_decceleration",
			Label = "Deceleration",
			Min = 0f,
			Max = 200f
		},
		new Row
		{
			Field = "_sinkSpeed",
			Label = "Sink speed",
			Min = 0f,
			Max = 30f
		}
	};

	internal static void Draw()
	{
		if (!MovementCheats.Available)
		{
			Widgets.RequireAvailable("Load into a save first.");
			return;
		}
		Widgets.Section("Flight", delegate
		{
			if (!MovementCheats.FlyAvailable)
			{
				Widgets.Note("PlayerMovement._curMoveSpeed did not resolve — flight unavailable.");
			}
			else
			{
				CheatState.Fly = Widgets.ToggleRow("Fly mode", CheatState.Fly, CheatState.Fly ? "active" : "off");
				CheatState.FlyHorizontalSpeed = Widgets.SliderRow("Horizontal speed", CheatState.FlyHorizontalSpeed, 1f, 200f, "0.#", 25f);
				CheatState.FlyVerticalSpeed = Widgets.SliderRow("Vertical speed", CheatState.FlyVerticalSpeed, 1f, 120f, "0.#", 12f);
				Widgets.Note("WASD moves, Space up, Left Ctrl down. Releasing both hovers instead of falling. Gravity is restored when you switch fly off.");
			}
		});
		Widgets.Section("Ground movement", delegate
		{
			Row[] rows = Rows;
			for (int i = 0; i < rows.Length; i++)
			{
				Row row = rows[i];
				float num = MovementCheats.Get(row.Field);
				float num2 = Widgets.SliderRow(row.Label, num, row.Min, row.Max, "0.##", MovementCheats.Default(row.Field));
				if (!Mathf.Approximately(num, num2))
				{
					MovementCheats.Set(row.Field, num2);
				}
			}
			GUILayout.Space(4f);
			Widgets.ActionButton("Reset all to defaults", delegate
			{
				MovementCheats.ResetAll();
				return true;
			}, "Movement reset", Theme.BtnAccent);
			Widgets.Note("Defaults are captured the first time each slider is read this session.");
		});
	}
}
