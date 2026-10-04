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
