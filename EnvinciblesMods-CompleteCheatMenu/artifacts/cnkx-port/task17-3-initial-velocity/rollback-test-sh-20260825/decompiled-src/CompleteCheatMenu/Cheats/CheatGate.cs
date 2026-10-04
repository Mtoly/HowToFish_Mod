using System;
using System.IO;
using System.Reflection;
using CompleteCheatMenu.Game;
using UnityEngine;

namespace CompleteCheatMenu.Cheats;

internal static class CheatGate
{
	private static bool _backupDone;

	internal static bool CheatsEnabled
	{
		get
		{
			PropertyInfo propertyInfo = GameBinder.Property("ClientSettings", "CheatsEnabled");
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

	internal static bool IsHost => Refs.IsHost;

	internal static bool InGame => Refs.InGame;

	internal static bool LocalFeaturesAvailable => InGame;

	internal static bool ServerFeaturesAvailable => InGame && IsHost;

	internal static bool EnableCheats(bool to)
	{
		bool num = GameBinder.TryInvoke(GameBinder.Method("ClientSettings", "ToggleCheats", 1), null, to);
		if (num)
		{
			Plugin.Log.LogInfo((object)("Developer cheats " + (to ? "enabled" : "disabled") + "."));
		}
		return num;
	}

	internal static void EnsureEnabled()
	{
		if (!CheatsEnabled)
		{
			EnableCheats(to: true);
		}
	}

	internal static string PageBlockReason()
	{
		if (!InGame)
		{
			return "Load into a save first.";
		}
		return null;
	}

	internal static string ServerBlockReason()
	{
		if (!InGame)
		{
			return "Load into a save first.";
		}
		if (!IsHost)
		{
			return "此操作需要由房主同步；本地功能仍可使用。";
		}
		return null;
	}

	internal static void BackupSavesOnce()
	{
		if (_backupDone)
		{
			return;
		}
		_backupDone = true;
		try
		{
			string persistentDataPath = Application.persistentDataPath;
			if (!string.IsNullOrEmpty(persistentDataPath) && Directory.Exists(persistentDataPath))
			{
				string text = Path.Combine(Path.GetDirectoryName(persistentDataPath) ?? persistentDataPath, Path.GetFileName(persistentDataPath) + "_ccm_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
				CopyDirectory(persistentDataPath, text);
				Plugin.Log.LogInfo((object)("Save backup written to " + text));
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Save backup skipped: " + ex.Message));
		}
	}

	private static void CopyDirectory(string src, string dst)
	{
		Directory.CreateDirectory(dst);
		string[] files = Directory.GetFiles(src);
		foreach (string text in files)
		{
			File.Copy(text, Path.Combine(dst, Path.GetFileName(text)), overwrite: true);
		}
		files = Directory.GetDirectories(src);
		foreach (string text2 in files)
		{
			string fileName = Path.GetFileName(text2);
			if (!fileName.StartsWith("ccm_backup", StringComparison.OrdinalIgnoreCase))
			{
				CopyDirectory(text2, Path.Combine(dst, fileName));
			}
		}
	}
}
