using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace Atomic.NetworkTickTuner;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.atomic.networkticktuner";
    private const string PluginName = "Network Tick Tuner";
    private const string PluginVersion = "0.1.0";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<int> _tickRate = null!;
    private ConfigEntry<bool> _changeFixedDeltaTime = null!;
    private ConfigEntry<bool> _logRuntimeValues = null!;
    private bool _applied;
    private float _nextProbe;
    private float _nextNetworkTransformProbe;
    private bool _networkTransformFound;

    private void Awake()
    {
        _enabled = Config.Bind("Network", "Enabled", true, "Apply the configured FishNet tick rate.");
        _tickRate = Config.Bind("Network", "TickRate", 64, "FishNet TimeManager tick rate.");
        _changeFixedDeltaTime = Config.Bind("Network", "ChangeUnityFixedDeltaTime", false, "Set Unity fixedDeltaTime to 1/TickRate.");
        _logRuntimeValues = Config.Bind("Network", "LogRuntimeValues", true, "Log runtime timing values after applying.");
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded; target TickRate={_tickRate.Value}.");
    }

    private void Update()
    {
        if (!_enabled.Value)
            return;
        if (!_applied && Time.unscaledTime >= _nextProbe)
        {
            _nextProbe = Time.unscaledTime + 0.5f;
            TryApply();
        }
        if (_applied && !_networkTransformFound && Time.unscaledTime >= _nextNetworkTransformProbe)
        {
            _nextNetworkTransformProbe = Time.unscaledTime + 2f;
            _networkTransformFound = ProbeNetworkTransforms();
        }
    }

    private bool ProbeNetworkTransforms()
    {
        Type? ntType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("FishNet.Component.Transforming.NetworkTransform", false))
            .FirstOrDefault(t => t != null);
        if (ntType == null)
        {
            Logger.LogWarning("NetworkTransform type not found for runtime probe.");
            return false;
        }

        FieldInfo? interval = ntType.GetField("_interval", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo? position = ntType.GetField("_synchronizePosition", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo? rotation = ntType.GetField("_synchronizeRotation", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo? interpolation = ntType.GetField("_interpolation", BindingFlags.Instance | BindingFlags.NonPublic);
        UnityEngine.Object[] components = Resources.FindObjectsOfTypeAll(ntType);
        Logger.LogInfo($"NetworkTransform probe: count={components.Length}, intervalField={interval != null}, positionField={position != null}, rotationField={rotation != null}, interpolationField={interpolation != null}.");
        for (int i = 0; i < Math.Min(components.Length, 32); i++)
        {
            UnityEngine.Object component = components[i];
            string intervalText = interval?.GetValue(component)?.ToString() ?? "n/a";
            string positionText = position?.GetValue(component)?.ToString() ?? "n/a";
            string rotationText = rotation?.GetValue(component)?.ToString() ?? "n/a";
            string interpolationText = interpolation?.GetValue(component)?.ToString() ?? "n/a";
            Logger.LogInfo($"NetworkTransform[{i}] name={component.name}, interval={intervalText}, pos={positionText}, rot={rotationText}, interpolation={interpolationText}.");
        }
        ProbePlayerNetworkComponents();
        return components.Length > 0;
    }

    private void ProbePlayerNetworkComponents()
    {
        Type? playerType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("PlayerMovement", false))
            .FirstOrDefault(t => t != null);
        if (playerType == null)
        {
            Logger.LogWarning("PlayerMovement type not found for component probe.");
            return;
        }

        UnityEngine.Object[] players = Resources.FindObjectsOfTypeAll(playerType);
        Logger.LogInfo($"PlayerMovement probe: count={players.Length}.");
        for (int i = 0; i < Math.Min(players.Length, 16); i++)
        {
            Component player = players[i] as Component;
            if (player == null) continue;
            Component[] attached = player.GetComponents<Component>();
            string names = string.Join(",", attached.Select(c => c == null ? "<missing>" : c.GetType().FullName));
            Logger.LogInfo($"PlayerMovement[{i}] object={player.name}, components={names}.");
        }
    }
    private void TryApply()
    {
        Type? timeManagerType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("FishNet.Managing.Timing.TimeManager", false))
            .FirstOrDefault(t => t != null);
        if (timeManagerType == null)
            return;

        UnityEngine.Object? manager = Resources.FindObjectsOfTypeAll(timeManagerType).FirstOrDefault();
        if (manager == null)
            return;

        PropertyInfo? tickRateProperty = timeManagerType.GetProperty("TickRate", BindingFlags.Instance | BindingFlags.Public);
        MethodInfo? setTickRate = timeManagerType.GetMethod("SetTickRate", BindingFlags.Instance | BindingFlags.Public);
        if (tickRateProperty == null || setTickRate == null)
        {
            Logger.LogError("FishNet TimeManager TickRate API was not found.");
            _applied = true;
            return;
        }

        int original = Convert.ToInt32(tickRateProperty.GetValue(manager));
        ushort requested = (ushort)Mathf.Clamp(_tickRate.Value, 1, 240);
        setTickRate.Invoke(manager, new object[] { requested });
        int runtime = Convert.ToInt32(tickRateProperty.GetValue(manager));

        if (_changeFixedDeltaTime.Value)
            Time.fixedDeltaTime = 1f / _tickRate.Value;

        if (_logRuntimeValues.Value)
        {
            PropertyInfo? interval = timeManagerType.GetProperty("TimingTickInterval", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object? intervalValue = interval?.GetValue(manager);
            Logger.LogInfo($"TimeManager found: original={original}, runtime={runtime}, interval={intervalValue ?? "n/a"}, fixedDeltaTime={Time.fixedDeltaTime:0.000000}.");
        }

        _networkTransformFound = ProbeNetworkTransforms();
        _applied = runtime == requested;
        if (!_applied)
            Logger.LogError($"TickRate verification failed: expected={requested}, runtime={runtime}.");
    }
}









