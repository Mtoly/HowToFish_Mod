using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx;
using CompleteCheatMenu.Cheats;

namespace CompleteCheatMenu.Runtime;

internal static class Presets
{
	private static string Dir => Path.Combine(Paths.ConfigPath, "CompleteCheatMenu-presets");

	internal static List<string> List()
	{
		try
		{
			if (!Directory.Exists(Dir))
			{
				return new List<string>();
			}
			return Directory.GetFiles(Dir, "*.txt").Select(Path.GetFileNameWithoutExtension).OrderBy((string n) => n, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}
		catch
		{
			return new List<string>();
		}
	}

	internal static bool Save(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		try
		{
			Directory.CreateDirectory(Dir);
			List<string> list = new List<string>
			{
				F("flySpeed", CheatState.FlySpeed),
				F("espDistance", CheatState.EspDistance),
				B("espEnabled", CheatState.EspEnabled),
				B("espCreatures", CheatState.EspCreatures),
				B("espItems", CheatState.EspItems),
				B("espPlayers", CheatState.EspPlayers),
				B("espTracers", CheatState.EspTracers),
				B("espLabels", CheatState.EspLabels),
				B("espBoxes", CheatState.EspBoxes),
				B("espSkeletons", CheatState.EspSkeletons),
				B("espHealthBars", CheatState.EspHealthBars),
				B("espTypeInfo", CheatState.EspTypeInfo),
				B("espVisibilityInfo", CheatState.EspVisibilityInfo),
				B("espHeldItemInfo", CheatState.EspHeldItemInfo),
				B("espItemValueInfo", CheatState.EspItemValueInfo),
				B("espContainerInfo", CheatState.EspContainerInfo),
				B("espAimCircle", CheatState.EspAimCircle),
				B("espDistanceLabels", CheatState.EspDistanceLabels),
				E("aimTargetMode", (int)CheatState.AimTargets),
				B("aimbot", CheatState.Aimbot),
				B("aimRequireLineOfSight", CheatState.AimRequireLineOfSight),
				B("aimExcludeDead", CheatState.AimExcludeDead),
				B("aimExcludeSeagulls", CheatState.AimExcludeSeagulls),
				B("aimExcludeHookedFish", CheatState.AimExcludeHookedFish),
				B("aimPrioritizeAlbatross", CheatState.AimPrioritizeAlbatross),
				F("aimFov", CheatState.AimFov),
				F("aimRange", CheatState.AimRange),
				F("aimHeightOffset", CheatState.AimHeightOffset),
				F("aimDistanceWeight", CheatState.AimDistanceWeight),
				F("aimAngleWeight", CheatState.AimAngleWeight),
				F("aimScreenRadius", CheatState.AimScreenRadius),
				F("aimScreenDistanceWeight", CheatState.AimScreenDistanceWeight),
				F("aimPriorityBonus", CheatState.AimPriorityBonus),
				F("aimPriorityBonusLimit", CheatState.AimPriorityBonusLimit),
				F("aimLockDuration", CheatState.AimLockDuration),
				F("aimSwitchPenalty", CheatState.AimSwitchPenalty),
				B("aimPrediction", CheatState.AimPrediction),
				F("aimVelocityMinInterval", CheatState.AimVelocityMinInterval),
				F("aimTeleportDistance", CheatState.AimTeleportDistance),
				F("aimMaxTargetSpeed", CheatState.AimMaxTargetSpeed),
				B("aimGravityCompensation", CheatState.AimGravityCompensation),
				F("aimGravityScale", CheatState.AimGravityScale),
				F("aimTrackingChance", CheatState.AimTrackingChance),
				B("aimTrackingDiagnostics", CheatState.AimTrackingDiagnostics),
				B("aimVisibleAssist", CheatState.AimVisibleAssist),
				F("aimVisibleResponse", CheatState.AimVisibleResponse),
				F("aimDistanceDropPerUnit", CheatState.AimDistanceDropPerUnit),
				B("aimRecoilCompensation", CheatState.AimRecoilCompensation),
				F("aimRecoilCompensationStrength", CheatState.AimRecoilCompensationStrength),
				B("instantHit", CheatState.InstantHit),
				F("instantHitProjectileSpeed", CheatState.InstantHitProjectileSpeed),
				F("fov", CheatState.Fov),
				F("freeCamSpeed", CheatState.FreeCamSpeed),
				F("itemCookness", CheatState.ItemCookness),
				F("itemWeight", CheatState.ItemWeight),
				F("itemKillScore", CheatState.ItemKillScore),
				F("itemBetting", CheatState.ItemBetting)
			};
			string[] fields = MovementCheats.Fields;
			foreach (string text in fields)
			{
				list.Add(F("move." + text, MovementCheats.Get(text)));
			}
			File.WriteAllLines(Path.Combine(Dir, Sanitize(name) + ".txt"), list);
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Preset save failed: " + ex.Message));
			return false;
		}
	}

	internal static bool Load(string name)
	{
		try
		{
			string path = Path.Combine(Dir, Sanitize(name) + ".txt");
			if (!File.Exists(path))
			{
				return false;
			}
			string[] array = File.ReadAllLines(path);
			bool? legacyCreatures = null;
			bool? legacyPlayers = null;
			bool loadedTargetMode = false;
			foreach (string text in array)
			{
				int num = text.IndexOf('=');
				if (num <= 0)
				{
					continue;
				}
				string text2 = text.Substring(0, num).Trim();
				string text3 = text.Substring(num + 1).Trim();
				if (text2.StartsWith("move.", StringComparison.Ordinal))
				{
					if (TryFloat(text3, out var value))
					{
						MovementCheats.Set(text2.Substring(5), value);
					}
					continue;
				}
				switch (text2)
				{
				case "flySpeed":
				{
					if (TryFloat(text3, out var value9))
					{
						CheatState.FlySpeed = value9;
					}
					break;
				}
				case "espDistance":
				{
					if (TryFloat(text3, out var value7))
					{
						CheatState.EspDistance = value7;
					}
					break;
				}
				case "espEnabled":
					CheatState.EspEnabled = text3 == "1";
					break;
				case "espCreatures":
					CheatState.EspCreatures = text3 == "1";
					break;
				case "espItems":
					CheatState.EspItems = text3 == "1";
					break;
				case "espPlayers":
					CheatState.EspPlayers = text3 == "1";
					break;
				case "espTracers":
					CheatState.EspTracers = text3 == "1";
					break;
				case "espLabels":
					CheatState.EspLabels = text3 == "1";
					break;
				case "espBoxes": CheatState.EspBoxes = text3 == "1"; break;
				case "espSkeletons": CheatState.EspSkeletons = text3 == "1"; break;
				case "espHealthBars": CheatState.EspHealthBars = text3 == "1"; break;
				case "espTypeInfo": CheatState.EspTypeInfo = text3 == "1"; break;
				case "espVisibilityInfo": CheatState.EspVisibilityInfo = text3 == "1"; break;
				case "espHeldItemInfo": CheatState.EspHeldItemInfo = text3 == "1"; break;
				case "espItemValueInfo": CheatState.EspItemValueInfo = text3 == "1"; break;
				case "espContainerInfo": CheatState.EspContainerInfo = text3 == "1"; break;
				case "espAimCircle": CheatState.EspAimCircle = text3 == "1"; break;
				case "espDistanceLabels": CheatState.EspDistanceLabels = text3 == "1"; break;
				case "aimTargetMode":
					if (int.TryParse(text3, NumberStyles.Integer, CultureInfo.InvariantCulture, out var targetMode) && Enum.IsDefined(typeof(AimTargetMode), targetMode)) { CheatState.AimTargets = (AimTargetMode)targetMode; loadedTargetMode = true; }
					break;
				case "aimAtCreatures": legacyCreatures = text3 == "1"; break;
				case "aimAtPlayers": legacyPlayers = text3 == "1"; break;
				case "aimbot": CheatState.Aimbot = text3 == "1"; break;
				case "aimRequireLineOfSight": CheatState.AimRequireLineOfSight = text3 == "1"; break;
				case "aimExcludeDead": CheatState.AimExcludeDead = text3 == "1"; break;
				case "aimExcludeSeagulls": CheatState.AimExcludeSeagulls = text3 == "1"; break;
				case "aimExcludeHookedFish": CheatState.AimExcludeHookedFish = text3 == "1"; break;
				case "aimPrioritizeAlbatross": CheatState.AimPrioritizeAlbatross = text3 == "1"; break;
				case "aimFov": if (TryFloat(text3, out var aimFov)) CheatState.AimFov = aimFov; break;
				case "aimRange": if (TryFloat(text3, out var aimRange)) CheatState.AimRange = aimRange; break;
				case "aimHeightOffset": if (TryFloat(text3, out var aimHeightOffset)) CheatState.AimHeightOffset = aimHeightOffset; break;
				case "aimDistanceWeight": if (TryFloat(text3, out var aimDistanceWeight)) CheatState.AimDistanceWeight = aimDistanceWeight; break;
				case "aimAngleWeight": if (TryFloat(text3, out var aimAngleWeight)) CheatState.AimAngleWeight = aimAngleWeight; break;
				case "aimScreenRadius": if (TryFloat(text3, out var aimScreenRadius)) CheatState.AimScreenRadius = aimScreenRadius; break;
				case "aimScreenDistanceWeight": if (TryFloat(text3, out var aimScreenDistanceWeight)) CheatState.AimScreenDistanceWeight = aimScreenDistanceWeight; break;
				case "aimPriorityBonus": if (TryFloat(text3, out var aimPriorityBonus)) CheatState.AimPriorityBonus = aimPriorityBonus; break;
				case "aimPriorityBonusLimit": if (TryFloat(text3, out var aimPriorityBonusLimit)) CheatState.AimPriorityBonusLimit = aimPriorityBonusLimit; break;
				case "aimLockDuration": if (TryFloat(text3, out var aimLockDuration)) CheatState.AimLockDuration = aimLockDuration; break;
				case "aimSwitchPenalty": if (TryFloat(text3, out var aimSwitchPenalty)) CheatState.AimSwitchPenalty = aimSwitchPenalty; break;
				case "aimPrediction": CheatState.AimPrediction = text3 == "1"; break;
				case "aimVelocityMinInterval": if (TryFloat(text3, out var aimVelocityMinInterval)) CheatState.AimVelocityMinInterval = aimVelocityMinInterval; break;
				case "aimTeleportDistance": if (TryFloat(text3, out var aimTeleportDistance)) CheatState.AimTeleportDistance = aimTeleportDistance; break;
				case "aimMaxTargetSpeed": if (TryFloat(text3, out var aimMaxTargetSpeed)) CheatState.AimMaxTargetSpeed = aimMaxTargetSpeed; break;
				case "aimGravityCompensation": CheatState.AimGravityCompensation = text3 == "1"; break;
				case "aimGravityScale": if (TryFloat(text3, out var aimGravityScale)) CheatState.AimGravityScale = aimGravityScale; break;
				case "aimTrackingChance": if (TryFloat(text3, out var aimTrackingChance)) CheatState.AimTrackingChance = aimTrackingChance; break;
				case "aimTrackingDiagnostics": CheatState.AimTrackingDiagnostics = text3 == "1"; break;
				case "aimVisibleAssist": CheatState.AimVisibleAssist = text3 == "1"; break;
				case "aimVisibleResponse": if (TryFloat(text3, out var aimVisibleResponse)) CheatState.AimVisibleResponse = aimVisibleResponse; break;
				case "aimDistanceDropPerUnit": if (TryFloat(text3, out var aimDistanceDrop)) CheatState.AimDistanceDropPerUnit = aimDistanceDrop; break;
				case "aimRecoilCompensation": CheatState.AimRecoilCompensation = text3 == "1"; break;
				case "aimRecoilCompensationStrength": if (TryFloat(text3, out var aimRecoilStrength)) CheatState.AimRecoilCompensationStrength = aimRecoilStrength; break;
				case "instantHit": CheatState.InstantHit = text3 == "1"; break;
				case "instantHitProjectileSpeed": if (TryFloat(text3, out var instantHitSpeed)) CheatState.InstantHitProjectileSpeed = instantHitSpeed; break;
				case "fov":
				{
					if (TryFloat(text3, out var value8))
					{
						CheatState.Fov = value8;
						VisualCheats.SetFov(value8);
					}
					break;
				}
				case "freeCamSpeed":
				{
					if (TryFloat(text3, out var value6))
					{
						CheatState.FreeCamSpeed = value6;
					}
					break;
				}
				case "itemCookness":
				{
					if (TryFloat(text3, out var value5))
					{
						CheatState.ItemCookness = value5;
					}
					break;
				}
				case "itemWeight":
				{
					if (TryFloat(text3, out var value4))
					{
						CheatState.ItemWeight = value4;
					}
					break;
				}
				case "itemKillScore":
				{
					if (TryFloat(text3, out var value3))
					{
						CheatState.ItemKillScore = value3;
					}
					break;
				}
				case "itemBetting":
				{
					if (TryFloat(text3, out var value2))
					{
						CheatState.ItemBetting = value2;
					}
					break;
				}
				}
			}
			if (!loadedTargetMode) ApplyLegacyTargetMode(legacyCreatures, legacyPlayers);
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Preset load failed: " + ex.Message));
			return false;
		}
	}

	internal static bool Delete(string name)
	{
		try
		{
			string path = Path.Combine(Dir, Sanitize(name) + ".txt");
			if (!File.Exists(path))
			{
				return false;
			}
			File.Delete(path);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string F(string key, float value)
	{
		return key + "=" + value.ToString("R", CultureInfo.InvariantCulture);
	}

	private static string B(string key, bool value)
	{
		return key + "=" + (value ? "1" : "0");
	}

	private static string E(string key, int value)
	{
		return key + "=" + value.ToString(CultureInfo.InvariantCulture);
	}

	private static void ApplyLegacyTargetMode(bool? creatures, bool? players)
	{
		if (!creatures.HasValue && !players.HasValue) return;
		bool includeCreatures = creatures ?? CheatState.AimTargetsCreatures;
		bool includePlayers = players ?? CheatState.AimTargetsPlayers;
		if (includeCreatures && includePlayers) CheatState.AimTargets = AimTargetMode.CreaturesAndPlayers;
		else if (includePlayers) CheatState.AimTargets = AimTargetMode.Players;
		else CheatState.AimTargets = AimTargetMode.Creatures;
	}

	private static bool TryFloat(string s, out float value)
	{
		return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
	}

	private static string Sanitize(string name)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char oldChar in invalidFileNameChars)
		{
			name = name.Replace(oldChar, '_');
		}
		return name.Trim();
	}
}
