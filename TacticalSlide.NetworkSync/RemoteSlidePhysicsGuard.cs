using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
namespace TacticalSlide.NetworkSync
{
    internal static class RemoteSlidePhysicsGuard
    {
        internal static void Install(Harmony harmony, ManualLogSource log)
        {
            // Validate every target before applying any patch. Do not skip base game methods.
            var targets = new List<MethodInfo>();
            Type controller = AccessTools.TypeByName("TacticalSlideMod.SlideController");
            if (controller == null) throw new MissingMemberException("SlideController");
            foreach (string name in new[] { "CheckAndConsumeSlideInput", "ApplySlidePhysics", "OnSlideHop" })
                targets.Add(Require(AccessTools.Method(controller, name), name));
            Type crouch = AccessTools.TypeByName("TacticalSlideMod.Patches.PlayerMovementPatch+PlayerMovement_Crouch_Patch");
            if (crouch == null) throw new MissingMemberException("slide crouch postfix type");
            targets.Add(Require(AccessTools.Method(crouch, "Postfix"), "slide crouch postfix"));
            var guard = new HarmonyMethod(typeof(RemoteSlidePhysicsGuard), nameof(AllowLocalMovement));
            foreach (MethodInfo target in targets) harmony.Patch(target, prefix: guard);
            log.LogInfo("Module 7 physics guard installed: input, velocity/slope, hop, slide crouch postfix; base game movement preserved.");
        }
        private static MethodInfo Require(MethodInfo method, string name)
        {
            if (method == null || method.GetParameters().Length == 0 || method.GetParameters()[0].ParameterType != typeof(PlayerMovement))
                throw new MissingMethodException("Module 7 incompatible target: " + name);
            return method;
        }
        private static bool AllowLocalMovement(PlayerMovement __0)
        {
            bool exists = __0 != null;
            var owner = exists ? __0.Owner : null;
            return SlideAuthorityPolicy.CanDrive(exists, owner != null, owner != null && owner.IsLocalClient);
        }
    }
}
