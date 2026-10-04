using System;
using BepInEx.Logging;
using UnityEngine;

namespace TacticalSlide.NetworkSync
{
    internal static class RemoteVisualDiagnostics
    {
        private static float _nextSummary;
        internal static int StateAdoptions { get; private set; }
        internal static int PoseActivations { get; private set; }
        internal static int PoseRestorations { get; private set; }
        internal static int MissingStates { get; private set; }
        internal static int MissingPlayers { get; private set; }
        internal static int OtherPlayerUpdates { get; private set; }
        internal static int OwnerlessUpdates { get; private set; }
        internal static int LocalOwnerUpdates { get; private set; }
        internal static int RemoteOwnerUpdates { get; private set; }
        internal static void StateAdopted(ManualLogSource log, int clientId, uint sequence)
        { StateAdoptions++; log.LogInfo($"Module 7 diag: state adopted client={clientId}, seq={sequence}, total={StateAdoptions}."); }
        internal static void PoseActivated(ManualLogSource log, int clientId, uint sequence, float progress)
        { PoseActivations++; log.LogInfo($"Module 7 diag: pose active client={clientId}, seq={sequence}, progress={progress:0.000}, total={PoseActivations}."); }
        internal static void PoseRestored(ManualLogSource log, int clientId, uint sequence)
        { PoseRestorations++; log.LogInfo($"Module 7 diag: pose restored client={clientId}, seq={sequence}, total={PoseRestorations}."); }
        internal static void MissingState(ManualLogSource log, int clientId)
        { MissingStates++; if (MissingStates <= 3) log.LogInfo($"Module 7 diag: no visual state client={clientId}, total={MissingStates}."); }
        internal static void MissingPlayer(ManualLogSource log, int clientId)
        { MissingPlayers++; if (MissingPlayers <= 3) log.LogInfo($"Module 7 diag: OtherPlayer model unavailable client={clientId}, total={MissingPlayers}."); }
        internal static void OtherPlayerUpdate(ManualLogSource log, OtherPlayer player)
        {
            OtherPlayerUpdates++;
            if (player == null || player.Owner == null) { OwnerlessUpdates++; if (OwnerlessUpdates <= 5) log.LogInfo($"Module 7 diag: OtherPlayer.Update owner=null, total={OwnerlessUpdates}."); return; }
            if (player.Owner.IsLocalClient) { LocalOwnerUpdates++; if (LocalOwnerUpdates <= 5) log.LogInfo($"Module 7 diag: OtherPlayer.Update local owner client={player.Owner.ClientId}, total={LocalOwnerUpdates}."); return; }
            RemoteOwnerUpdates++;
            if (RemoteOwnerUpdates <= 10) log.LogInfo($"Module 7 diag: OtherPlayer.Update remote owner client={player.Owner.ClientId}, total={RemoteOwnerUpdates}.");
        }
        internal static void Summary(ManualLogSource log)
        {
            if (Time.unscaledTime < _nextSummary) return;
            _nextSummary = Time.unscaledTime + 5f;
            log.LogInfo($"Module 7 diag summary: adopted={StateAdoptions}, active={PoseActivations}, restored={PoseRestorations}, missingState={MissingStates}, missingPlayer={MissingPlayers}, updates={OtherPlayerUpdates}, ownerless={OwnerlessUpdates}, localOwner={LocalOwnerUpdates}, remoteOwner={RemoteOwnerUpdates}.");
        }
    }
}
