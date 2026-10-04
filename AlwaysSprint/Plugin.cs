using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace HowToFish.AlwaysSprint;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.howtofish.alwayssprint";
    public const string Name = "Always Sprint";
    public const string Version = "1.0.0";

    internal static ConfigEntry<bool> Enabled = null!;
    private ConfigEntry<KeyboardShortcut> menuShortcut = null!;
    private Harmony harmony = null!;
    private bool showMenu;
    private Rect window = new(20, 20, 360, 150);

    private void Awake()
    {
        Enabled = Config.Bind("Always Sprint", "Enabled", true,
            "Keep sprint active while moving. Disable here or in the in-game panel.");
        menuShortcut = Config.Bind("Always Sprint", "Settings shortcut", new KeyboardShortcut(KeyCode.F8),
            "Open the in-game Always Sprint settings panel.");
        harmony = new Harmony(Guid);
        harmony.PatchAll();
        Logger.LogInfo($"{Name} loaded. Enabled={Enabled.Value}");
    }

    private void Update()
    {
        if (menuShortcut.Value.IsDown()) showMenu = !showMenu;
    }

    private void OnGUI()
    {
        if (showMenu) window = GUI.Window(29401, window, DrawWindow, "Always Sprint");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label("Movement setting");
        var value = GUILayout.Toggle(Enabled.Value, "Always sprint while moving");
        if (value != Enabled.Value)
        {
            Enabled.Value = value;
            Enabled.ConfigFile.Save();
        }
        GUILayout.Label($"Status: {(Enabled.Value ? "ON" : "OFF")}");
        GUILayout.Label("Press F8 to close this panel.");
        GUI.DragWindow(new Rect(0, 0, 10000, 24));
    }

    private void OnDestroy() => harmony?.UnpatchSelf();

    [HarmonyPatch(typeof(PlayerMovement), "Move")]
    private static class PlayerMovementMovePatch
    {
        private static readonly AccessTools.FieldRef<PlayerMovement, bool> SprintInput =
            AccessTools.FieldRefAccess<PlayerMovement, bool>("_sprintInput");

        private static void Prefix(PlayerMovement __instance)
        {
            if (Enabled.Value && __instance != null)
                SprintInput(__instance) = true;
        }
    }
}
