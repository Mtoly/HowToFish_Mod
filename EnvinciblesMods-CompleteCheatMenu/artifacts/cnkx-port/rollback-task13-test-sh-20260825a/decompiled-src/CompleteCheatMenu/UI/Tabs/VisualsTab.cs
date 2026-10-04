using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class VisualsTab
{
	internal const string Title = "Visuals";

	internal static void Draw()
	{
		Widgets.Section("ESP", delegate
		{
			CheatState.EspEnabled = Widgets.ToggleRow("Enable ESP", CheatState.EspEnabled, CheatState.EspEnabled ? $"{VisualCheats.Targets.Count} tracked" : "off");
			GUILayout.BeginHorizontal();
			CheatState.EspCreatures = GUILayout.Toggle(CheatState.EspCreatures, "  Creatures", Theme.Toggle);
			CheatState.EspItems = GUILayout.Toggle(CheatState.EspItems, "  Items", Theme.Toggle);
			CheatState.EspPlayers = GUILayout.Toggle(CheatState.EspPlayers, "  Players", Theme.Toggle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			CheatState.EspLabels = GUILayout.Toggle(CheatState.EspLabels, "  Names", Theme.Toggle);
			CheatState.EspDistanceLabels = GUILayout.Toggle(CheatState.EspDistanceLabels, "  Distance", Theme.Toggle);
			CheatState.EspTracers = GUILayout.Toggle(CheatState.EspTracers, "  Tracers", Theme.Toggle);
			GUILayout.EndHorizontal();
			CheatState.EspDistance = Widgets.SliderRow("Max distance", CheatState.EspDistance, 20f, 600f, "0", 150f);
			Widgets.Note("Creatures are found by scanning the scene twice a second — the game keeps no live creature list. Raising the distance costs nothing extra.");
		});
		Widgets.Section("Camera", delegate
		{
			if (!VisualCheats.FovAvailable)
			{
				Widgets.Note("PlayerCamera.SetFOV did not resolve — field of view unavailable.");
			}
			else
			{
				float num = Widgets.SliderRow("Field of view", CheatState.Fov, 30f, 140f, "0", 60f);
				if (!Mathf.Approximately(num, CheatState.Fov))
				{
					CheatState.Fov = num;
					VisualCheats.SetFov(num);
				}
			}
			GUILayout.Space(4f);
			if (!VisualCheats.FreeCamAvailable)
			{
				Widgets.Note("Free camera needs a live player camera. Load into a save.");
			}
			else
			{
				bool freeCamActive = VisualCheats.FreeCamActive;
				bool flag = Widgets.ToggleRow("Free camera", freeCamActive, freeCamActive ? "active" : "off");
				if (flag != freeCamActive)
				{
					Notifications.Result(VisualCheats.ToggleFreeCam(flag), flag ? "Free camera on" : "Free camera off", "Free camera toggle failed");
				}
				CheatState.FreeCamSpeed = Widgets.SliderRow("Speed", CheatState.FreeCamSpeed, 1f, 80f, "0.#", 15f);
				CheatState.FreeCamSensitivity = Widgets.SliderRow("Sensitivity", CheatState.FreeCamSensitivity, 0.2f, 8f, "0.##", 2f);
				Widgets.Note("Mouse looks, WASD moves, Space and Ctrl for height, Shift to boost. Your body is now pinned in place while the camera is detached.");
				GUILayout.Space(4f);
				if (!VisualCheats.BodyAvailable)
				{
					Widgets.Note("Own-body rendering unavailable — PlayerSkin.InitializeOther did not resolve.");
				}
				else
				{
					bool bodyShown = VisualCheats.BodyShown;
					bool flag2 = Widgets.ToggleRow("Show my character", bodyShown, bodyShown ? "visible" : "hidden");
					if (flag2 != bodyShown)
					{
						Notifications.Result(VisualCheats.ShowOwnBody(flag2), flag2 ? "Character visible" : "Character hidden", "Could not switch the model");
					}
					Widgets.Note("The game only builds hand meshes for your own player — that's why free camera showed floating hands. This runs the full-body setup it uses for everyone else. Expect it to look wrong in first person; turn it off when you leave free camera.");
				}
			}
		});
		Widgets.Section("Effects", delegate
		{
			bool damageNumbers = VisualCheats.DamageNumbers;
			bool flag = Widgets.ToggleRow("Damage numbers", damageNumbers, damageNumbers ? "on" : "off");
			if (flag != damageNumbers)
			{
				VisualCheats.SetDamageNumbers(flag);
			}
			bool blood = VisualCheats.Blood;
			bool flag2 = Widgets.ToggleRow("Blood", blood, blood ? "on" : "off");
			if (flag2 != blood)
			{
				VisualCheats.SetBlood(flag2);
			}
			GUILayout.BeginHorizontal();
			Widgets.ActionButton("Clear decals", VisualCheats.ClearDecals, "Decals cleared");
			Widgets.ActionButton("Decals off", () => VisualCheats.SetDecals(to: false), "Decals hidden");
			Widgets.ActionButton("Decals on", () => VisualCheats.SetDecals(to: true), "Decals shown");
			GUILayout.EndHorizontal();
		});
	}
}
