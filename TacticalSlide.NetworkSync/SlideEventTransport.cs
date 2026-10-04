using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using FishNet.Broadcast;
using FishNet;
using FishNet.Connection;
using FishNet.Serializing;
using FishNet.Transporting;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using HarmonyLib;
using UnityEngine;

namespace TacticalSlide.NetworkSync;

public enum SlideEventKind : byte
{
    StartSlide = 1,
    SlideHop = 2,
    EndSlide = 3
}

public struct SlideEventBroadcastV2 : IBroadcast
{
    public int SenderClientId;
    public uint Sequence;
    public SlideEventKind Kind;
    public Vector3 Direction;
    public ushort DurationMilliseconds;
}

internal sealed class SlideEventTransport : MonoBehaviour
{
    private static readonly Dictionary<int, uint> LastSequenceByClient = new();
    private static readonly Dictionary<int, RemoteSlideVisualState> VisualStatesByClient = new();
    internal static bool TryGetVisualState(int clientId, out RemoteSlideVisualState state) => VisualStatesByClient.TryGetValue(clientId, out state);
    private static uint _localSequence;
    private static readonly ManagerBinding<ClientManager> ClientBinding = new();
    private static readonly ManagerBinding<ServerManager> ServerBinding = new();
    private float _nextBindingCheck;
    private bool _clientWasStarted;

    private void Update()
    {
        if (!_registered || Time.unscaledTime < _nextBindingCheck) return;
        _nextBindingCheck = Time.unscaledTime + 0.5f;
        RefreshBindings();
        bool started = ClientBinding.Current != null && ClientBinding.Current.Started;
        if (_clientWasStarted && !started) LastSequenceByClient.Clear(); VisualStatesByClient.Clear();
        _clientWasStarted = started;
    }

    private static void RefreshBindings()
    {
        try
        {
            var client = InstanceFinder.ClientManager;
            if (ClientBinding.Bind(client == null ? null : client, AttachClient, DetachClient))
                LastSequenceByClient.Clear(); VisualStatesByClient.Clear();
        }
        catch (Exception e) { NetworkSyncPlugin.Logger?.LogWarning("Module 6 client binding retry: " + e.Message); }
        try
        {
            var server = InstanceFinder.ServerManager;
            ServerBinding.Bind(server == null ? null : server, AttachServer, DetachServer);
        }
        catch (Exception e) { NetworkSyncPlugin.Logger?.LogWarning("Module 6 server binding retry: " + e.Message); }
    }
    private static void AttachClient(ClientManager manager)
    {
        manager.RegisterBroadcast<SlideEventBroadcastV2>(OnClientBroadcast);
        NetworkSyncPlugin.Logger?.LogInfo("Module 6 client receiver bound; started=" + manager.Started);
    }
    private static void AttachServer(ServerManager manager)
    {
        manager.RegisterBroadcast<SlideEventBroadcastV2>(OnServerBroadcast);
        NetworkSyncPlugin.Logger?.LogInfo("Module 6 server receiver bound; started=" + manager.Started);
    }
    private static void DetachClient(ClientManager manager)
    {
        if (manager != null) manager.UnregisterBroadcast<SlideEventBroadcastV2>(OnClientBroadcast);
    }
    private static void DetachServer(ServerManager manager)
    {
        if (manager != null) manager.UnregisterBroadcast<SlideEventBroadcastV2>(OnServerBroadcast);
    }
    private static bool _registered;
    private static Harmony? _harmony;
    private static Type? _slideControllerType;
    private static System.Reflection.PropertyInfo? _isSliding;
    private static System.Reflection.FieldInfo? _slideTimer;
    private static ushort _durationMs;
    private struct Transition { internal bool Local; internal bool Before; }
    private static System.Reflection.PropertyInfo? _slideDirection;

    internal static void Install(ManualLogSource logger, Harmony harmony)
    {
        if (_registered) return;
        _registered = true;
        _harmony = harmony;
        RegisterSerializers(logger);
        _slideControllerType = AccessTools.TypeByName("TacticalSlideMod.SlideController");
        if (_slideControllerType != null)
        {
            _isSliding = AccessTools.Property(_slideControllerType, "IsSliding");
            _slideTimer = AccessTools.Field(_slideControllerType, "_slideTimer");
            if (_isSliding == null || _slideTimer == null) throw new MissingMemberException("Slide lifecycle state/timer missing");
            _slideDirection = AccessTools.Property(_slideControllerType, "CurrentSlideDir");
            harmony.Patch(AccessTools.Method(_slideControllerType, "StartSlide"), prefix: new HarmonyMethod(typeof(SlideEventTransport), nameof(BeforeMovement)), postfix: new HarmonyMethod(typeof(SlideEventTransport), nameof(OnStartSlide)));
            harmony.Patch(AccessTools.Method(_slideControllerType, "OnSlideHop"), prefix: new HarmonyMethod(typeof(SlideEventTransport), nameof(BeforeMovement)), postfix: new HarmonyMethod(typeof(SlideEventTransport), nameof(OnSlideHop)));
            harmony.Patch(AccessTools.Method(_slideControllerType, "EndSlide"), prefix: new HarmonyMethod(typeof(SlideEventTransport), nameof(BeforeMovement)), postfix: new HarmonyMethod(typeof(SlideEventTransport), nameof(OnEndSlide)));
            harmony.Patch(AccessTools.Method(_slideControllerType, "CancelSlide"), prefix: new HarmonyMethod(typeof(SlideEventTransport), nameof(BeforeCancel)), postfix: new HarmonyMethod(typeof(SlideEventTransport), nameof(OnEndSlide)));
            logger.LogInfo("Module 6 V2 transition hooks installed; duration unit=milliseconds.");
        }
        else
        {
            logger.LogWarning("Module 6 SlideController type was not found.");
        }
        RefreshBindings();
        logger.LogInfo("Module 6 lifecycle hooks ready; receiver binding follows manager availability.");
    }

    private static void RegisterSerializers(ManualLogSource logger)
    {
        GenericWriter<SlideEventBroadcastV2>.SetWrite(WriteSlideEvent);
        GenericReader<SlideEventBroadcastV2>.SetRead(ReadSlideEvent);
        logger.LogInfo("Module 6 custom SlideEventBroadcastV2 serializers registered.");
    }

    private static void WriteSlideEvent(Writer writer, SlideEventBroadcastV2 value)
    {
        writer.WriteInt32(value.SenderClientId);
        writer.WriteUInt32(value.Sequence);
        writer.WriteByte((byte)value.Kind);
        writer.WriteVector3(value.Direction);
        writer.WriteUInt16(value.DurationMilliseconds);
    }

    private static SlideEventBroadcastV2 ReadSlideEvent(Reader reader)
    {
        return new SlideEventBroadcastV2
        {
            SenderClientId = reader.ReadInt32(),
            Sequence = reader.ReadUInt32(),
            Kind = (SlideEventKind)reader.ReadByte(),
            Direction = reader.ReadVector3(),
            DurationMilliseconds = reader.ReadUInt16()
        };
    }
    internal static void Uninstall()
    {
        if (!_registered) return;
        try { ClientBinding.Clear(DetachClient); }
        finally { ServerBinding.Clear(DetachServer); LastSequenceByClient.Clear(); VisualStatesByClient.Clear(); }
        _registered = false;
    }

    private static bool Sliding() => _isSliding?.GetValue(null) is bool value && value;
    private static void BeforeMovement(PlayerMovement __0, out Transition __state)
    {
        __state = new Transition { Local = __0 != null && __0.Owner != null && __0.Owner.IsLocalClient, Before = Sliding() };
    }
    private static void BeforeCancel(out Transition __state)
    {
        var local = Player.LocalPlayer;
        __state = new Transition { Local = local != null && local.Owner != null && local.Owner.IsLocalClient, Before = Sliding() };
    }
    private static void OnStartSlide(object __instance, Transition __state)
    {
        if (!SlideEventRules.IsTransition(1, __state.Local, __state.Before, Sliding())) return;
        _durationMs = _slideTimer?.GetValue(__instance) is float seconds ? SlideEventRules.DurationMs(seconds) : (ushort)0;
        if (_durationMs == 0) { NetworkSyncPlugin.Logger?.LogWarning("Module 6 V2 start omitted: duration outside (0,10] seconds."); return; }
        Send(SlideEventKind.StartSlide);
    }
    private static void OnSlideHop(Transition __state)
    {
        if (SlideEventRules.IsTransition(2, __state.Local, __state.Before, Sliding())) Send(SlideEventKind.SlideHop);
    }
    private static void OnEndSlide(Transition __state)
    {
        if (SlideEventRules.IsTransition(3, __state.Local, __state.Before, Sliding())) Send(SlideEventKind.EndSlide);
    }

    private static void Send(SlideEventKind kind)
    {
        if (!_registered) { NetworkSyncPlugin.Logger?.LogInfo("Module 6 send skipped: transport not registered"); return; }
        if (InstanceFinder.ClientManager == null) { NetworkSyncPlugin.Logger?.LogInfo("Module 6 send skipped: ClientManager null"); return; }
        if (!InstanceFinder.ClientManager.Started) { NetworkSyncPlugin.Logger?.LogInfo("Module 6 send skipped: ClientManager not started"); return; }
        RefreshBindings();
        Player local = Player.LocalPlayer;
        if (local == null) { NetworkSyncPlugin.Logger?.LogInfo("Module 6 send skipped: Player.LocalPlayer null"); return; }
        if (local.Owner == null) { NetworkSyncPlugin.Logger?.LogInfo("Module 6 send skipped: local Owner null"); return; }
        SlideEventBroadcastV2 message = new SlideEventBroadcastV2
        {
            SenderClientId = local.Owner.ClientId,
            Sequence = ++_localSequence,
            Kind = kind,
            Direction = ReadDirection(),
            DurationMilliseconds = _durationMs
        };
        NetworkSyncPlugin.Logger?.LogInfo($"Module 6 local slide event: kind={message.Kind}, seq={message.Sequence}, client={message.SenderClientId}.");
        InstanceFinder.ClientManager.Broadcast(message, Channel.Reliable);
    }

    private static Vector3 ReadDirection()
    {
        object? value = _slideDirection?.GetValue(null);
        return value is Vector3 direction && direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
    }

    private static void OnServerBroadcast(NetworkConnection connection, SlideEventBroadcastV2 message, Channel channel)
    {
        if (InstanceFinder.ServerManager == null || !InstanceFinder.ServerManager.Started) return;
        if (!Valid(message)) return;
        message.SenderClientId = connection.ClientId;
        NetworkSyncPlugin.Logger?.LogInfo($"Module 6 server received slide event: kind={message.Kind}, seq={message.Sequence}, client={message.SenderClientId}.");
        InstanceFinder.ServerManager.Broadcast(message, requireAuthenticated: true, channel: Channel.Reliable);
    }

    private static bool Valid(SlideEventBroadcastV2 m) => SlideEventRules.Valid((byte)m.Kind, m.DurationMilliseconds, m.Direction.x, m.Direction.y, m.Direction.z);

    private static void OnClientBroadcast(SlideEventBroadcastV2 message, Channel channel)
    {
        if (!Valid(message)) return;
        Player local = Player.LocalPlayer;
        if (local != null && local.Owner != null && message.SenderClientId == local.Owner.ClientId) return;
        if (LastSequenceByClient.TryGetValue(message.SenderClientId, out uint previous) && unchecked((int)(message.Sequence - previous)) <= 0) return;
        LastSequenceByClient[message.SenderClientId] = message.Sequence;
        if (!VisualStatesByClient.TryGetValue(message.SenderClientId, out var visual)) { visual = new RemoteSlideVisualState(); VisualStatesByClient[message.SenderClientId] = visual; }
        if (!visual.Apply(message.Kind, message.Sequence, message.Direction, Time.unscaledTime, message.DurationMilliseconds)) return;
        visual.Tick(Time.unscaledTime, NetworkSyncPlugin.DelayMs.Value / 1000.0);
        NetworkSyncPlugin.Logger?.LogInfo($"Module 6 remote slide event: sender={message.SenderClientId}, kind={message.Kind}, seq={message.Sequence}, durationMs={message.DurationMilliseconds}.");
    }
}







