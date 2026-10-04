using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using FishNet;
using HarmonyLib;
using UnityEngine;

namespace TacticalSlide.NetworkSync
{
    [BepInPlugin("com.atomic.tacticalslide.networksync", "Tactical Slide Network Sync", "0.2.0")]
    [BepInDependency("com.atomic.tacticalslide")]
    public sealed class NetworkSyncPlugin : BaseUnityPlugin
    {
        internal static ConfigEntry<float> DelayMs;
        internal static ConfigEntry<float> MaxExtrapolationMs;
        internal static ConfigEntry<float> SnapDistance;
        internal static BepInEx.Logging.ManualLogSource Logger;
        private Harmony _harmony;

        private void Awake()
        {
            DelayMs = Config.Bind("RemoteInterpolation", "InterpolationDelayMs", 80f, "Target render delay for remote player snapshots.");
            MaxExtrapolationMs = Config.Bind("RemoteInterpolation", "MaxExtrapolationMs", 50f, "Maximum extrapolation after the newest snapshot.");
            SnapDistance = Config.Bind("RemoteInterpolation", "SnapDistance", 4f, "Distance at which a remote player is corrected immediately.");
            _harmony = new Harmony("com.atomic.tacticalslide.networksync");
            _harmony.PatchAll(typeof(RemoteOtherPlayerPatches));
            Logger = base.Logger;
            RemoteSlidePhysicsGuard.Install(_harmony, Logger);
            OtherPlayerVisualMapping.Install(_harmony, Logger);
            RemoteSlidePoseApplicator.Install(_harmony, Logger);
            gameObject.AddComponent<SlideEventTransport>();
            SlideEventTransport.Install(Logger, _harmony);
            Logger.LogInfo("Tactical Slide Network Sync 0.2.0 loaded; time-buffered OtherPlayer interpolation enabled.");
        }

        private void OnDestroy()
        {
            SlideEventTransport.Uninstall();
            if (_harmony != null) _harmony.UnpatchSelf();
        }
    }

    internal sealed class RemoteSnapshotBuffer
    {
        internal struct Snapshot
        {
            internal float Time;
            internal Vector3 Position;
            internal Vector2 Rotation;
            internal bool OnBoat;
        }

        private readonly List<Snapshot> _items = new List<Snapshot>(8);
        internal Snapshot Latest { get { return _items[_items.Count - 1]; } }
        internal int Count { get { return _items.Count; } }

        internal void Push(Vector3 position, Vector2 rotation, bool onBoat, bool teleport)
        {
            if (teleport) _items.Clear();
            float now = Time.unscaledTime;
            if (_items.Count > 0 && now <= Latest.Time) now = Latest.Time + 0.0001f;
            _items.Add(new Snapshot { Time = now, Position = position, Rotation = rotation, OnBoat = onBoat });
            while (_items.Count > 8) _items.RemoveAt(0);
        }

        internal bool TrySample(float targetTime, out Snapshot sample, out Snapshot previous, out bool extrapolated)
        {
            sample = default(Snapshot); previous = default(Snapshot); extrapolated = false;
            if (_items.Count == 0) return false;
            if (_items.Count == 1) { sample = Latest; return true; }
            for (int i = 1; i < _items.Count; i++)
            {
                Snapshot b = _items[i];
                if (targetTime <= b.Time)
                {
                    Snapshot a = _items[i - 1];
                    float t = Mathf.InverseLerp(a.Time, b.Time, targetTime);
                    sample = new Snapshot { Time = targetTime, Position = Vector3.Lerp(a.Position, b.Position, t), Rotation = Vector2.Lerp(a.Rotation, b.Rotation, t), OnBoat = b.OnBoat };
                    previous = a;
                    return true;
                }
            }
            Snapshot last = Latest;
            Snapshot before = _items[_items.Count - 2];
            float extra = Mathf.Min(targetTime - last.Time, NetworkSyncPlugin.MaxExtrapolationMs.Value / 1000f);
            Vector3 velocity = (last.Position - before.Position) / Mathf.Max(last.Time - before.Time, 0.0001f);
            sample = last;
            sample.Position = last.Position + velocity * extra;
            sample.Time = targetTime;
            previous = before;
            extrapolated = extra > 0f;
            return true;
        }
    }

    [HarmonyPatch]
    internal static class RemoteOtherPlayerPatches
    {
        private static readonly Dictionary<OtherPlayer, RemoteSnapshotBuffer> Buffers = new Dictionary<OtherPlayer, RemoteSnapshotBuffer>();

        private static RemoteSnapshotBuffer BufferFor(OtherPlayer player)
        {
            RemoteSnapshotBuffer buffer;
            if (!Buffers.TryGetValue(player, out buffer))
            {
                buffer = new RemoteSnapshotBuffer();
                Buffers[player] = buffer;
            }
            return buffer;
        }

        [HarmonyPatch(typeof(OtherPlayer), "RpcLogic___SetReceivedPosRot___4106386375")]
        [HarmonyPostfix]
        private static void CaptureSnapshot(OtherPlayer __instance, Vector3 __0, Vector2 __1, bool __2, bool __3)
        {
            if (__instance == null || __instance.Owner == null || __instance.Owner.IsLocalClient) return;
            BufferFor(__instance).Push(__0, __1, __2, __3);
        }

        [HarmonyPatch(typeof(OtherPlayer), "ApplyReceivedPosRot")]
        [HarmonyPrefix]
        private static bool ApplyBufferedSnapshot(OtherPlayer __instance)
        {
            if (__instance == null || __instance.Owner == null || __instance.Owner.IsLocalClient) return true;
            RemoteSnapshotBuffer buffer;
            if (!Buffers.TryGetValue(__instance, out buffer) || buffer.Count == 0) return true;
            float targetTime = Time.unscaledTime - Mathf.Max(0f, NetworkSyncPlugin.DelayMs.Value) / 1000f;
            RemoteSnapshotBuffer.Snapshot sample, previous;
            bool extrapolated;
            if (!buffer.TrySample(targetTime, out sample, out previous, out extrapolated)) return true;
            Transform body = __instance.Transform;
            if (body == null) return true;
            Vector3 desired = sample.Position;
            if (__instance.OnBoat && (bool)BoatManager.Boat && (bool)BoatManager.Boat.VisualBoat)
                desired = BoatManager.Boat.VisualBoat.TransformPoint(sample.Position);
            if (Vector3.Distance(body.position, desired) > NetworkSyncPlugin.SnapDistance.Value)
            {
                body.position = desired;
            }
            else
            {
                body.position = desired;
            }
            body.rotation = Quaternion.Euler(0f, sample.Rotation.y, 0f);
            if (__instance.CamProxy != null)
                __instance.CamProxy.localRotation = Quaternion.Euler(sample.Rotation.x, 0f, 0f);
            return false;
        }
    }
}






