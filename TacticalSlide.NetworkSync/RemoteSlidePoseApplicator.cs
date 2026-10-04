using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TacticalSlide.NetworkSync
{
    internal static class RemoteSlidePoseApplicator
    {
        private sealed class Baseline
        {
            internal Vector3 LowerPosition;
            internal Quaternion LowerRotation;
            internal Quaternion[] FootRotations;
        }
        private static readonly Dictionary<OtherPlayer, Baseline> Baselines = new();
        private static readonly Dictionary<OtherPlayer, bool> LastActive = new();
        private static readonly System.Reflection.FieldInfo PlayerRef = AccessTools.Field(typeof(OtherPlayer), "_player");
        private static readonly System.Reflection.FieldInfo FootModelsRef = AccessTools.Field(typeof(PlayerLegs), "_footModels");
        internal static void Install(Harmony harmony, ManualLogSource log)
        {
            var update = AccessTools.Method(typeof(OtherPlayer), "Update");
            if (update == null) throw new MissingMethodException("OtherPlayer.Update");
            harmony.Patch(update, postfix: new HarmonyMethod(typeof(RemoteSlidePoseApplicator), nameof(ApplyAfterUpdate)));
            log.LogInfo("Module 7F remote slide body/feet pose hook installed.");
        }
        private static void ApplyAfterUpdate(OtherPlayer __instance)
        {
            if (__instance == null || __instance.Owner == null || __instance.Owner.IsLocalClient) return;
            if (!OtherPlayerVisualMapping.TryGetState(__instance, out var state)) { RemoteVisualDiagnostics.MissingState(NetworkSyncPlugin.Logger, __instance.Owner.ClientId); return; }
            Player player = PlayerRef?.GetValue(__instance) as Player;
            if (player == null || player.Body == null || player.Body.LowerBody == null) { RemoteVisualDiagnostics.MissingPlayer(NetworkSyncPlugin.Logger, __instance.Owner.ClientId); return; }
            if (!Baselines.TryGetValue(__instance, out var baseline))
            {
                baseline = new Baseline { LowerPosition = player.Body.LowerBody.localPosition, LowerRotation = player.Body.LowerBody.localRotation, FootRotations = CaptureFeet(player.Legs) };
                Baselines[__instance] = baseline;
            }
            float w = Mathf.Clamp01(state.Timeline.Weight);
            bool active = w > 0.001f;
            if (!LastActive.TryGetValue(__instance, out var wasActive) || wasActive != active)
            {
                LastActive[__instance] = active;
                if (active) RemoteVisualDiagnostics.PoseActivated(NetworkSyncPlugin.Logger, __instance.Owner.ClientId, state.LastSequence, state.Timeline.Progress);
                else if (wasActive) RemoteVisualDiagnostics.PoseRestored(NetworkSyncPlugin.Logger, __instance.Owner.ClientId, state.LastSequence);
            }
            if (w <= 0.001f)
            {
                Restore(player, baseline);
                return;
            }
            Vector3 dir = state.Direction ?? __instance.Transform.forward;
            dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
            if (dir.sqrMagnitude < 0.001f) dir = __instance.Transform.forward;
            Quaternion slideRot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(-16f, 0f, 0f);
            player.Body.LowerBody.localPosition = Vector3.Lerp(baseline.LowerPosition, baseline.LowerPosition + Vector3.down * 0.22f, w);
            player.Body.LowerBody.localRotation = Quaternion.Slerp(baseline.LowerRotation, slideRot, w);
            Transform[] feet = FootModelsRef?.GetValue(player.Legs) as Transform[];
            if (feet != null)
            {
                for (int i=0; i<feet.Length && i<baseline.FootRotations.Length; i++)
                    if (feet[i] != null) feet[i].rotation = Quaternion.Slerp(baseline.FootRotations[i], slideRot * Quaternion.Euler(i==0 ? -12f : 18f, i==0 ? 0f : -20f, 0f), w);
            }
        }
        private static Quaternion[] CaptureFeet(PlayerLegs legs)
        {
            Transform[] feet = legs == null ? null : FootModelsRef?.GetValue(legs) as Transform[];
            if (feet == null) return new Quaternion[0];
            var result = new Quaternion[feet.Length];
            for (int i=0; i<feet.Length; i++) result[i] = feet[i] == null ? Quaternion.identity : feet[i].rotation;
            return result;
        }
        private static void Restore(Player player, Baseline baseline)
        {
            player.Body.LowerBody.localPosition = baseline.LowerPosition;
            player.Body.LowerBody.localRotation = baseline.LowerRotation;
            Transform[] feet = FootModelsRef?.GetValue(player.Legs) as Transform[];
            if (feet == null) return;
            for (int i=0; i<feet.Length && i<baseline.FootRotations.Length; i++) if (feet[i] != null) feet[i].rotation = baseline.FootRotations[i];
        }
    }
}


