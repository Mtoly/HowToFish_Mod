using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTFMapLoader
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(
        "com.howtofish.devislandunlocked",
        BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.howtofish.maploader";
        public const string Name = "HTF Map Loader";
        public const string Version = "0.2.8";

        internal static Plugin Instance;
        internal static ManualLogSource Log;

        private Harmony _harmony;

        private ConfigEntry<bool> _enabled;
        private ConfigEntry<bool> _verbose;
        private ConfigEntry<bool> _repairRuntime;
        private ConfigEntry<bool> _enableMapWildlife;

        private readonly List<RegisteredMap> _maps =
            new List<RegisteredMap>();

        private readonly Dictionary<byte, RegisteredMap> _mapsByIndex =
            new Dictionary<byte, RegisteredMap>();

        private bool _scanned;
        private bool _registered;
        private bool _loggedDisabled;
        private bool _verifiedRegisteredTriggers;
        private float _managerFirstSeenTime = -1f;

        private bool _bypassQueuePatch;
        private bool _transitioning;
        private RegisteredMap _loadingMap;
        private RegisteredMap _currentMap;

        private object _radarUi;
        private MethodInfo _radarUpdateDotMethod;
        private float _nextRadarAcquire;

        private int _nativeGroundLayer = -1;
        private PhysicsMaterial _nativeGroundMaterial;

        private string LegacyMapsRoot =>
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Maps"
            );

        private string PluginsRoot =>
            Paths.PluginPath;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _enabled = Config.Bind(
                "General",
                "Enabled",
                true,
                "Enable HTF Map Loader."
            );

            _repairRuntime = Config.Bind(
                "General",
                "RepairMissingRuntimeObjects",
                true,
                "Automatically repair common Island/SpawnManager/Level setup when a map is missing it."
            );

            _enableMapWildlife = Config.Bind(
                "General",
                "AllowMapWildlifeOptions",
                true,
                "Allow map.json to request optional built-in Seagull and Clam spawners."
            );

            _verbose = Config.Bind(
                "Debug",
                "VerboseLogging",
                false,
                "Enable verbose Map Loader logging."
            );

            Directory.CreateDirectory(LegacyMapsRoot);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            PatchQueueRequest();
            PatchNativeIslandSpawnerTrigger();

            Logger.LogInfo($"{Name} v{Version} loaded.");
            Logger.LogInfo($"Enabled={_enabled.Value}, RepairRuntime={_repairRuntime.Value}, WildlifeOptions={_enableMapWildlife.Value}");
            Logger.LogInfo($"Legacy maps directory: {LegacyMapsRoot}");
            Logger.LogInfo($"Thunderstore plugin root: {PluginsRoot}");
            Logger.LogInfo(
                "Map packs are discovered recursively from BepInEx/plugins/**/Maps/**/map.json."
            );
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _harmony?.UnpatchSelf();

            foreach (RegisteredMap map in _maps)
            {
                try
                {
                    if (map.Bundle != null)
                        map.Bundle.Unload(false);
                }
                catch
                {
                }
            }
        }

        private void Update()
        {
            if (!_enabled.Value)
            {
                if (!_loggedDisabled)
                {
                    _loggedDisabled = true;
                    Logger.LogWarning(
                        "[GENERAL] HTF Map Loader is disabled by config: [General] Enabled=false."
                    );
                }

                return;
            }

            _loggedDisabled = false;

            if (!_scanned)
            {
                ScanMapFolders();
                _scanned = true;
            }

            if (!_registered && _maps.Count > 0)
                TryRegisterMaps();

            // On How to Fish 1.0.5 the radar can finish rebuilding when the first
            // gameplay island loads. Creating our extra MapDots before that can make
            // them disappear again, so wait for a real gameplay world.
            if (_registered &&
                !_verifiedRegisteredTriggers &&
                IsGameplayWorldReady())
            {
                VerifyRegisteredNavigationTriggers();
                _verifiedRegisteredTriggers = true;
            }

            if (_registered && IsGameplayWorldReady())
                UpdateRadar();
        }

        // ============================================================
        // MAP MANIFEST / DISCOVERY
        // ============================================================

        [Serializable]
        public sealed class MapManifest
        {
            public string id;
            public string name;
            public string author;

            public string bundle;
            public string scene;

            // "auto" or "manual".
            public string positionMode;
            public SerializableVector3 position =
                new SerializableVector3();

            // Move IslandHolder/scene roots to the navigation position.
            // Set false if the scene was deliberately authored/baked at its final
            // world coordinates.
            public bool moveScene = true;

            // #RRGGBB or #RRGGBBAA. Empty = auto palette.
            public string radarColor = "";

            public float islandSize = 55f;

            public RuntimeManifest runtime =
                new RuntimeManifest();

            public WildlifeManifest wildlife =
                new WildlifeManifest();
        }

        [Serializable]
        public sealed class SerializableVector3
        {
            public float x;
            public float y;
            public float z;

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }

        [Serializable]
        public sealed class RuntimeManifest
        {
            // Names searched recursively inside IslandHolder.
            public string playerSpawn = "PlayerSpawnPoint";
            public string boatSpawn = "BoatSpawnPoint";

            // Fallback offsets when the map does not contain these markers.
            public SerializableVector3 defaultPlayerSpawn =
                new SerializableVector3 { x = 0f, y = 6f, z = 0f };

            public SerializableVector3 defaultBoatSpawn =
                new SerializableVector3 { x = 0f, y = 0.5f, z = -25f };
        }

        [Serializable]
        public sealed class WildlifeManifest
        {
            // Opt-in, NEVER forced on every custom map.
            public bool seagulls = false;
            public bool clams = false;

            public int seagullMax = 3;
            public float seagullDelay = 60f;

            public int clamMax = 3;
            public float clamDelay = 15f;
        }

        private sealed class RegisteredMap
        {
            public MapManifest Manifest;
            public string Folder;
            public string BundlePath;
            public string SceneName;
            public string ScenePath;
            public Vector3 Position;
            public Color RadarColor;
            public byte LogicalIndex;

            public AssetBundle Bundle;
            public object MapDot;

            public override string ToString()
            {
                return
                    $"{Manifest?.name ?? Manifest?.id ?? "Map"} " +
                    $"index={LogicalIndex} scene='{SceneName}' pos={Position}";
            }
        }

        private void ScanMapFolders()
        {
            _maps.Clear();

            string[] manifests =
                DiscoverMapManifests();

            Logger.LogInfo(
                $"[MAPS] Found {manifests.Length} map.json file(s) across installed plugin packages."
            );

            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            int order = 0;

            foreach (string manifestPath in manifests)
            {
                try
                {
                    string json =
                        File.ReadAllText(
                            manifestPath
                        );

                    MapManifest manifest =
                        UnityEngine.JsonUtility.FromJson<MapManifest>(
                            json
                        );

                    // Unity JsonUtility is reliable for the simple fields here, but
                    // on some game/Unity builds nested vector-like objects have been
                    // observed to remain at their default (0,0,0) values.
                    // Parse these known Vector3 objects explicitly from the raw JSON
                    // so navigation/spawn coordinates never silently collapse to zero.
                    ApplyExplicitVectorOverrides(
                        manifest,
                        json,
                        manifestPath
                    );

                    if (!ValidateManifest(
                            manifest,
                            manifestPath))
                    {
                        continue;
                    }

                    if (!ids.Add(manifest.id.Trim()))
                    {
                        Logger.LogWarning(
                            $"[MAPS] Duplicate map id '{manifest.id}'. Skipping {manifestPath}."
                        );
                        continue;
                    }

                    string folder =
                        Path.GetDirectoryName(
                            manifestPath
                        );

                    string bundleName =
                        manifest.bundle.Trim();

                    string bundlePath =
                        Path.Combine(
                            folder,
                            bundleName
                        );

                    if (!File.Exists(bundlePath))
                    {
                        Logger.LogWarning(
                            $"[MAPS] '{manifest.id}' bundle does not exist: {bundlePath}"
                        );
                        continue;
                    }

                    RegisteredMap map =
                        new RegisteredMap
                        {
                            Manifest = manifest,
                            Folder = folder,
                            BundlePath = bundlePath,
                            SceneName = manifest.scene != null
                                ? manifest.scene.Trim()
                                : "",
                            Position = ResolveMapPosition(
                                manifest,
                                order
                            ),
                            RadarColor = ResolveRadarColor(
                                manifest,
                                order
                            )
                        };

                    _maps.Add(map);
                    order++;

                    Logger.LogInfo(
                        $"[MAPS] Discovered '{manifest.name ?? manifest.id}' bundle='{bundleName}'."
                    );
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        $"[MAPS] Failed reading '{manifestPath}': {ex.Message}"
                    );
                }
            }
        }

        private string[] DiscoverMapManifests()
        {
            HashSet<string> results =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

            // Backwards compatibility: maps installed manually beside HTFMapLoader.dll.
            AddMapManifestsFromMapsRoot(
                LegacyMapsRoot,
                results
            );

            // Thunderstore/r2modman installs each package in its own folder below
            // BepInEx/plugins. Walk that tree and only scan directories named "Maps".
            // This lets packages such as:
            //
            // BepInEx/plugins/Rompedev-Island9/Maps/Island9/map.json
            //
            // work without copying files into the HTFMapLoader package directory.
            try
            {
                if (Directory.Exists(PluginsRoot))
                {
                    foreach (string mapsDirectory in
                        EnumerateDirectoriesSafe(
                            PluginsRoot,
                            "Maps"))
                    {
                        AddMapManifestsFromMapsRoot(
                            mapsDirectory,
                            results
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    $"[MAPS] Failed scanning plugin packages: {ex.Message}"
                );
            }

            return results
                .OrderBy(
                    p => p,
                    StringComparer.OrdinalIgnoreCase
                )
                .ToArray();
        }

        private void AddMapManifestsFromMapsRoot(
            string mapsRoot,
            HashSet<string> results)
        {
            if (string.IsNullOrWhiteSpace(mapsRoot) ||
                results == null ||
                !Directory.Exists(mapsRoot))
            {
                return;
            }

            try
            {
                foreach (string manifest in
                    Directory.GetFiles(
                        mapsRoot,
                        "map.json",
                        SearchOption.AllDirectories))
                {
                    try
                    {
                        results.Add(
                            Path.GetFullPath(manifest)
                        );
                    }
                    catch
                    {
                        results.Add(manifest);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    $"[MAPS] Could not scan '{mapsRoot}': {ex.Message}"
                );
            }
        }

        private IEnumerable<string> EnumerateDirectoriesSafe(
            string root,
            string wantedName)
        {
            Stack<string> pending =
                new Stack<string>();

            pending.Push(root);

            while (pending.Count > 0)
            {
                string current =
                    pending.Pop();

                string[] children;

                try
                {
                    children =
                        Directory.GetDirectories(current);
                }
                catch
                {
                    continue;
                }

                foreach (string child in children)
                {
                    string name;

                    try
                    {
                        name =
                            Path.GetFileName(
                                child.TrimEnd(
                                    Path.DirectorySeparatorChar,
                                    Path.AltDirectorySeparatorChar
                                )
                            );
                    }
                    catch
                    {
                        continue;
                    }

                    if (string.Equals(
                            name,
                            wantedName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        yield return child;

                        // A Maps directory is already a package content root.
                        // AddMapManifestsFromMapsRoot scans below it recursively.
                        continue;
                    }

                    pending.Push(child);
                }
            }
        }

        private void ApplyExplicitVectorOverrides(
            MapManifest manifest,
            string json,
            string manifestPath)
        {
            if (manifest == null ||
                string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            SerializableVector3 parsed;

            if (TryReadVector3Object(
                    json,
                    "position",
                    out parsed))
            {
                manifest.position =
                    parsed;
            }

            if (manifest.runtime == null)
                manifest.runtime = new RuntimeManifest();

            string runtimeJson;

            if (TryReadObjectBody(
                    json,
                    "runtime",
                    out runtimeJson))
            {
                if (TryReadVector3Object(
                        runtimeJson,
                        "defaultPlayerSpawn",
                        out parsed))
                {
                    manifest.runtime.defaultPlayerSpawn =
                        parsed;
                }

                if (TryReadVector3Object(
                        runtimeJson,
                        "defaultBoatSpawn",
                        out parsed))
                {
                    manifest.runtime.defaultBoatSpawn =
                        parsed;
                }
            }

            if (_verbose != null &&
                _verbose.Value)
            {
                Logger.LogInfo(
                    $"[JSON] Parsed vectors from '{manifestPath}': " +
                    $"position={manifest.position?.ToVector3()}, " +
                    $"defaultPlayerSpawn={manifest.runtime?.defaultPlayerSpawn?.ToVector3()}, " +
                    $"defaultBoatSpawn={manifest.runtime?.defaultBoatSpawn?.ToVector3()}."
                );
            }
        }

        private static bool TryReadVector3Object(
            string json,
            string propertyName,
            out SerializableVector3 value)
        {
            value = null;

            string body;

            if (!TryReadObjectBody(
                    json,
                    propertyName,
                    out body))
            {
                return false;
            }

            float x;
            float y;
            float z;

            if (!TryReadFloatProperty(
                    body,
                    "x",
                    out x) ||
                !TryReadFloatProperty(
                    body,
                    "y",
                    out y) ||
                !TryReadFloatProperty(
                    body,
                    "z",
                    out z))
            {
                return false;
            }

            value =
                new SerializableVector3
                {
                    x = x,
                    y = y,
                    z = z
                };

            return true;
        }

        private static bool TryReadObjectBody(
            string json,
            string propertyName,
            out string body)
        {
            body = null;

            if (string.IsNullOrWhiteSpace(json) ||
                string.IsNullOrWhiteSpace(propertyName))
            {
                return false;
            }

            Match property =
                Regex.Match(
                    json,
                    "\"" +
                    Regex.Escape(propertyName) +
                    "\"\\s*:\\s*\\{",
                    RegexOptions.IgnoreCase
                );

            if (!property.Success)
                return false;

            int openBrace =
                json.IndexOf(
                    '{',
                    property.Index
                );

            if (openBrace < 0)
                return false;

            int depth = 0;
            bool inString = false;
            bool escaped = false;

            for (int i = openBrace;
                 i < json.Length;
                 i++)
            {
                char c =
                    json[i];

                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }

                    if (c == '\\')
                    {
                        escaped = true;
                        continue;
                    }

                    if (c == '"')
                        inString = false;

                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == '{')
                {
                    depth++;
                    continue;
                }

                if (c == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        body =
                            json.Substring(
                                openBrace + 1,
                                i - openBrace - 1
                            );

                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryReadFloatProperty(
            string objectBody,
            string propertyName,
            out float value)
        {
            value = 0f;

            if (string.IsNullOrWhiteSpace(objectBody))
                return false;

            Match match =
                Regex.Match(
                    objectBody,
                    "\"" +
                    Regex.Escape(propertyName) +
                    "\"\\s*:\\s*" +
                    "([-+]?(?:\\d+(?:\\.\\d+)?|\\.\\d+)(?:[eE][-+]?\\d+)?)",
                    RegexOptions.IgnoreCase
                );

            if (!match.Success)
                return false;

            return float.TryParse(
                match.Groups[1].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value
            );
        }

        private bool ValidateManifest(
            MapManifest manifest,
            string manifestPath)
        {
            if (manifest == null)
            {
                Logger.LogWarning(
                    $"[MAPS] Invalid JSON manifest: {manifestPath}."
                );
                return false;
            }

            List<string> missing =
                new List<string>();

            if (string.IsNullOrWhiteSpace(manifest.id))
                missing.Add("id");

            if (string.IsNullOrWhiteSpace(manifest.name))
                missing.Add("name");

            if (string.IsNullOrWhiteSpace(manifest.author))
                missing.Add("author");

            if (string.IsNullOrWhiteSpace(manifest.bundle))
                missing.Add("bundle");

            if (string.IsNullOrWhiteSpace(manifest.scene))
                missing.Add("scene");

            if (string.IsNullOrWhiteSpace(manifest.positionMode))
                missing.Add("positionMode");

            if (missing.Count > 0)
            {
                Logger.LogWarning(
                    $"[MAPS] '{manifestPath}' is missing required field(s): " +
                    string.Join(", ", missing) +
                    ". See MAP_JSON_REFERENCE.md."
                );
                return false;
            }

            string mode =
                manifest.positionMode.Trim();

            if (!string.Equals(
                    mode,
                    "auto",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    mode,
                    "manual",
                    StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning(
                    $"[MAPS] '{manifest.id}' has invalid positionMode='{manifest.positionMode}'. " +
                    "Use 'auto' or 'manual'."
                );
                return false;
            }

            if (string.Equals(
                    mode,
                    "manual",
                    StringComparison.OrdinalIgnoreCase) &&
                manifest.position == null)
            {
                Logger.LogWarning(
                    $"[MAPS] '{manifest.id}' uses manual positioning but has no position object."
                );
                return false;
            }

            if (!string.IsNullOrWhiteSpace(
                    manifest.radarColor))
            {
                Color parsed;

                if (!ColorUtility.TryParseHtmlString(
                        manifest.radarColor.Trim(),
                        out parsed))
                {
                    Logger.LogWarning(
                        $"[MAPS] '{manifest.id}' has invalid radarColor='{manifest.radarColor}'. " +
                        "Use a hex color like #20DFFF."
                    );
                    return false;
                }
            }

            if (manifest.runtime == null)
            {
                Logger.LogWarning(
                    $"[MAPS] '{manifest.id}' is missing runtime. " +
                    "Include the runtime object even if you keep the default marker names."
                );
                return false;
            }

            if (manifest.wildlife == null)
            {
                Logger.LogWarning(
                    $"[MAPS] '{manifest.id}' is missing wildlife. " +
                    "Include the wildlife object and set seagulls/clams explicitly."
                );
                return false;
            }

            return true;
        }

        private Vector3 ResolveMapPosition(
            MapManifest manifest,
            int order)
        {
            if (manifest != null &&
                string.Equals(
                    manifest.positionMode,
                    "manual",
                    StringComparison.OrdinalIgnoreCase))
            {
                return manifest.position != null
                    ? manifest.position.ToVector3()
                    : Vector3.zero;
            }

            // Automatic placement in expanding rings around the native world.
            // Deliberately conservative spacing so map navigation triggers do not
            // overlap each other.
            const float baseRadius = 650f;
            const float ringStep = 450f;
            const int perRing = 8;

            int ring = order / perRing;
            int slot = order % perRing;

            float radius =
                baseRadius +
                ring * ringStep;

            float angle =
                (Mathf.PI * 2f / perRing) *
                slot +
                Mathf.PI * 0.125f;

            return new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
        }

        private Color ResolveRadarColor(
            MapManifest manifest,
            int order)
        {
            Color parsed;

            if (manifest != null &&
                !string.IsNullOrWhiteSpace(manifest.radarColor) &&
                ColorUtility.TryParseHtmlString(
                    manifest.radarColor.Trim(),
                    out parsed))
            {
                parsed.a = 1f;
                return parsed;
            }

            Color[] palette =
            {
                new Color(0.15f, 0.85f, 1.00f, 1f), // cyan
                new Color(0.75f, 0.35f, 1.00f, 1f), // purple
                new Color(1.00f, 0.35f, 0.75f, 1f), // pink
                new Color(0.30f, 1.00f, 0.65f, 1f), // mint
                new Color(0.45f, 0.65f, 1.00f, 1f), // blue
                new Color(1.00f, 0.80f, 0.25f, 1f)  // gold
            };

            return palette[
                Math.Abs(order) % palette.Length
            ];
        }

        // ============================================================
        // ISLAND REGISTRATION
        // ============================================================

        private void TryRegisterMaps()
        {
            object islandManager =
                FindLiveObjectByTypeName(
                    "IslandManager"
                );

            if (islandManager == null)
                return;

            FieldInfo infosField =
                FindField(
                    islandManager.GetType(),
                    "_islandInfos"
                );

            Array infos =
                infosField != null
                    ? infosField.GetValue(islandManager) as Array
                    : null;

            if (infos == null ||
                infos.Length < 5)
            {
                return;
            }

            // Give soft dependencies (notably DevIslandUnlocked) time to register
            // their own islands first. This keeps all indexes append-only.
            if (_managerFirstSeenTime < 0f)
            {
                _managerFirstSeenTime =
                    Time.unscaledTime;

                return;
            }

            if (Time.unscaledTime -
                _managerFirstSeenTime <
                1.5f)
            {
                return;
            }

            try
            {
                CacheNativeGroundCollision();

                int firstIndex =
                    infos.Length;

                object sourceInfo =
                    infos.GetValue(
                        Math.Min(
                            4,
                            infos.Length - 1
                        )
                    );

                if (sourceInfo == null)
                {
                    Logger.LogError(
                        "[REGISTER] No IslandInfo template available."
                    );
                    return;
                }

                Type infoType =
                    sourceInfo.GetType();

                FieldInfo spawnField =
                    FindField(
                        infoType,
                        "_spawnPosition"
                    );

                FieldInfo warningField =
                    FindField(
                        infoType,
                        "_warning"
                    );

                GameObject sourceSpawn =
                    spawnField?.GetValue(
                        sourceInfo
                    ) as GameObject;

                GameObject sourceWarning =
                    warningField?.GetValue(
                        sourceInfo
                    ) as GameObject;

                if (spawnField == null ||
                    warningField == null ||
                    sourceSpawn == null)
                {
                    Logger.LogError(
                        "[REGISTER] Native IslandInfo trigger template is incomplete."
                    );
                    return;
                }

                // Resolve final navigation positions only after all native/soft-dependency
                // islands (for example DevIslandUnlocked) have registered. This lets the
                // loader detect collisions with their real logical navigation points.
                ResolveNonConflictingMapPositions(
                    infos,
                    spawnField
                );

                int finalLength =
                    infos.Length +
                    _maps.Count;

                Array expanded =
                    Array.CreateInstance(
                        infoType,
                        finalLength
                    );

                Array.Copy(
                    infos,
                    expanded,
                    infos.Length
                );

                for (int i = 0; i < _maps.Count; i++)
                {
                    RegisteredMap map =
                        _maps[i];

                    int index =
                        firstIndex + i;

                    if (index > byte.MaxValue)
                    {
                        Logger.LogError(
                            "[REGISTER] Too many maps for byte island indexes."
                        );
                        break;
                    }

                    map.LogicalIndex =
                        (byte)index;

                    object newInfo =
                        CreateIslandInfoForMap(
                            map,
                            infoType,
                            sourceSpawn,
                            sourceWarning,
                            spawnField,
                            warningField
                        );

                    if (newInfo == null)
                        continue;

                    expanded.SetValue(
                        newInfo,
                        index
                    );

                    _mapsByIndex[
                        map.LogicalIndex
                    ] = map;

                    Logger.LogWarning(
                        $"[REGISTER] {map.Manifest.name} " +
                        $"index={map.LogicalIndex} " +
                        $"scene='{map.Manifest.scene}' " +
                        $"pos={map.Position} " +
                        $"bundle='{map.Manifest.bundle}' " +
                        $"radar={map.Manifest.radarColor}"
                    );
                }

                infosField.SetValue(
                    islandManager,
                    expanded
                );

                _registered = true;

                Logger.LogWarning(
                    $"[REGISTER] Added {_mapsByIndex.Count} custom map(s). " +
                    $"IslandInfo {infos.Length} -> {expanded.Length}."
                );

                ResetRadarReferences();
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[REGISTER] Failed: {ex}"
                );
            }
        }

        private void ResolveNonConflictingMapPositions(
            Array existingInfos,
            FieldInfo spawnField)
        {
            const float minimumSpacing = 550f;

            List<Vector3> occupied =
                new List<Vector3>();

            if (existingInfos != null)
            {
                for (int i = 0;
                     i < existingInfos.Length;
                     i++)
                {
                    object info =
                        existingInfos.GetValue(i);

                    if (info == null)
                        continue;

                    Vector3 position;

                    if (TryGetIslandInfoPosition(
                            info,
                            spawnField,
                            out position))
                    {
                        occupied.Add(
                            position
                        );

                        if (_verbose.Value)
                        {
                            Logger.LogInfo(
                                $"[PLACEMENT] Existing island index={i} pos={position}."
                            );
                        }
                    }
                }
            }

            for (int i = 0;
                 i < _maps.Count;
                 i++)
            {
                RegisteredMap map =
                    _maps[i];

                Vector3 requested =
                    map.Position;

                Vector3 conflictWith;

                if (!FindPositionConflict(
                        requested,
                        occupied,
                        minimumSpacing,
                        out conflictWith))
                {
                    occupied.Add(
                        requested
                    );

                    continue;
                }

                Vector3 replacement =
                    FindFreeNavigationPosition(
                        occupied,
                        i,
                        minimumSpacing
                    );

                Logger.LogWarning(
                    $"[PLACEMENT] '{map.Manifest.id}' requested {requested}, but that " +
                    $"position conflicts with an existing island at {conflictWith}. " +
                    $"Automatically moved navigation/radar position to {replacement}."
                );

                map.Position =
                    replacement;

                occupied.Add(
                    replacement
                );
            }
        }

        private bool TryGetIslandInfoPosition(
            object info,
            FieldInfo fallbackSpawnField,
            out Vector3 position)
        {
            position =
                Vector3.zero;

            if (info == null)
                return false;

            try
            {
                PropertyInfo property =
                    info.GetType().GetProperty(
                        "IslandPosition",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                if (property != null &&
                    property.PropertyType == typeof(Vector3) &&
                    property.CanRead)
                {
                    position =
                        (Vector3)property.GetValue(
                            info,
                            null
                        );

                    return true;
                }
            }
            catch
            {
            }

            try
            {
                FieldInfo spawnField =
                    fallbackSpawnField ??
                    FindField(
                        info.GetType(),
                        "_spawnPosition"
                    );

                GameObject spawn =
                    spawnField?.GetValue(
                        info
                    ) as GameObject;

                if (spawn != null)
                {
                    position =
                        spawn.transform.position;

                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool FindPositionConflict(
            Vector3 candidate,
            IEnumerable<Vector3> occupied,
            float minimumSpacing,
            out Vector3 conflictingPosition)
        {
            conflictingPosition =
                Vector3.zero;

            foreach (Vector3 other in occupied)
            {
                Vector2 a =
                    new Vector2(
                        candidate.x,
                        candidate.z
                    );

                Vector2 b =
                    new Vector2(
                        other.x,
                        other.z
                    );

                if (Vector2.Distance(
                        a,
                        b) <
                    minimumSpacing)
                {
                    conflictingPosition =
                        other;

                    return true;
                }
            }

            return false;
        }

        private Vector3 FindFreeNavigationPosition(
            IEnumerable<Vector3> occupied,
            int mapOrder,
            float minimumSpacing)
        {
            List<Vector3> occupiedList =
                occupied.ToList();

            // Search expanding rings well outside the native world. The phase offset
            // avoids placing every third-party map on the exact cardinal axes.
            const float firstRadius = 1250f;
            const float ringStep = 450f;
            const int slotsPerRing = 12;

            for (int ring = 0;
                 ring < 64;
                 ring++)
            {
                float radius =
                    firstRadius +
                    ring * ringStep;

                for (int slot = 0;
                     slot < slotsPerRing;
                     slot++)
                {
                    int shiftedSlot =
                        (slot + mapOrder * 3) %
                        slotsPerRing;

                    float angle =
                        (Mathf.PI * 2f /
                         slotsPerRing) *
                        shiftedSlot +
                        Mathf.PI / 12f;

                    Vector3 candidate =
                        new Vector3(
                            Mathf.Cos(angle) *
                            radius,
                            0f,
                            Mathf.Sin(angle) *
                            radius
                        );

                    Vector3 conflict;

                    if (!FindPositionConflict(
                            candidate,
                            occupiedList,
                            minimumSpacing,
                            out conflict))
                    {
                        return candidate;
                    }
                }
            }

            // Extremely unlikely fallback.
            return new Vector3(
                2500f + mapOrder * 650f,
                0f,
                2500f
            );
        }

        private object CreateIslandInfoForMap(
            RegisteredMap map,
            Type infoType,
            GameObject sourceSpawn,
            GameObject sourceWarning,
            FieldInfo spawnField,
            FieldInfo warningField)
        {
            Transform sourceHolder =
                sourceSpawn.transform.parent;

            GameObject clonedHolder =
                sourceHolder != null
                    ? Instantiate(
                        sourceHolder.gameObject
                    )
                    : new GameObject(
                        $"HTFML_Trigger_{map.Manifest.id}"
                    );

            clonedHolder.name =
                $"HTFML_IslandPositionHolder_{map.Manifest.id}";

            Transform clonedSpawnTransform =
                FindChildRecursive(
                    clonedHolder.transform,
                    sourceSpawn.name
                );

            if (clonedSpawnTransform == null)
            {
                Logger.LogError(
                    $"[REGISTER] '{map.Manifest.id}': cloned spawn not found."
                );
                Destroy(clonedHolder);
                return null;
            }

            Vector3 delta =
                map.Position -
                clonedSpawnTransform.position;

            clonedHolder.transform.position +=
                delta;

            GameObject clonedSpawn =
                clonedSpawnTransform.gameObject;

            clonedSpawn.name =
                $"HTFML_IslandPosition_{map.Manifest.id}";

            GameObject clonedWarning =
                null;

            if (sourceWarning != null)
            {
                Transform warningTransform =
                    FindChildRecursive(
                        clonedHolder.transform,
                        sourceWarning.name
                    );

                if (warningTransform != null)
                {
                    clonedWarning =
                        warningTransform.gameObject;

                    clonedWarning.name =
                        $"HTFML_IslandWarning_{map.Manifest.id}";
                }
            }

            Type spawnerType =
                AccessTools.TypeByName(
                    "IslandSpawner"
                );

            Component spawner =
                spawnerType != null
                    ? clonedSpawn.GetComponent(
                        spawnerType
                    )
                    : null;

            FieldInfo indexField =
                spawner != null
                    ? FindField(
                        spawner.GetType(),
                        "_islandIndex"
                    )
                    : null;

            if (spawner == null ||
                indexField == null)
            {
                Logger.LogError(
                    $"[REGISTER] '{map.Manifest.id}': IslandSpawner/index missing."
                );
                Destroy(clonedHolder);
                return null;
            }

            indexField.SetValue(
                spawner,
                Convert.ChangeType(
                    map.LogicalIndex,
                    indexField.FieldType
                )
            );

            object newInfo =
                Activator.CreateInstance(
                    infoType,
                    true
                );

            spawnField.SetValue(
                newInfo,
                clonedSpawn
            );

            warningField.SetValue(
                newInfo,
                clonedWarning
            );

            clonedHolder.SetActive(true);
            clonedSpawn.SetActive(true);

            RepairClonedTriggerGeometry(
                sourceHolder != null
                    ? sourceHolder.gameObject
                    : null,
                clonedHolder
            );

            DumpRegisteredNavigationTrigger(
                map,
                clonedHolder,
                clonedSpawn,
                spawner
            );

            return newInfo;
        }

        private void RepairClonedTriggerGeometry(
            GameObject sourceHolder,
            GameObject clonedHolder)
        {
            if (sourceHolder == null ||
                clonedHolder == null)
            {
                return;
            }

            Collider[] source =
                sourceHolder.GetComponentsInChildren<Collider>(
                    true
                );

            Collider[] clone =
                clonedHolder.GetComponentsInChildren<Collider>(
                    true
                );

            int count =
                Math.Min(
                    source.Length,
                    clone.Length
                );

            for (int i = 0; i < count; i++)
            {
                Collider src =
                    source[i];

                Collider dst =
                    clone[i];

                if (src == null ||
                    dst == null ||
                    src.GetType() != dst.GetType())
                {
                    continue;
                }

                bool enabled =
                    dst.enabled;

                dst.enabled =
                    false;

                if (src is SphereCollider srcSphere &&
                    dst is SphereCollider dstSphere)
                {
                    dstSphere.center =
                        srcSphere.center;

                    dstSphere.radius =
                        srcSphere.radius;
                }
                else if (src is BoxCollider srcBox &&
                         dst is BoxCollider dstBox)
                {
                    dstBox.center =
                        srcBox.center;

                    dstBox.size =
                        srcBox.size;
                }
                else if (src is CapsuleCollider srcCapsule &&
                         dst is CapsuleCollider dstCapsule)
                {
                    dstCapsule.center =
                        srcCapsule.center;

                    dstCapsule.radius =
                        srcCapsule.radius;

                    dstCapsule.height =
                        srcCapsule.height;

                    dstCapsule.direction =
                        srcCapsule.direction;
                }

                Physics.SyncTransforms();

                dst.enabled =
                    enabled;
            }

            Physics.SyncTransforms();

            Logger.LogWarning(
                $"[TRIGGER FIX] Refreshed {count} cloned collider(s) after moving " +
                $"'{clonedHolder.name}' to {clonedHolder.transform.position}."
            );
        }

        private void DumpRegisteredNavigationTrigger(
            RegisteredMap map,
            GameObject holder,
            GameObject spawn,
            Component spawner)
        {
            if (map == null ||
                holder == null ||
                spawn == null)
            {
                return;
            }

            try
            {
                Collider[] colliders =
                    holder.GetComponentsInChildren<Collider>(
                        true
                    );

                Logger.LogWarning(
                    $"[TRIGGER DUMP] '{map.Manifest.id}' holder='{holder.name}' " +
                    $"holderActive={holder.activeInHierarchy} " +
                    $"spawn='{spawn.name}' spawnActive={spawn.activeInHierarchy} " +
                    $"spawnPos={spawn.transform.position} " +
                    $"spawnerEnabled={(spawner is Behaviour b ? b.enabled.ToString() : "n/a")} " +
                    $"colliders={colliders.Length}."
                );

                foreach (Collider col in colliders)
                {
                    if (col == null)
                        continue;

                    Rigidbody rb =
                        col.GetComponent<Rigidbody>();

                    Logger.LogWarning(
                        $"[TRIGGER DUMP]   collider='{col.name}' " +
                        $"type={col.GetType().Name} enabled={col.enabled} " +
                        $"isTrigger={col.isTrigger} active={col.gameObject.activeInHierarchy} " +
                        $"layer={col.gameObject.layer} tag='{SafeTag(col.gameObject)}' " +
                        $"center={col.bounds.center} size={col.bounds.size} " +
                        $"rb={(rb != null ? $"kinematic={rb.isKinematic}" : "none")}."
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    $"[TRIGGER DUMP] Failed for '{map.Manifest.id}': {ex.Message}"
                );
            }
        }

        private static string SafeTag(
            GameObject go)
        {
            if (go == null)
                return "<null>";

            try
            {
                return go.tag;
            }
            catch
            {
                return "<unavailable>";
            }
        }

        // ============================================================
        // QUEUE / SCENE LOADING
        // ============================================================

        private void PatchNativeIslandSpawnerTrigger()
        {
            try
            {
                Type spawnerType =
                    AccessTools.TypeByName(
                        "IslandSpawner"
                    );

                if (spawnerType == null)
                {
                    Logger.LogWarning(
                        "[PATCH] IslandSpawner type not found."
                    );
                    return;
                }

                int patched = 0;

                foreach (string methodName in new[]
                {
                    "OnTriggerEnter",
                    "OnTriggerStay"
                })
                {
                    MethodInfo method =
                        spawnerType.GetMethod(
                            methodName,
                            BindingFlags.Instance |
                            BindingFlags.Public |
                            BindingFlags.NonPublic,
                            null,
                            new[] { typeof(Collider) },
                            null
                        );

                    if (method == null)
                        continue;

                    _harmony.Patch(
                        method,
                        postfix: new HarmonyMethod(
                            typeof(Plugin),
                            nameof(NativeIslandSpawnerTriggerPostfix)
                        )
                    );

                    patched++;

                    Logger.LogInfo(
                        $"[PATCH] IslandSpawner.{methodName}(Collider) patched."
                    );
                }

                if (patched == 0)
                {
                    Logger.LogWarning(
                        "[PATCH] No IslandSpawner trigger callbacks were found."
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[PATCH] IslandSpawner trigger patch failed: {ex}"
                );
            }
        }

        private static void NativeIslandSpawnerTriggerPostfix(
            object __instance,
            Collider other)
        {
            Plugin plugin =
                Instance;

            if (plugin == null ||
                !plugin._enabled.Value ||
                __instance == null ||
                other == null)
            {
                return;
            }

            FieldInfo indexField =
                FindField(
                    __instance.GetType(),
                    "_islandIndex"
                );

            if (indexField == null)
                return;

            byte islandIndex;

            try
            {
                islandIndex =
                    Convert.ToByte(
                        indexField.GetValue(
                            __instance
                        )
                    );
            }
            catch
            {
                return;
            }

            RegisteredMap target;

            if (!plugin._mapsByIndex.TryGetValue(
                    islandIndex,
                    out target))
            {
                return;
            }

            plugin.Logger.LogWarning(
                $"[ISLAND SPAWNER HIT] map='{target.Manifest.id}' index={islandIndex} " +
                $"spawner='{(__instance as Component)?.gameObject?.name}' " +
                $"other='{other.name}' otherLayer={other.gameObject.layer}."
            );

            // This is still the game's own IslandSpawner collider callback.
            // We only repair the final QueueRequest for custom indices, because
            // How to Fish 1.0.5 can run the cloned callback without successfully
            // routing an index that points to an external AssetBundle map.
            if (plugin.IsCustomMapAlreadyActive(
                    target) ||
                plugin._transitioning)
            {
                return;
            }

            if (!IsGameplayBoatOrPlayerCollider(
                    other))
            {
                if (plugin._verbose.Value)
                {
                    plugin.Logger.LogInfo(
                        $"[ISLAND SPAWNER] '{target.Manifest.id}' ignored collider " +
                        $"'{other.name}'."
                    );
                }

                return;
            }

            object islandManager =
                FindLiveObjectByTypeName(
                    "IslandManager"
                );

            if (islandManager == null)
                return;

            FieldInfo hasQueuedField =
                FindField(
                    islandManager.GetType(),
                    "_hasQueuedIsland"
                );

            try
            {
                if (hasQueuedField != null &&
                    hasQueuedField.FieldType == typeof(bool) &&
                    (bool)hasQueuedField.GetValue(islandManager))
                {
                    return;
                }
            }
            catch
            {
            }

            MethodInfo queue =
                AccessTools.Method(
                    islandManager.GetType(),
                    "QueueRequest",
                    new[] { typeof(byte) }
                );

            if (queue == null)
                return;

            plugin.Logger.LogWarning(
                $"[ISLAND SPAWNER] Native trigger reached custom map " +
                $"'{target.Manifest.id}' index={islandIndex} with collider " +
                $"'{other.name}'. Forwarding QueueRequest({islandIndex})."
            );

            try
            {
                // Do not bypass the QueueRequest Harmony prefix. The prefix converts
                // this custom logical index into the AssetBundle scene transition.
                queue.Invoke(
                    islandManager,
                    new object[]
                    {
                        islandIndex
                    }
                );
            }
            catch (Exception ex)
            {
                plugin.Logger.LogWarning(
                    $"[ISLAND SPAWNER] QueueRequest repair failed: {ex.Message}"
                );
            }
        }

        private static bool IsGameplayBoatOrPlayerCollider(
            Collider other)
        {
            Transform cursor =
                other != null
                    ? other.transform
                    : null;

            Type boatType =
                AccessTools.TypeByName(
                    "Boat"
                );

            Type playerType =
                AccessTools.TypeByName(
                    "Player"
                );

            while (cursor != null)
            {
                GameObject go =
                    cursor.gameObject;

                string name =
                    go.name ?? "";

                if (string.Equals(
                        name,
                        "MenuBoat",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (boatType != null &&
                    go.GetComponent(
                        boatType
                    ) != null)
                {
                    return true;
                }

                if (playerType != null &&
                    go.GetComponent(
                        playerType
                    ) != null)
                {
                    return true;
                }

                if (name.IndexOf(
                        "HostBoatCol",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf(
                        "LocalPlayer",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                cursor =
                    cursor.parent;
            }

            return false;
        }

        private void PatchQueueRequest()
        {
            try
            {
                Type islandManagerType =
                    AccessTools.TypeByName(
                        "IslandManager"
                    );

                MethodInfo queueRequest =
                    islandManagerType != null
                        ? AccessTools.Method(
                            islandManagerType,
                            "QueueRequest",
                            new Type[]
                            {
                                typeof(byte)
                            })
                        : null;

                if (queueRequest == null)
                {
                    Logger.LogError(
                        "[PATCH] IslandManager.QueueRequest(byte) not found."
                    );
                    return;
                }

                _harmony =
                    new Harmony(Guid);

                _harmony.Patch(
                    queueRequest,
                    prefix: new HarmonyMethod(
                        typeof(Plugin),
                        nameof(QueueRequestPrefix)
                    )
                );

                Logger.LogInfo(
                    "[PATCH] IslandManager.QueueRequest(byte) patched."
                );
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[PATCH] QueueRequest failed: {ex}"
                );
            }
        }

        private static bool QueueRequestPrefix(
            object __instance,
            byte islandId)
        {
            if (Instance == null ||
                !Instance._enabled.Value ||
                Instance._bypassQueuePatch)
            {
                return true;
            }

            RegisteredMap target;

            if (Instance._mapsByIndex.TryGetValue(
                    islandId,
                    out target))
            {
                // The navigation trigger lives permanently in the Game scene.
                // While the player/boat remains inside that trigger, Unity can fire
                // OnTriggerStay repeatedly. IslandSpawner may also repeat QueueRequest.
                //
                // If this custom map is ALREADY loaded, swallow the duplicate request.
                // Previously we started TransitionToCustomMap again, which unloaded and
                // reloaded the same AssetBundle scene every ~1 second, producing the
                // visible island flicker.
                if (Instance.IsCustomMapAlreadyActive(
                        target))
                {
                    if (Instance._verbose.Value)
                    {
                        Instance.Logger.LogInfo(
                            $"[QUEUE] Ignoring duplicate QueueRequest({islandId}) " +
                            $"for already active map '{target.Manifest.id}'."
                        );
                    }

                    return false;
                }

                if (!Instance._transitioning)
                {
                    Instance.StartCoroutine(
                        Instance.TransitionToCustomMap(
                            target
                        )
                    );
                }

                return false;
            }

            if (Instance._currentMap != null)
            {
                if (!Instance._transitioning)
                {
                    Instance.StartCoroutine(
                        Instance.LeaveCustomAndQueueNative(
                            __instance,
                            islandId
                        )
                    );
                }

                return false;
            }

            return true;
        }

        private bool IsCustomMapAlreadyActive(
            RegisteredMap target)
        {
            if (target == null)
                return false;

            if (_currentMap != null &&
                ReferenceEquals(
                    _currentMap,
                    target))
            {
                Scene currentScene =
                    SceneManager.GetSceneByName(
                        target.SceneName
                    );

                if (currentScene.IsValid() &&
                    currentScene.isLoaded)
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(
                    target.SceneName))
            {
                Scene scene =
                    SceneManager.GetSceneByName(
                        target.SceneName
                    );

                if (scene.IsValid() &&
                    scene.isLoaded)
                {
                    // Recover state if a scene-load callback happened before
                    // _currentMap was assigned or another mod caused a reference reset.
                    _currentMap =
                        target;

                    return true;
                }
            }

            return false;
        }

        private IEnumerator TransitionToCustomMap(
            RegisteredMap target)
        {
            if (IsCustomMapAlreadyActive(
                    target))
            {
                yield break;
            }

            _transitioning = true;

            try
            {
                if (_currentMap != null)
                {
                    yield return
                        UnloadMapScene(
                            _currentMap
                        );
                }

                if (!EnsureBundleLoaded(target))
                    yield break;

                // Cache native physics before unloading the current native island.
                CacheNativeGroundCollision();

                yield return
                    UnloadAllGameplayIslandsExceptGame();

                _loadingMap =
                    target;

                AsyncOperation load =
                    SceneManager.LoadSceneAsync(
                        target.SceneName,
                        LoadSceneMode.Additive
                    );

                if (load == null)
                {
                    Logger.LogError(
                        $"[LOAD] LoadSceneAsync('{target.SceneName}') returned null."
                    );
                    _loadingMap = null;
                    yield break;
                }

                while (!load.isDone)
                    yield return null;

                _currentMap =
                    target;

                _loadingMap =
                    null;

                Logger.LogWarning(
                    $"[LOAD] Custom map active: {target}"
                );
            }
            finally
            {
                _transitioning = false;
            }
        }

        private IEnumerator LeaveCustomAndQueueNative(
            object islandManager,
            byte islandId)
        {
            _transitioning = true;

            // C# iterators cannot yield from inside a try block that has a catch.
            // Do the asynchronous unload first, then protect only the synchronous
            // reflection/QueueRequest forwarding code.
            if (_currentMap != null)
            {
                yield return UnloadMapScene(
                    _currentMap
                );
            }

            _currentMap =
                null;

            try
            {
                MethodInfo queue =
                    AccessTools.Method(
                        islandManager.GetType(),
                        "QueueRequest",
                        new Type[]
                        {
                            typeof(byte)
                        });

                if (queue == null)
                {
                    Logger.LogError(
                        "[LOAD] Could not forward native QueueRequest."
                    );
                }
                else
                {
                    _bypassQueuePatch = true;

                    queue.Invoke(
                        islandManager,
                        new object[]
                        {
                            islandId
                        });

                    Logger.LogInfo(
                        $"[LOAD] Forwarded native QueueRequest({islandId})."
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[LOAD] Leaving custom map failed: {ex}"
                );
            }
            finally
            {
                _bypassQueuePatch = false;
                _transitioning = false;
            }
        }

        private IEnumerator UnloadAllGameplayIslandsExceptGame()
        {
            List<AsyncOperation> ops =
                new List<AsyncOperation>();

            for (int i =
                    SceneManager.sceneCount - 1;
                 i >= 0;
                 i--)
            {
                Scene scene =
                    SceneManager.GetSceneAt(i);

                if (!scene.IsValid() ||
                    !scene.isLoaded)
                {
                    continue;
                }

                if (string.Equals(
                        scene.name,
                        "Game",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Logger.LogInfo(
                    $"[LOAD] Unloading '{scene.name}'."
                );

                AsyncOperation op =
                    SceneManager.UnloadSceneAsync(
                        scene
                    );

                if (op != null)
                    ops.Add(op);
            }

            foreach (AsyncOperation op in ops)
            {
                while (!op.isDone)
                    yield return null;
            }
        }

        private IEnumerator UnloadMapScene(
            RegisteredMap map)
        {
            if (map == null ||
                string.IsNullOrWhiteSpace(
                    map.SceneName))
            {
                yield break;
            }

            Scene scene =
                SceneManager.GetSceneByName(
                    map.SceneName
                );

            if (scene.IsValid() &&
                scene.isLoaded)
            {
                AsyncOperation op =
                    SceneManager.UnloadSceneAsync(
                        scene
                    );

                if (op != null)
                {
                    while (!op.isDone)
                        yield return null;
                }
            }
        }

        private bool EnsureBundleLoaded(
            RegisteredMap map)
        {
            if (map.Bundle != null &&
                !string.IsNullOrWhiteSpace(
                    map.SceneName))
            {
                return true;
            }

            try
            {
                map.Bundle =
                    AssetBundle.LoadFromFile(
                        map.BundlePath
                    );

                if (map.Bundle == null)
                {
                    Logger.LogError(
                        $"[BUNDLE] LoadFromFile failed: {map.BundlePath}"
                    );
                    return false;
                }

                string[] scenes =
                    map.Bundle.GetAllScenePaths();

                if (scenes == null ||
                    scenes.Length == 0)
                {
                    Logger.LogError(
                        $"[BUNDLE] '{map.Manifest.id}' contains no scenes."
                    );
                    return false;
                }

                string requested =
                    map.Manifest.scene ?? "";

                string scenePath =
                    null;

                if (!string.IsNullOrWhiteSpace(
                        requested))
                {
                    scenePath =
                        scenes.FirstOrDefault(
                            p =>
                                string.Equals(
                                    Path.GetFileNameWithoutExtension(p),
                                    requested,
                                    StringComparison.OrdinalIgnoreCase
                                ) ||
                                string.Equals(
                                    p,
                                    requested,
                                    StringComparison.OrdinalIgnoreCase
                                )
                        );
                }

                if (scenePath == null)
                {
                    Logger.LogError(
                        $"[BUNDLE] '{map.Manifest.id}' requested scene " +
                        $"'{requested}', but that scene was not found in the bundle."
                    );

                    Logger.LogError(
                        $"[BUNDLE] Available scenes: {string.Join(", ", scenes)}"
                    );

                    return false;
                }

                map.ScenePath =
                    scenePath;

                map.SceneName =
                    Path.GetFileNameWithoutExtension(
                        scenePath
                    );

                Logger.LogWarning(
                    $"[BUNDLE] '{map.Manifest.id}' -> scene '{map.SceneName}'."
                );

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[BUNDLE] '{map.Manifest.id}' failed: {ex}"
                );
                return false;
            }
        }

        // ============================================================
        // LOADED SCENE PREPARATION
        // ============================================================

        private void OnSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            if (scene.IsValid() &&
                scene.isLoaded &&
                (
                    string.Equals(scene.name, "Game", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(scene.name, "DevIsland", StringComparison.OrdinalIgnoreCase) ||
                    scene.name.StartsWith("Island", StringComparison.OrdinalIgnoreCase)
                ))
            {
                ResetRadarReferences();
            }

            RegisteredMap map =
                _maps.FirstOrDefault(
                    m =>
                        !string.IsNullOrWhiteSpace(
                            m.SceneName) &&
                        string.Equals(
                            m.SceneName,
                            scene.name,
                            StringComparison.OrdinalIgnoreCase
                        )
                );

            if (map == null)
                return;

            Logger.LogWarning(
                $"[RUNTIME] Preparing custom scene '{scene.name}'."
            );

            PositionMapScene(
                map,
                scene
            );

            if (_repairRuntime.Value)
                PrepareMapRuntime(
                    map,
                    scene
                );

            if (_enableMapWildlife.Value)
                ApplyOptionalWildlife(
                    map,
                    scene
                );

            ResetRadarReferences();
        }

        private void OnSceneUnloaded(
            Scene scene)
        {
            if (_currentMap != null &&
                string.Equals(
                    _currentMap.SceneName,
                    scene.name,
                    StringComparison.OrdinalIgnoreCase))
            {
                _currentMap =
                    null;
            }
        }

        private void PositionMapScene(
            RegisteredMap map,
            Scene scene)
        {
            if (map == null ||
                !map.Manifest.moveScene)
            {
                Logger.LogInfo(
                    $"[RUNTIME] '{map?.Manifest?.id}' moveScene=false; scene coordinates preserved."
                );
                return;
            }

            GameObject[] roots =
                scene.GetRootGameObjects();

            GameObject holder =
                roots.FirstOrDefault(
                    r =>
                        r != null &&
                        string.Equals(
                            r.name,
                            "IslandHolder",
                            StringComparison.OrdinalIgnoreCase)
                );

            if (holder != null)
            {
                Vector3 before =
                    holder.transform.position;

                holder.transform.position =
                    map.Position;

                Logger.LogWarning(
                    $"[RUNTIME] '{map.Manifest.id}' IslandHolder moved {before} -> " +
                    $"{holder.transform.position} (requested {map.Position})."
                );
            }
            else
            {
                foreach (GameObject root in roots)
                {
                    if (root != null)
                        root.transform.position +=
                            map.Position;
                }

                Logger.LogWarning(
                    $"[RUNTIME] '{map.Manifest.id}' has no IslandHolder; moved " +
                    $"{roots.Length} root(s) by {map.Position}."
                );
            }
        }

        private void PrepareMapRuntime(
            RegisteredMap map,
            Scene scene)
        {
            GameObject holder =
                scene.GetRootGameObjects()
                    .FirstOrDefault(
                        r =>
                            r != null &&
                            string.Equals(
                                r.name,
                                "IslandHolder",
                                StringComparison.OrdinalIgnoreCase)
                    );

            if (holder == null)
            {
                holder =
                    new GameObject(
                        "IslandHolder"
                    );

                SceneManager.MoveGameObjectToScene(
                    holder,
                    scene
                );

                holder.transform.position =
                    map.Manifest.moveScene
                        ? map.Position
                        : Vector3.zero;
            }

            AdaptLevelPhysics(
                holder.transform
            );

            EnsureIslandComponent(
                map,
                holder.transform
            );

            EnsureSpawnManager(
                map,
                holder.transform
            );
        }

        private void EnsureIslandComponent(
            RegisteredMap map,
            Transform holder)
        {
            Type islandType =
                AccessTools.TypeByName(
                    "Island"
                );

            if (islandType == null)
                return;

            Transform managerTransform =
                FindChildRecursive(
                    holder,
                    "IslandManager"
                );

            GameObject managerGo;

            if (managerTransform != null)
            {
                managerGo =
                    managerTransform.gameObject;
            }
            else
            {
                managerGo =
                    new GameObject(
                        "IslandManager"
                    );

                managerGo.transform.SetParent(
                    holder,
                    false
                );
            }

            Component island =
                managerGo.GetComponent(
                    islandType
                );

            if (island == null)
                island =
                    managerGo.AddComponent(
                        islandType
                    );

            SetFieldIfExists(
                island,
                "_islandSize",
                map.Manifest.islandSize
            );
        }

        private void EnsureSpawnManager(
            RegisteredMap map,
            Transform holder)
        {
            Type spawnManagerType =
                AccessTools.TypeByName(
                    "SpawnManager"
                );

            if (spawnManagerType == null)
                return;

            Transform existing =
                FindChildRecursive(
                    holder,
                    "SpawnManager"
                );

            GameObject go;

            if (existing != null)
            {
                go =
                    existing.gameObject;
            }
            else
            {
                go =
                    new GameObject(
                        "SpawnManager"
                    );

                go.transform.SetParent(
                    holder,
                    false
                );
            }

            go.SetActive(false);

            string playerName =
                map.Manifest.runtime?.playerSpawn ??
                "PlayerSpawnPoint";

            string boatName =
                map.Manifest.runtime?.boatSpawn ??
                "BoatSpawnPoint";

            Transform playerSpawn =
                FindChildRecursive(
                    holder,
                    playerName
                );

            if (playerSpawn == null)
            {
                GameObject p =
                    new GameObject(
                        playerName
                    );

                p.transform.SetParent(
                    go.transform,
                    false
                );

                p.transform.localPosition =
                    map.Manifest.runtime?.defaultPlayerSpawn?.ToVector3()
                    ?? new Vector3(0f, 6f, 0f);

                playerSpawn =
                    p.transform;
            }

            Transform boatSpawn =
                FindChildRecursive(
                    holder,
                    boatName
                );

            if (boatSpawn == null)
            {
                GameObject b =
                    new GameObject(
                        boatName
                    );

                b.transform.SetParent(
                    go.transform,
                    false
                );

                b.transform.localPosition =
                    map.Manifest.runtime?.defaultBoatSpawn?.ToVector3()
                    ?? new Vector3(0f, 0.5f, -25f);

                boatSpawn =
                    b.transform;
            }

            Component spawnManager =
                go.GetComponent(
                    spawnManagerType
                );

            if (spawnManager == null)
                spawnManager =
                    go.AddComponent(
                        spawnManagerType
                    );

            SetFieldIfExists(
                spawnManager,
                "_testPrint",
                $"HTF Map Loader: {map.Manifest.id}"
            );

            SetFieldIfExists(
                spawnManager,
                "_playerSpawnPoint",
                playerSpawn
            );

            SetFieldIfExists(
                spawnManager,
                "_boatSpawnPoint",
                boatSpawn
            );

            object boatPrefab =
                FindBoatPrefabInMemory();

            if (boatPrefab != null)
            {
                SetFieldIfExists(
                    spawnManager,
                    "_boatPrefab",
                    boatPrefab
                );
            }

            go.SetActive(true);

            Logger.LogInfo(
                $"[RUNTIME] '{map.Manifest.id}' PlayerSpawn={playerSpawn.position}, BoatSpawn={boatSpawn.position}."
            );
        }

        // ============================================================
        // OPTIONAL BUILT-IN WILDLIFE
        // ============================================================

        private void ApplyOptionalWildlife(
            RegisteredMap map,
            Scene scene)
        {
            WildlifeManifest wildlife =
                map.Manifest.wildlife;

            if (wildlife == null)
                return;

            if (wildlife.seagulls)
            {
                EnsureSeagullSpawner(
                    scene,
                    Mathf.Clamp(
                        wildlife.seagullMax,
                        1,
                        20
                    ),
                    Mathf.Max(
                        1f,
                        wildlife.seagullDelay
                    )
                );
            }

            if (wildlife.clams)
            {
                EnsureClamSpawner(
                    scene,
                    Mathf.Clamp(
                        wildlife.clamMax,
                        1,
                        20
                    ),
                    Mathf.Max(
                        1f,
                        wildlife.clamDelay
                    )
                );
            }
        }

        private void EnsureSeagullSpawner(
            Scene scene,
            int maxCount,
            float delay)
        {
            GameObject holder =
                FindIslandHolder(
                    scene
                );

            if (holder == null ||
                FindChildRecursive(
                    holder.transform,
                    "SeagullSpawner") != null)
            {
                return;
            }

            Type spawnerType =
                AccessTools.TypeByName(
                    "ItemSpawner"
                );

            Type birdType =
                AccessTools.TypeByName(
                    "Bird"
                );

            if (spawnerType == null ||
                birdType == null)
            {
                return;
            }

            UnityEngine.Object prefab =
                FindNamedComponentPrefab(
                    birdType,
                    "Seagull"
                );

            if (prefab == null)
            {
                Logger.LogWarning(
                    "[WILDLIFE] Seagull prefab not available in memory."
                );
                return;
            }

            Transform root =
                GetOrCreateChild(
                    holder.transform,
                    "CreatureSpawners"
                );

            GameObject go =
                new GameObject(
                    "SeagullSpawner"
                );

            go.transform.SetParent(
                root,
                false
            );

            go.transform.localPosition =
                new Vector3(
                    0f,
                    20f,
                    0f
                );

            go.SetActive(false);

            BoxCollider box =
                go.AddComponent<BoxCollider>();

            box.isTrigger = true;
            box.size =
                new Vector3(
                    70f,
                    12f,
                    70f
                );

            Component spawner =
                go.AddComponent(
                    spawnerType
                );

            ConfigureItemSpawner(
                spawner,
                box,
                prefab,
                false,
                true,
                maxCount,
                delay,
                1
            );

            go.SetActive(true);

            Logger.LogInfo(
                $"[WILDLIFE] SeagullSpawner added max={maxCount}, delay={delay:0.#}."
            );
        }

        private void EnsureClamSpawner(
            Scene scene,
            int maxCount,
            float delay)
        {
            GameObject holder =
                FindIslandHolder(
                    scene
                );

            if (holder == null ||
                FindChildRecursive(
                    holder.transform,
                    "ClamSpawner") != null)
            {
                return;
            }

            Type spawnerType =
                AccessTools.TypeByName(
                    "ItemSpawner"
                );

            Type creatureType =
                AccessTools.TypeByName(
                    "Creature"
                );

            if (spawnerType == null ||
                creatureType == null)
            {
                return;
            }

            UnityEngine.Object prefab =
                FindNamedComponentPrefab(
                    creatureType,
                    "Clam"
                );

            if (prefab == null)
            {
                Logger.LogWarning(
                    "[WILDLIFE] Clam prefab not available in memory."
                );
                return;
            }

            Transform root =
                GetOrCreateChild(
                    holder.transform,
                    "CreatureSpawners"
                );

            GameObject go =
                new GameObject(
                    "ClamSpawner"
                );

            go.transform.SetParent(
                root,
                false
            );

            go.transform.localPosition =
                new Vector3(
                    0f,
                    1.5f,
                    0f
                );

            go.SetActive(false);

            BoxCollider box =
                go.AddComponent<BoxCollider>();

            box.isTrigger = true;
            box.center =
                new Vector3(
                    0f,
                    -1.25f,
                    0f
                );

            box.size =
                new Vector3(
                    28f,
                    3f,
                    28f
                );

            Component spawner =
                go.AddComponent(
                    spawnerType
                );

            ConfigureItemSpawner(
                spawner,
                box,
                prefab,
                true,
                false,
                maxCount,
                delay,
                2
            );

            go.SetActive(true);

            Logger.LogInfo(
                $"[WILDLIFE] ClamSpawner added max={maxCount}, delay={delay:0.#}."
            );
        }

        private void ConfigureItemSpawner(
            Component spawner,
            BoxCollider box,
            UnityEngine.Object prefab,
            bool deadCreature,
            bool spawnInAir,
            int max,
            float delay,
            int count)
        {
            SetFieldIfExists(spawner, "_isActive", true);
            SetFieldIfExists(spawner, "_itemToSpawn", prefab);
            SetFieldIfExists(spawner, "_spawnsDeadCreature", deadCreature);
            SetFieldIfExists(spawner, "_startFrequencyForRadio", 98);
            SetFieldIfExists(spawner, "_useRandomRotation", true);
            SetFieldIfExists(spawner, "_maxItemsSpawned", max);
            SetFieldIfExists(spawner, "_spawnInstant", true);
            SetFieldIfExists(spawner, "_onlySpawnOnce", false);
            SetFieldIfExists(spawner, "_spawnInAir", spawnInAir);
            SetFieldIfExists(spawner, "_spawnDelay", delay);
            SetFieldIfExists(spawner, "_spawnCount", count);
            SetFieldIfExists(spawner, "_spawnBox", box);
            SetFieldIfExists(spawner, "_hasSpawned", false);

            FieldInfo listField =
                FindField(
                    spawner.GetType(),
                    "_itemsSpawned"
                );

            if (listField != null)
            {
                try
                {
                    listField.SetValue(
                        spawner,
                        Activator.CreateInstance(
                            listField.FieldType
                        )
                    );
                }
                catch
                {
                }
            }
        }

        private void VerifyRegisteredNavigationTriggers()
        {
            Type spawnerType =
                AccessTools.TypeByName(
                    "IslandSpawner"
                );

            if (spawnerType == null)
                return;

            UnityEngine.Object[] all =
                Resources.FindObjectsOfTypeAll(
                    spawnerType
                );

            foreach (RegisteredMap map in _mapsByIndex.Values)
            {
                Component match =
                    all
                        .OfType<Component>()
                        .FirstOrDefault(c =>
                        {
                            if (c == null ||
                                c.gameObject == null)
                                return false;

                            FieldInfo f =
                                FindField(
                                    c.GetType(),
                                    "_islandIndex"
                                );

                            if (f == null)
                                return false;

                            try
                            {
                                return Convert.ToByte(
                                    f.GetValue(c)
                                ) == map.LogicalIndex;
                            }
                            catch
                            {
                                return false;
                            }
                        });

                if (match == null)
                {
                    Logger.LogError(
                        $"[TRIGGER VERIFY] '{map.Manifest.id}' index={map.LogicalIndex}: " +
                        "NO live IslandSpawner found."
                    );
                    continue;
                }

                Collider[] cols =
                    match.gameObject
                        .transform
                        .parent
                        ?.GetComponentsInChildren<Collider>(true)
                    ?? Array.Empty<Collider>();

                Logger.LogWarning(
                    $"[TRIGGER VERIFY] '{map.Manifest.id}' index={map.LogicalIndex}: " +
                    $"spawner='{match.gameObject.name}' active={match.gameObject.activeInHierarchy} " +
                    $"pos={match.transform.position} colliders={cols.Length}."
                );
            }
        }

        private bool IsGameplayWorldReady()
        {
            bool hasGame = false;
            bool hasLoadedIsland = false;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.IsValid() || !scene.isLoaded)
                    continue;

                if (string.Equals(
                        scene.name,
                        "Game",
                        StringComparison.OrdinalIgnoreCase))
                {
                    hasGame = true;
                    continue;
                }

                if (scene.name.StartsWith(
                        "Island",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        scene.name,
                        "DevIsland",
                        StringComparison.OrdinalIgnoreCase) ||
                    _maps.Any(m =>
                        !string.IsNullOrWhiteSpace(m.SceneName) &&
                        string.Equals(
                            m.SceneName,
                            scene.name,
                            StringComparison.OrdinalIgnoreCase)))
                {
                    hasLoadedIsland = true;
                }
            }

            if (!hasGame || !hasLoadedIsland)
                return false;

            object islandManager =
                FindLiveObjectByTypeName("IslandManager");

            if (islandManager == null)
                return false;

            FieldInfo infosField =
                FindField(
                    islandManager.GetType(),
                    "_islandInfos"
                );

            Array infos =
                infosField?.GetValue(islandManager) as Array;

            return infos != null &&
                   _mapsByIndex.Keys.All(index => index < infos.Length);
        }

        // ============================================================
        // RADAR
        // ============================================================

        private void UpdateRadar()
        {
            if (_mapsByIndex.Count == 0)
                return;

            if (_radarUi == null ||
                _maps.Any(
                    m =>
                        _mapsByIndex.ContainsKey(
                            m.LogicalIndex
                        ) &&
                        m.MapDot == null))
            {
                EnsureRadarDots();
            }

            if (_radarUi == null)
                return;

            if (!IsNativeRadarCurrentlyVisible())
            {
                foreach (RegisteredMap map in _maps)
                    SetMapDotVisualActive(
                        map.MapDot,
                        false
                    );

                return;
            }

            foreach (RegisteredMap map in _maps)
            {
                if (map.MapDot == null)
                    continue;

                try
                {
                    if (_radarUpdateDotMethod == null)
                    {
                        Type mapDotType =
                            map.MapDot.GetType();

                        _radarUpdateDotMethod =
                            _radarUi.GetType().GetMethod(
                                "UpdateDot",
                                BindingFlags.Instance |
                                BindingFlags.Public |
                                BindingFlags.NonPublic,
                                null,
                                new Type[]
                                {
                                    mapDotType,
                                    typeof(Vector3)
                                },
                                null
                            );
                    }

                    _radarUpdateDotMethod?.Invoke(
                        _radarUi,
                        new object[]
                        {
                            map.MapDot,
                            map.Position
                        });

                    SetMapDotVisualActive(
                        map.MapDot,
                        true
                    );

                    ApplyMapDotColor(
                        map
                    );
                }
                catch
                {
                    ResetRadarReferences();
                    return;
                }
            }
        }

        private void EnsureRadarDots()
        {
            if (Time.unscaledTime <
                _nextRadarAcquire)
            {
                return;
            }

            _nextRadarAcquire =
                Time.unscaledTime + 1f;

            object radar =
                FindLiveObjectByTypeName(
                    "RadarUI"
                );

            if (radar == null)
                return;

            FieldInfo dotsField =
                FindField(
                    radar.GetType(),
                    "_islandDots"
                );

            Array dots =
                dotsField?.GetValue(
                    radar
                ) as Array;

            if (dots == null)
                return;

            Type mapDotType =
                dots.GetType().GetElementType();

            if (mapDotType == null)
                return;

            int required =
                _maps
                    .Where(
                        m =>
                            _mapsByIndex.ContainsKey(
                                m.LogicalIndex))
                    .Select(
                        m =>
                            (int)m.LogicalIndex + 1
                    )
                    .DefaultIfEmpty(
                        dots.Length
                    )
                    .Max();

            Array expanded =
                Array.CreateInstance(
                    mapDotType,
                    Math.Max(
                        dots.Length,
                        required
                    )
                );

            Array.Copy(
                dots,
                expanded,
                dots.Length
            );

            object template =
                null;

            for (int i = 0; i < dots.Length; i++)
            {
                object candidate =
                    dots.GetValue(i);

                if (candidate != null)
                {
                    template =
                        candidate;
                    break;
                }
            }

            if (template == null)
                return;

            foreach (RegisteredMap map in _maps)
            {
                int index =
                    map.LogicalIndex;

                object existing =
                    index < dots.Length
                        ? dots.GetValue(index)
                        : null;

                if (existing != null &&
                    map.MapDot == null &&
                    IsOurMapDot(existing, map.Manifest.id))
                {
                    map.MapDot =
                        existing;
                }

                if (map.MapDot == null)
                {
                    map.MapDot =
                        CloneMapDot(
                            template,
                            $"HTFML_Radar_{map.Manifest.id}"
                        );
                }

                if (map.MapDot != null)
                {
                    expanded.SetValue(
                        map.MapDot,
                        index
                    );

                    ApplyMapDotColor(
                        map
                    );
                }
            }

            dotsField.SetValue(
                radar,
                expanded
            );

            _radarUi =
                radar;

            _radarUpdateDotMethod =
                null;

            Logger.LogWarning(
                $"[RADAR] MapDot array {dots.Length} -> {expanded.Length}. " +
                $"Custom={string.Join(", ", _mapsByIndex.Values.Select(m => $"{m.Manifest.id}@{m.LogicalIndex}:{m.Position}"))}"
            );
        }

        private bool IsOurMapDot(
            object mapDot,
            string mapId)
        {
            if (mapDot == null)
                return false;

            try
            {
                FieldInfo dotField =
                    FindField(mapDot.GetType(), "_dot");

                Component visual =
                    dotField?.GetValue(mapDot) as Component;

                return visual != null &&
                       visual.gameObject != null &&
                       string.Equals(
                           visual.gameObject.name,
                           $"HTFML_Radar_{mapId}",
                           StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private object CloneMapDot(
            object template,
            string name)
        {
            MethodInfo cloneMethod =
                typeof(object).GetMethod(
                    "MemberwiseClone",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic
                );

            object clone =
                cloneMethod?.Invoke(
                    template,
                    null
                );

            if (clone == null)
                return null;

            FieldInfo dotField =
                FindField(
                    clone.GetType(),
                    "_dot"
                );

            Component source =
                dotField?.GetValue(
                    template
                ) as Component;

            if (source == null ||
                source.transform.parent == null)
            {
                return null;
            }

            GameObject visual =
                Instantiate(
                    source.gameObject
                );

            visual.name =
                name;

            visual.transform.SetParent(
                source.transform.parent,
                false
            );

            visual.transform.localPosition =
                source.transform.localPosition;

            visual.transform.localRotation =
                source.transform.localRotation;

            visual.transform.localScale =
                source.transform.localScale;

            Component image =
                visual.GetComponent(
                    dotField.FieldType
                );

            if (image == null)
            {
                Destroy(visual);
                return null;
            }

            dotField.SetValue(
                clone,
                image
            );

            visual.SetActive(
                source.gameObject.activeInHierarchy
            );

            return clone;
        }

        private bool IsNativeRadarCurrentlyVisible()
        {
            if (_radarUi == null)
                return false;

            FieldInfo dotsField =
                FindField(
                    _radarUi.GetType(),
                    "_islandDots"
                );

            Array dots =
                dotsField?.GetValue(
                    _radarUi
                ) as Array;

            if (dots == null)
                return false;

            int nativeCount =
                Math.Min(
                    5,
                    dots.Length
                );

            for (int i = 0; i < nativeCount; i++)
            {
                object dot =
                    dots.GetValue(i);

                FieldInfo visualField =
                    dot != null
                        ? FindField(
                            dot.GetType(),
                            "_dot"
                        )
                        : null;

                Component visual =
                    visualField?.GetValue(
                        dot
                    ) as Component;

                if (visual != null &&
                    visual.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyMapDotColor(
            RegisteredMap map)
        {
            if (map?.MapDot == null)
                return;

            FieldInfo dotField =
                FindField(
                    map.MapDot.GetType(),
                    "_dot"
                );

            Component visual =
                dotField?.GetValue(
                    map.MapDot
                ) as Component;

            PropertyInfo color =
                visual?.GetType().GetProperty(
                    "color",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            if (color != null &&
                color.CanWrite &&
                color.PropertyType ==
                    typeof(Color))
            {
                try
                {
                    color.SetValue(
                        visual,
                        map.RadarColor,
                        null
                    );
                }
                catch
                {
                }
            }
        }

        private void SetMapDotVisualActive(
            object mapDot,
            bool active)
        {
            if (mapDot == null)
                return;

            FieldInfo field =
                FindField(
                    mapDot.GetType(),
                    "_dot"
                );

            Component visual =
                field?.GetValue(
                    mapDot
                ) as Component;

            if (visual != null &&
                visual.gameObject.activeSelf !=
                    active)
            {
                visual.gameObject.SetActive(
                    active
                );
            }
        }

        private void ResetRadarReferences()
        {
            _radarUi = null;
            _radarUpdateDotMethod = null;
            _nextRadarAcquire =
                Time.unscaledTime + 0.75f;

            foreach (RegisteredMap map in _maps)
                map.MapDot = null;
        }

        // ============================================================
        // PHYSICS / HELPERS
        // ============================================================

        private void CacheNativeGroundCollision()
        {
            Collider best =
                null;

            float bestScore =
                -1f;

            for (int i = 0;
                 i < SceneManager.sceneCount;
                 i++)
            {
                Scene scene =
                    SceneManager.GetSceneAt(i);

                if (!scene.IsValid() ||
                    !scene.isLoaded)
                {
                    continue;
                }

                bool native =
                    scene.name.StartsWith(
                        "Island",
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    string.Equals(
                        scene.name,
                        "DevIsland",
                        StringComparison.OrdinalIgnoreCase
                    );

                if (!native)
                    continue;

                foreach (GameObject root in
                    scene.GetRootGameObjects())
                {
                    foreach (Collider col in
                        root.GetComponentsInChildren<Collider>(
                            true))
                    {
                        if (col == null ||
                            !col.enabled ||
                            col.isTrigger)
                        {
                            continue;
                        }

                        Vector3 s =
                            col.bounds.size;

                        float score =
                            s.x * s.z;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = col;
                        }
                    }
                }
            }

            if (best == null)
                return;

            _nativeGroundLayer =
                best.gameObject.layer;

            _nativeGroundMaterial =
                best.sharedMaterial;
        }

        private void AdaptLevelPhysics(
            Transform holder)
        {
            if (holder == null)
                return;

            int level =
                LayerMask.NameToLayer(
                    "Level"
                );

            if (level < 0)
                level =
                    _nativeGroundLayer >= 0
                        ? _nativeGroundLayer
                        : 8;

            Collider[] colliders =
                holder.GetComponentsInChildren<Collider>(
                    true
                );

            int changed = 0;

            foreach (Collider col in colliders)
            {
                if (col == null ||
                    col.isTrigger)
                {
                    continue;
                }

                Rigidbody rb =
                    col.gameObject.GetComponent<Rigidbody>();

                if (rb != null &&
                    !rb.isKinematic)
                {
                    continue;
                }

                col.gameObject.layer =
                    level;

                TrySetLevelTag(
                    col.gameObject
                );

                col.enabled =
                    true;

                if (_nativeGroundMaterial != null &&
                    col.sharedMaterial == null)
                {
                    col.sharedMaterial =
                        _nativeGroundMaterial;
                }

                changed++;
            }

            if (_verbose.Value)
            {
                Logger.LogInfo(
                    $"[PHYSICS] Adapted {changed} static collider(s) to Level."
                );
            }
        }

        private bool TrySetLevelTag(
            GameObject go)
        {
            try
            {
                if (!go.CompareTag("Level"))
                    go.tag = "Level";

                return true;
            }
            catch
            {
                return false;
            }
        }

        private object FindBoatPrefabInMemory()
        {
            Type boatType =
                AccessTools.TypeByName(
                    "Boat"
                );

            if (boatType == null)
                return null;

            UnityEngine.Object[] objects =
                Resources.FindObjectsOfTypeAll(
                    boatType
                );

            foreach (UnityEngine.Object obj in objects)
            {
                Component component =
                    obj as Component;

                if (component == null)
                    continue;

                Scene scene =
                    component.gameObject.scene;

                if (!scene.IsValid() ||
                    scene.buildIndex < 0)
                {
                    return obj;
                }
            }

            return objects.FirstOrDefault(
                o => o != null
            );
        }

        private UnityEngine.Object FindNamedComponentPrefab(
            Type componentType,
            string wantedName)
        {
            UnityEngine.Object[] objects =
                Resources.FindObjectsOfTypeAll(
                    componentType
                );

            UnityEngine.Object fallback =
                null;

            foreach (UnityEngine.Object obj in objects)
            {
                Component component =
                    obj as Component;

                if (component == null)
                    continue;

                string name =
                    component.gameObject.name ?? "";

                if (name.IndexOf(
                        wantedName,
                        StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Scene scene =
                    component.gameObject.scene;

                if (!scene.IsValid() ||
                    scene.buildIndex < 0)
                {
                    return component;
                }

                if (fallback == null)
                    fallback = component;
            }

            return fallback;
        }

        private GameObject FindIslandHolder(
            Scene scene)
        {
            if (!scene.IsValid() ||
                !scene.isLoaded)
            {
                return null;
            }

            return scene.GetRootGameObjects()
                .FirstOrDefault(
                    r =>
                        r != null &&
                        string.Equals(
                            r.name,
                            "IslandHolder",
                            StringComparison.OrdinalIgnoreCase)
                );
        }

        private Transform GetOrCreateChild(
            Transform parent,
            string name)
        {
            Transform existing =
                FindChildRecursive(
                    parent,
                    name
                );

            if (existing != null)
                return existing;

            GameObject go =
                new GameObject(
                    name
                );

            go.transform.SetParent(
                parent,
                false
            );

            return go.transform;
        }

        private static object FindLiveObjectByTypeName(
            string typeName)
        {
            Type type =
                AccessTools.TypeByName(
                    typeName
                );

            if (type == null)
                return null;

            UnityEngine.Object[] objects =
                Resources.FindObjectsOfTypeAll(
                    type
                );

            foreach (UnityEngine.Object obj in objects)
            {
                Component component =
                    obj as Component;

                if (component == null)
                    continue;

                Scene scene =
                    component.gameObject.scene;

                if (scene.IsValid() &&
                    scene.isLoaded)
                {
                    return component;
                }
            }

            return objects.FirstOrDefault(
                o => o != null
            );
        }

        private static FieldInfo FindField(
            Type type,
            string name)
        {
            for (Type cursor = type;
                 cursor != null;
                 cursor = cursor.BaseType)
            {
                FieldInfo field =
                    cursor.GetField(
                        name,
                        BindingFlags.Instance |
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly
                    );

                if (field != null)
                    return field;
            }

            return null;
        }

        private static Transform FindChildRecursive(
            Transform root,
            string name)
        {
            if (root == null)
                return null;

            if (string.Equals(
                    root.name,
                    name,
                    StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0;
                 i < root.childCount;
                 i++)
            {
                Transform found =
                    FindChildRecursive(
                        root.GetChild(i),
                        name
                    );

                if (found != null)
                    return found;
            }

            return null;
        }

        private static bool SetFieldIfExists(
            object target,
            string fieldName,
            object value)
        {
            if (target == null)
                return false;

            FieldInfo field =
                FindField(
                    target.GetType(),
                    fieldName
                );

            if (field == null)
                return false;

            try
            {
                object converted =
                    value;

                if (value != null &&
                    !field.FieldType.IsInstanceOfType(
                        value))
                {
                    if (field.FieldType.IsEnum)
                    {
                        converted =
                            Enum.ToObject(
                                field.FieldType,
                                value
                            );
                    }
                    else
                    {
                        converted =
                            Convert.ChangeType(
                                value,
                                field.FieldType
                            );
                    }
                }

                field.SetValue(
                    target,
                    converted
                );

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
