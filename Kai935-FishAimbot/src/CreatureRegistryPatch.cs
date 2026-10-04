using HarmonyLib;

namespace FishAimbotMod
{
    [HarmonyPatch(typeof(Creature), "Awake")]
    internal static class CreatureRegistryPatch
    {
        private static void Postfix(Creature __instance)
        {
            FishTargeting.Register(__instance);
        }
    }
}
