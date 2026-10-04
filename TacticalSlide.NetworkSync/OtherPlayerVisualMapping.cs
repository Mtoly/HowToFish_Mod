using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TacticalSlide.NetworkSync
{
    internal static class OtherPlayerVisualMapping
    {
        private static readonly Dictionary<OtherPlayer, RemoteSlideVisualState> States = new();
        internal static bool TryGetState(OtherPlayer player, out RemoteSlideVisualState state) => States.TryGetValue(player, out state);
        private static float _nextScan;
        internal static void Install(Harmony harmony, ManualLogSource log)
        {
            var update = AccessTools.Method(typeof(OtherPlayer), "Update");
            if (update == null) throw new MissingMethodException("OtherPlayer.Update");
            harmony.Patch(update, postfix: new HarmonyMethod(typeof(OtherPlayerVisualMapping), nameof(OnUpdate)));
            log.LogInfo("Module 7D OtherPlayer visual mapping hook installed.");
        }
        private static void OnUpdate(OtherPlayer __instance)
        {
            if (__instance == null) return;
            RemoteVisualDiagnostics.OtherPlayerUpdate(NetworkSyncPlugin.Logger, __instance);
            if (__instance.Owner == null || __instance.Owner.IsLocalClient) return;
            int clientId = __instance.Owner.ClientId;
            if (SlideEventTransport.TryGetVisualState(clientId, out var networkState))
            {
                if (!States.TryGetValue(__instance, out var existing) || !object.ReferenceEquals(existing, networkState))
                {
                    States[__instance] = networkState;
                    RemoteVisualDiagnostics.StateAdopted(NetworkSyncPlugin.Logger, clientId, networkState.LastSequence);
                }
            }
            if (!States.TryGetValue(__instance, out var state))
            {
                state = new RemoteSlideVisualState();
                States[__instance] = state;
            }
            state.Tick(Time.unscaledTime, NetworkSyncPlugin.DelayMs.Value / 1000.0);
            RemoteVisualDiagnostics.Summary(NetworkSyncPlugin.Logger);
            // Position interpolation remains authoritative. This hook exposes only visual state;
            // no transform, Rigidbody, collider, or input mutation occurs here.
        }
    }
}


