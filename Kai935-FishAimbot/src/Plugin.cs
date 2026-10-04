using BepInEx;
using HarmonyLib;

namespace FishAimbotMod
{
    [BepInPlugin("com.yourname.fishaimbot", "Fish Aimbot", "1.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new Harmony("com.yourname.fishaimbot").PatchAll();
            FishTargeting.Initialize();
        }
    }
}
