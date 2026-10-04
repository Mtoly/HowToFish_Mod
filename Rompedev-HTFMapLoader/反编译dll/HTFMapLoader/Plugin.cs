using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HTFMapLoader
{
	// Token: 0x02000004 RID: 4
	[BepInPlugin("com.howtofish.maploader", "HTF Map Loader", "0.2.8")]
	[BepInDependency("com.howtofish.devislandunlocked", 2)]
	public sealed class Plugin : BaseUnityPlugin
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3 RVA: 0x00002067 File Offset: 0x00000267
		private string LegacyMapsRoot
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Maps");
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000004 RID: 4 RVA: 0x00002082 File Offset: 0x00000282
		private string PluginsRoot
		{
			get
			{
				return Paths.PluginPath;
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000208C File Offset: 0x0000028C
		private void Awake()
		{
			Plugin.Instance = this;
			Plugin.Log = base.Logger;
			this._enabled = base.Config.Bind<bool>("General", "Enabled", true, "Enable HTF Map Loader.");
			this._repairRuntime = base.Config.Bind<bool>("General", "RepairMissingRuntimeObjects", true, "Automatically repair common Island/SpawnManager/Level setup when a map is missing it.");
			this._enableMapWildlife = base.Config.Bind<bool>("General", "AllowMapWildlifeOptions", true, "Allow map.json to request optional built-in Seagull and Clam spawners.");
			this._verbose = base.Config.Bind<bool>("Debug", "VerboseLogging", false, "Enable verbose Map Loader logging.");
			Directory.CreateDirectory(this.LegacyMapsRoot);
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
			SceneManager.sceneUnloaded += new UnityAction<Scene>(this.OnSceneUnloaded);
			this.PatchQueueRequest();
			this.PatchNativeIslandSpawnerTrigger();
			base.Logger.LogInfo("HTF Map Loader v0.2.8 loaded.");
			base.Logger.LogInfo(string.Format("Enabled={0}, RepairRuntime={1}, WildlifeOptions={2}", this._enabled.Value, this._repairRuntime.Value, this._enableMapWildlife.Value));
			base.Logger.LogInfo("Legacy maps directory: " + this.LegacyMapsRoot);
			base.Logger.LogInfo("Thunderstore plugin root: " + this.PluginsRoot);
			base.Logger.LogInfo("Map packs are discovered recursively from BepInEx/plugins/**/Maps/**/map.json.");
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002204 File Offset: 0x00000404
		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
			SceneManager.sceneUnloaded -= new UnityAction<Scene>(this.OnSceneUnloaded);
			Harmony harmony = this._harmony;
			if (harmony != null)
			{
				harmony.UnpatchSelf();
			}
			foreach (Plugin.RegisteredMap registeredMap in this._maps)
			{
				try
				{
					if (registeredMap.Bundle != null)
					{
						registeredMap.Bundle.Unload(false);
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000022B0 File Offset: 0x000004B0
		private void Update()
		{
			if (!this._enabled.Value)
			{
				if (!this._loggedDisabled)
				{
					this._loggedDisabled = true;
					base.Logger.LogWarning("[GENERAL] HTF Map Loader is disabled by config: [General] Enabled=false.");
				}
				return;
			}
			this._loggedDisabled = false;
			if (!this._scanned)
			{
				this.ScanMapFolders();
				this._scanned = true;
			}
			if (!this._registered && this._maps.Count > 0)
			{
				this.TryRegisterMaps();
			}
			if (this._registered && !this._verifiedRegisteredTriggers && this.IsGameplayWorldReady())
			{
				this.VerifyRegisteredNavigationTriggers();
				this._verifiedRegisteredTriggers = true;
			}
			if (this._registered && this.IsGameplayWorldReady())
			{
				this.UpdateRadar();
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002360 File Offset: 0x00000560
		private void ScanMapFolders()
		{
			this._maps.Clear();
			string[] array = this.DiscoverMapManifests();
			base.Logger.LogInfo(string.Format("[MAPS] Found {0} map.json file(s) across installed plugin packages.", array.Length));
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int num = 0;
			foreach (string text in array)
			{
				try
				{
					string text2 = File.ReadAllText(text);
					Plugin.MapManifest mapManifest = JsonUtility.FromJson<Plugin.MapManifest>(text2);
					this.ApplyExplicitVectorOverrides(mapManifest, text2, text);
					if (this.ValidateManifest(mapManifest, text))
					{
						if (!hashSet.Add(mapManifest.id.Trim()))
						{
							base.Logger.LogWarning(string.Concat(new string[] { "[MAPS] Duplicate map id '", mapManifest.id, "'. Skipping ", text, "." }));
						}
						else
						{
							string directoryName = Path.GetDirectoryName(text);
							string text3 = mapManifest.bundle.Trim();
							string text4 = Path.Combine(directoryName, text3);
							if (!File.Exists(text4))
							{
								base.Logger.LogWarning("[MAPS] '" + mapManifest.id + "' bundle does not exist: " + text4);
							}
							else
							{
								Plugin.RegisteredMap registeredMap = new Plugin.RegisteredMap
								{
									Manifest = mapManifest,
									Folder = directoryName,
									BundlePath = text4,
									SceneName = ((mapManifest.scene != null) ? mapManifest.scene.Trim() : ""),
									Position = this.ResolveMapPosition(mapManifest, num),
									RadarColor = this.ResolveRadarColor(mapManifest, num)
								};
								this._maps.Add(registeredMap);
								num++;
								base.Logger.LogInfo(string.Concat(new string[]
								{
									"[MAPS] Discovered '",
									mapManifest.name ?? mapManifest.id,
									"' bundle='",
									text3,
									"'."
								}));
							}
						}
					}
				}
				catch (Exception ex)
				{
					base.Logger.LogError("[MAPS] Failed reading '" + text + "': " + ex.Message);
				}
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000025A0 File Offset: 0x000007A0
		private string[] DiscoverMapManifests()
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this.AddMapManifestsFromMapsRoot(this.LegacyMapsRoot, hashSet);
			try
			{
				if (Directory.Exists(this.PluginsRoot))
				{
					foreach (string text in this.EnumerateDirectoriesSafe(this.PluginsRoot, "Maps"))
					{
						this.AddMapManifestsFromMapsRoot(text, hashSet);
					}
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogWarning("[MAPS] Failed scanning plugin packages: " + ex.Message);
			}
			return hashSet.OrderBy<string, string>((string p) => p, StringComparer.OrdinalIgnoreCase).ToArray<string>();
		}

		// Token: 0x0600000A RID: 10 RVA: 0x0000267C File Offset: 0x0000087C
		private void AddMapManifestsFromMapsRoot(string mapsRoot, HashSet<string> results)
		{
			if (string.IsNullOrWhiteSpace(mapsRoot) || results == null || !Directory.Exists(mapsRoot))
			{
				return;
			}
			try
			{
				foreach (string text in Directory.GetFiles(mapsRoot, "map.json", SearchOption.AllDirectories))
				{
					try
					{
						results.Add(Path.GetFullPath(text));
					}
					catch
					{
						results.Add(text);
					}
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogWarning("[MAPS] Could not scan '" + mapsRoot + "': " + ex.Message);
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000271C File Offset: 0x0000091C
		private IEnumerable<string> EnumerateDirectoriesSafe(string root, string wantedName)
		{
			Plugin.<EnumerateDirectoriesSafe>d__41 <EnumerateDirectoriesSafe>d__ = new Plugin.<EnumerateDirectoriesSafe>d__41(-2);
			<EnumerateDirectoriesSafe>d__.<>3__root = root;
			<EnumerateDirectoriesSafe>d__.<>3__wantedName = wantedName;
			return <EnumerateDirectoriesSafe>d__;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002734 File Offset: 0x00000934
		private void ApplyExplicitVectorOverrides(Plugin.MapManifest manifest, string json, string manifestPath)
		{
			if (manifest == null || string.IsNullOrWhiteSpace(json))
			{
				return;
			}
			Plugin.SerializableVector3 serializableVector;
			if (Plugin.TryReadVector3Object(json, "position", out serializableVector))
			{
				manifest.position = serializableVector;
			}
			if (manifest.runtime == null)
			{
				manifest.runtime = new Plugin.RuntimeManifest();
			}
			string text;
			if (Plugin.TryReadObjectBody(json, "runtime", out text))
			{
				if (Plugin.TryReadVector3Object(text, "defaultPlayerSpawn", out serializableVector))
				{
					manifest.runtime.defaultPlayerSpawn = serializableVector;
				}
				if (Plugin.TryReadVector3Object(text, "defaultBoatSpawn", out serializableVector))
				{
					manifest.runtime.defaultBoatSpawn = serializableVector;
				}
			}
			if (this._verbose != null && this._verbose.Value)
			{
				ManualLogSource logger = base.Logger;
				string[] array = new string[6];
				array[0] = "[JSON] Parsed vectors from '";
				array[1] = manifestPath;
				array[2] = "': ";
				int num = 3;
				string text2 = "position={0}, ";
				Plugin.SerializableVector3 position = manifest.position;
				array[num] = string.Format(text2, (position != null) ? new Vector3?(position.ToVector3()) : null);
				int num2 = 4;
				string text3 = "defaultPlayerSpawn={0}, ";
				Plugin.RuntimeManifest runtime = manifest.runtime;
				Vector3? vector;
				if (runtime == null)
				{
					vector = null;
				}
				else
				{
					Plugin.SerializableVector3 defaultPlayerSpawn = runtime.defaultPlayerSpawn;
					vector = ((defaultPlayerSpawn != null) ? new Vector3?(defaultPlayerSpawn.ToVector3()) : null);
				}
				array[num2] = string.Format(text3, vector);
				int num3 = 5;
				string text4 = "defaultBoatSpawn={0}.";
				Plugin.RuntimeManifest runtime2 = manifest.runtime;
				Vector3? vector2;
				if (runtime2 == null)
				{
					vector2 = null;
				}
				else
				{
					Plugin.SerializableVector3 defaultBoatSpawn = runtime2.defaultBoatSpawn;
					vector2 = ((defaultBoatSpawn != null) ? new Vector3?(defaultBoatSpawn.ToVector3()) : null);
				}
				array[num3] = string.Format(text4, vector2);
				logger.LogInfo(string.Concat(array));
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000028BC File Offset: 0x00000ABC
		private static bool TryReadVector3Object(string json, string propertyName, out Plugin.SerializableVector3 value)
		{
			value = null;
			string text;
			if (!Plugin.TryReadObjectBody(json, propertyName, out text))
			{
				return false;
			}
			float num;
			float num2;
			float num3;
			if (!Plugin.TryReadFloatProperty(text, "x", out num) || !Plugin.TryReadFloatProperty(text, "y", out num2) || !Plugin.TryReadFloatProperty(text, "z", out num3))
			{
				return false;
			}
			value = new Plugin.SerializableVector3
			{
				x = num,
				y = num2,
				z = num3
			};
			return true;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002928 File Offset: 0x00000B28
		private static bool TryReadObjectBody(string json, string propertyName, out string body)
		{
			body = null;
			if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
			{
				return false;
			}
			Match match = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\\{", RegexOptions.IgnoreCase);
			if (!match.Success)
			{
				return false;
			}
			int num = json.IndexOf('{', match.Index);
			if (num < 0)
			{
				return false;
			}
			int num2 = 0;
			bool flag = false;
			bool flag2 = false;
			for (int i = num; i < json.Length; i++)
			{
				char c = json[i];
				if (flag)
				{
					if (flag2)
					{
						flag2 = false;
					}
					else if (c == '\\')
					{
						flag2 = true;
					}
					else if (c == '"')
					{
						flag = false;
					}
				}
				else if (c == '"')
				{
					flag = true;
				}
				else if (c == '{')
				{
					num2++;
				}
				else if (c == '}')
				{
					num2--;
					if (num2 == 0)
					{
						body = json.Substring(num + 1, i - num - 1);
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002A04 File Offset: 0x00000C04
		private static bool TryReadFloatProperty(string objectBody, string propertyName, out float value)
		{
			value = 0f;
			if (string.IsNullOrWhiteSpace(objectBody))
			{
				return false;
			}
			Match match = Regex.Match(objectBody, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*([-+]?(?:\\d+(?:\\.\\d+)?|\\.\\d+)(?:[eE][-+]?\\d+)?)", RegexOptions.IgnoreCase);
			return match.Success && float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002A6C File Offset: 0x00000C6C
		private bool ValidateManifest(Plugin.MapManifest manifest, string manifestPath)
		{
			if (manifest == null)
			{
				base.Logger.LogWarning("[MAPS] Invalid JSON manifest: " + manifestPath + ".");
				return false;
			}
			List<string> list = new List<string>();
			if (string.IsNullOrWhiteSpace(manifest.id))
			{
				list.Add("id");
			}
			if (string.IsNullOrWhiteSpace(manifest.name))
			{
				list.Add("name");
			}
			if (string.IsNullOrWhiteSpace(manifest.author))
			{
				list.Add("author");
			}
			if (string.IsNullOrWhiteSpace(manifest.bundle))
			{
				list.Add("bundle");
			}
			if (string.IsNullOrWhiteSpace(manifest.scene))
			{
				list.Add("scene");
			}
			if (string.IsNullOrWhiteSpace(manifest.positionMode))
			{
				list.Add("positionMode");
			}
			if (list.Count > 0)
			{
				base.Logger.LogWarning(string.Concat(new string[]
				{
					"[MAPS] '",
					manifestPath,
					"' is missing required field(s): ",
					string.Join(", ", list),
					". See MAP_JSON_REFERENCE.md."
				}));
				return false;
			}
			string text = manifest.positionMode.Trim();
			if (!string.Equals(text, "auto", StringComparison.OrdinalIgnoreCase) && !string.Equals(text, "manual", StringComparison.OrdinalIgnoreCase))
			{
				base.Logger.LogWarning(string.Concat(new string[] { "[MAPS] '", manifest.id, "' has invalid positionMode='", manifest.positionMode, "'. Use 'auto' or 'manual'." }));
				return false;
			}
			if (string.Equals(text, "manual", StringComparison.OrdinalIgnoreCase) && manifest.position == null)
			{
				base.Logger.LogWarning("[MAPS] '" + manifest.id + "' uses manual positioning but has no position object.");
				return false;
			}
			Color color;
			if (!string.IsNullOrWhiteSpace(manifest.radarColor) && !ColorUtility.TryParseHtmlString(manifest.radarColor.Trim(), ref color))
			{
				base.Logger.LogWarning(string.Concat(new string[] { "[MAPS] '", manifest.id, "' has invalid radarColor='", manifest.radarColor, "'. Use a hex color like #20DFFF." }));
				return false;
			}
			if (manifest.runtime == null)
			{
				base.Logger.LogWarning("[MAPS] '" + manifest.id + "' is missing runtime. Include the runtime object even if you keep the default marker names.");
				return false;
			}
			if (manifest.wildlife == null)
			{
				base.Logger.LogWarning("[MAPS] '" + manifest.id + "' is missing wildlife. Include the wildlife object and set seagulls/clams explicitly.");
				return false;
			}
			return true;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002CD4 File Offset: 0x00000ED4
		private Vector3 ResolveMapPosition(Plugin.MapManifest manifest, int order)
		{
			if (manifest == null || !string.Equals(manifest.positionMode, "manual", StringComparison.OrdinalIgnoreCase))
			{
				int num = order / 8;
				int num2 = order % 8;
				float num3 = 650f + (float)num * 450f;
				float num4 = 0.7853982f * (float)num2 + 0.3926991f;
				return new Vector3(Mathf.Cos(num4) * num3, 0f, Mathf.Sin(num4) * num3);
			}
			if (manifest.position == null)
			{
				return Vector3.zero;
			}
			return manifest.position.ToVector3();
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002D54 File Offset: 0x00000F54
		private Color ResolveRadarColor(Plugin.MapManifest manifest, int order)
		{
			Color color;
			if (manifest != null && !string.IsNullOrWhiteSpace(manifest.radarColor) && ColorUtility.TryParseHtmlString(manifest.radarColor.Trim(), ref color))
			{
				color.a = 1f;
				return color;
			}
			Color[] array = new Color[]
			{
				new Color(0.15f, 0.85f, 1f, 1f),
				new Color(0.75f, 0.35f, 1f, 1f),
				new Color(1f, 0.35f, 0.75f, 1f),
				new Color(0.3f, 1f, 0.65f, 1f),
				new Color(0.45f, 0.65f, 1f, 1f),
				new Color(1f, 0.8f, 0.25f, 1f)
			};
			return array[Math.Abs(order) % array.Length];
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002E6C File Offset: 0x0000106C
		private void TryRegisterMaps()
		{
			object obj = Plugin.FindLiveObjectByTypeName("IslandManager");
			if (obj == null)
			{
				return;
			}
			FieldInfo fieldInfo = Plugin.FindField(obj.GetType(), "_islandInfos");
			Array array = ((fieldInfo != null) ? (fieldInfo.GetValue(obj) as Array) : null);
			if (array == null || array.Length < 5)
			{
				return;
			}
			if (this._managerFirstSeenTime < 0f)
			{
				this._managerFirstSeenTime = Time.unscaledTime;
				return;
			}
			if (Time.unscaledTime - this._managerFirstSeenTime < 1.5f)
			{
				return;
			}
			try
			{
				this.CacheNativeGroundCollision();
				int length = array.Length;
				object value = array.GetValue(Math.Min(4, array.Length - 1));
				if (value == null)
				{
					base.Logger.LogError("[REGISTER] No IslandInfo template available.");
				}
				else
				{
					Type type = value.GetType();
					FieldInfo fieldInfo2 = Plugin.FindField(type, "_spawnPosition");
					FieldInfo fieldInfo3 = Plugin.FindField(type, "_warning");
					GameObject gameObject = ((fieldInfo2 != null) ? fieldInfo2.GetValue(value) : null) as GameObject;
					GameObject gameObject2 = ((fieldInfo3 != null) ? fieldInfo3.GetValue(value) : null) as GameObject;
					if (fieldInfo2 == null || fieldInfo3 == null || gameObject == null)
					{
						base.Logger.LogError("[REGISTER] Native IslandInfo trigger template is incomplete.");
					}
					else
					{
						this.ResolveNonConflictingMapPositions(array, fieldInfo2);
						int num = array.Length + this._maps.Count;
						Array array2 = Array.CreateInstance(type, num);
						Array.Copy(array, array2, array.Length);
						for (int i = 0; i < this._maps.Count; i++)
						{
							Plugin.RegisteredMap registeredMap = this._maps[i];
							int num2 = length + i;
							if (num2 > 255)
							{
								base.Logger.LogError("[REGISTER] Too many maps for byte island indexes.");
								break;
							}
							registeredMap.LogicalIndex = (byte)num2;
							object obj2 = this.CreateIslandInfoForMap(registeredMap, type, gameObject, gameObject2, fieldInfo2, fieldInfo3);
							if (obj2 != null)
							{
								array2.SetValue(obj2, num2);
								this._mapsByIndex[registeredMap.LogicalIndex] = registeredMap;
								base.Logger.LogWarning(string.Concat(new string[]
								{
									"[REGISTER] ",
									registeredMap.Manifest.name,
									" ",
									string.Format("index={0} ", registeredMap.LogicalIndex),
									"scene='",
									registeredMap.Manifest.scene,
									"' ",
									string.Format("pos={0} ", registeredMap.Position),
									"bundle='",
									registeredMap.Manifest.bundle,
									"' radar=",
									registeredMap.Manifest.radarColor
								}));
							}
						}
						fieldInfo.SetValue(obj, array2);
						this._registered = true;
						base.Logger.LogWarning(string.Format("[REGISTER] Added {0} custom map(s). ", this._mapsByIndex.Count) + string.Format("IslandInfo {0} -> {1}.", array.Length, array2.Length));
						this.ResetRadarReferences();
					}
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogError(string.Format("[REGISTER] Failed: {0}", ex));
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000031D0 File Offset: 0x000013D0
		private void ResolveNonConflictingMapPositions(Array existingInfos, FieldInfo spawnField)
		{
			List<Vector3> list = new List<Vector3>();
			if (existingInfos != null)
			{
				for (int i = 0; i < existingInfos.Length; i++)
				{
					object value = existingInfos.GetValue(i);
					Vector3 vector;
					if (value != null && this.TryGetIslandInfoPosition(value, spawnField, out vector))
					{
						list.Add(vector);
						if (this._verbose.Value)
						{
							base.Logger.LogInfo(string.Format("[PLACEMENT] Existing island index={0} pos={1}.", i, vector));
						}
					}
				}
			}
			for (int j = 0; j < this._maps.Count; j++)
			{
				Plugin.RegisteredMap registeredMap = this._maps[j];
				Vector3 position = registeredMap.Position;
				Vector3 vector2;
				if (!Plugin.FindPositionConflict(position, list, 550f, out vector2))
				{
					list.Add(position);
				}
				else
				{
					Vector3 vector3 = this.FindFreeNavigationPosition(list, j, 550f);
					base.Logger.LogWarning(string.Format("[PLACEMENT] '{0}' requested {1}, but that ", registeredMap.Manifest.id, position) + string.Format("position conflicts with an existing island at {0}. ", vector2) + string.Format("Automatically moved navigation/radar position to {0}.", vector3));
					registeredMap.Position = vector3;
					list.Add(vector3);
				}
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00003308 File Offset: 0x00001508
		private bool TryGetIslandInfoPosition(object info, FieldInfo fallbackSpawnField, out Vector3 position)
		{
			position = Vector3.zero;
			if (info == null)
			{
				return false;
			}
			try
			{
				PropertyInfo property = info.GetType().GetProperty("IslandPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null && property.PropertyType == typeof(Vector3) && property.CanRead)
				{
					position = (Vector3)property.GetValue(info, null);
					return true;
				}
			}
			catch
			{
			}
			try
			{
				FieldInfo fieldInfo = fallbackSpawnField ?? Plugin.FindField(info.GetType(), "_spawnPosition");
				GameObject gameObject = ((fieldInfo != null) ? fieldInfo.GetValue(info) : null) as GameObject;
				if (gameObject != null)
				{
					position = gameObject.transform.position;
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000033E8 File Offset: 0x000015E8
		private static bool FindPositionConflict(Vector3 candidate, IEnumerable<Vector3> occupied, float minimumSpacing, out Vector3 conflictingPosition)
		{
			conflictingPosition = Vector3.zero;
			foreach (Vector3 vector in occupied)
			{
				Vector2 vector2 = new Vector2(candidate.x, candidate.z);
				Vector2 vector3;
				vector3..ctor(vector.x, vector.z);
				if (Vector2.Distance(vector2, vector3) < minimumSpacing)
				{
					conflictingPosition = vector;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00003470 File Offset: 0x00001670
		private Vector3 FindFreeNavigationPosition(IEnumerable<Vector3> occupied, int mapOrder, float minimumSpacing)
		{
			List<Vector3> list = occupied.ToList<Vector3>();
			for (int i = 0; i < 64; i++)
			{
				float num = 1250f + (float)i * 450f;
				for (int j = 0; j < 12; j++)
				{
					int num2 = (j + mapOrder * 3) % 12;
					float num3 = 0.5235988f * (float)num2 + 0.2617994f;
					Vector3 vector;
					vector..ctor(Mathf.Cos(num3) * num, 0f, Mathf.Sin(num3) * num);
					Vector3 vector2;
					if (!Plugin.FindPositionConflict(vector, list, minimumSpacing, out vector2))
					{
						return vector;
					}
				}
			}
			return new Vector3(2500f + (float)mapOrder * 650f, 0f, 2500f);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00003514 File Offset: 0x00001714
		private object CreateIslandInfoForMap(Plugin.RegisteredMap map, Type infoType, GameObject sourceSpawn, GameObject sourceWarning, FieldInfo spawnField, FieldInfo warningField)
		{
			Transform parent = sourceSpawn.transform.parent;
			GameObject gameObject = ((parent != null) ? Object.Instantiate<GameObject>(parent.gameObject) : new GameObject("HTFML_Trigger_" + map.Manifest.id));
			gameObject.name = "HTFML_IslandPositionHolder_" + map.Manifest.id;
			Transform transform = Plugin.FindChildRecursive(gameObject.transform, sourceSpawn.name);
			if (transform == null)
			{
				base.Logger.LogError("[REGISTER] '" + map.Manifest.id + "': cloned spawn not found.");
				Object.Destroy(gameObject);
				return null;
			}
			Vector3 vector = map.Position - transform.position;
			gameObject.transform.position += vector;
			GameObject gameObject2 = transform.gameObject;
			gameObject2.name = "HTFML_IslandPosition_" + map.Manifest.id;
			GameObject gameObject3 = null;
			if (sourceWarning != null)
			{
				Transform transform2 = Plugin.FindChildRecursive(gameObject.transform, sourceWarning.name);
				if (transform2 != null)
				{
					gameObject3 = transform2.gameObject;
					gameObject3.name = "HTFML_IslandWarning_" + map.Manifest.id;
				}
			}
			Type type = AccessTools.TypeByName("IslandSpawner");
			Component component = ((type != null) ? gameObject2.GetComponent(type) : null);
			FieldInfo fieldInfo = ((component != null) ? Plugin.FindField(component.GetType(), "_islandIndex") : null);
			if (component == null || fieldInfo == null)
			{
				base.Logger.LogError("[REGISTER] '" + map.Manifest.id + "': IslandSpawner/index missing.");
				Object.Destroy(gameObject);
				return null;
			}
			fieldInfo.SetValue(component, Convert.ChangeType(map.LogicalIndex, fieldInfo.FieldType));
			object obj = Activator.CreateInstance(infoType, true);
			spawnField.SetValue(obj, gameObject2);
			warningField.SetValue(obj, gameObject3);
			gameObject.SetActive(true);
			gameObject2.SetActive(true);
			this.RepairClonedTriggerGeometry((parent != null) ? parent.gameObject : null, gameObject);
			this.DumpRegisteredNavigationTrigger(map, gameObject, gameObject2, component);
			return obj;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00003758 File Offset: 0x00001958
		private void RepairClonedTriggerGeometry(GameObject sourceHolder, GameObject clonedHolder)
		{
			if (sourceHolder == null || clonedHolder == null)
			{
				return;
			}
			Collider[] componentsInChildren = sourceHolder.GetComponentsInChildren<Collider>(true);
			Collider[] componentsInChildren2 = clonedHolder.GetComponentsInChildren<Collider>(true);
			int num = Math.Min(componentsInChildren.Length, componentsInChildren2.Length);
			for (int i = 0; i < num; i++)
			{
				Collider collider = componentsInChildren[i];
				Collider collider2 = componentsInChildren2[i];
				if (!(collider == null) && !(collider2 == null) && !(collider.GetType() != collider2.GetType()))
				{
					bool enabled = collider2.enabled;
					collider2.enabled = false;
					SphereCollider sphereCollider = collider as SphereCollider;
					if (sphereCollider == null)
					{
						goto IL_00BE;
					}
					SphereCollider sphereCollider2 = collider2 as SphereCollider;
					if (sphereCollider2 == null)
					{
						goto IL_00BE;
					}
					sphereCollider2.center = sphereCollider.center;
					sphereCollider2.radius = sphereCollider.radius;
					IL_0148:
					Physics.SyncTransforms();
					collider2.enabled = enabled;
					goto IL_0156;
					IL_00BE:
					BoxCollider boxCollider = collider as BoxCollider;
					if (boxCollider != null)
					{
						BoxCollider boxCollider2 = collider2 as BoxCollider;
						if (boxCollider2 != null)
						{
							boxCollider2.center = boxCollider.center;
							boxCollider2.size = boxCollider.size;
							goto IL_0148;
						}
					}
					CapsuleCollider capsuleCollider = collider as CapsuleCollider;
					if (capsuleCollider == null)
					{
						goto IL_0148;
					}
					CapsuleCollider capsuleCollider2 = collider2 as CapsuleCollider;
					if (capsuleCollider2 != null)
					{
						capsuleCollider2.center = capsuleCollider.center;
						capsuleCollider2.radius = capsuleCollider.radius;
						capsuleCollider2.height = capsuleCollider.height;
						capsuleCollider2.direction = capsuleCollider.direction;
						goto IL_0148;
					}
					goto IL_0148;
				}
				IL_0156:;
			}
			Physics.SyncTransforms();
			base.Logger.LogWarning(string.Format("[TRIGGER FIX] Refreshed {0} cloned collider(s) after moving ", num) + string.Format("'{0}' to {1}.", clonedHolder.name, clonedHolder.transform.position));
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000390C File Offset: 0x00001B0C
		private void DumpRegisteredNavigationTrigger(Plugin.RegisteredMap map, GameObject holder, GameObject spawn, Component spawner)
		{
			if (map == null || holder == null || spawn == null)
			{
				return;
			}
			try
			{
				Collider[] componentsInChildren = holder.GetComponentsInChildren<Collider>(true);
				ManualLogSource logger = base.Logger;
				string[] array = new string[12];
				array[0] = "[TRIGGER DUMP] '";
				array[1] = map.Manifest.id;
				array[2] = "' holder='";
				array[3] = holder.name;
				array[4] = "' ";
				array[5] = string.Format("holderActive={0} ", holder.activeInHierarchy);
				array[6] = string.Format("spawn='{0}' spawnActive={1} ", spawn.name, spawn.activeInHierarchy);
				array[7] = string.Format("spawnPos={0} ", spawn.transform.position);
				array[8] = "spawnerEnabled=";
				int num = 9;
				Behaviour behaviour = spawner as Behaviour;
				array[num] = ((behaviour != null) ? behaviour.enabled.ToString() : "n/a");
				array[10] = " ";
				array[11] = string.Format("colliders={0}.", componentsInChildren.Length);
				logger.LogWarning(string.Concat(array));
				foreach (Collider collider in componentsInChildren)
				{
					if (!(collider == null))
					{
						Rigidbody component = collider.GetComponent<Rigidbody>();
						base.Logger.LogWarning(string.Concat(new string[]
						{
							"[TRIGGER DUMP]   collider='",
							collider.name,
							"' ",
							string.Format("type={0} enabled={1} ", collider.GetType().Name, collider.enabled),
							string.Format("isTrigger={0} active={1} ", collider.isTrigger, collider.gameObject.activeInHierarchy),
							string.Format("layer={0} tag='{1}' ", collider.gameObject.layer, Plugin.SafeTag(collider.gameObject)),
							string.Format("center={0} size={1} ", collider.bounds.center, collider.bounds.size),
							"rb=",
							(component != null) ? string.Format("kinematic={0}", component.isKinematic) : "none",
							"."
						}));
					}
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogWarning("[TRIGGER DUMP] Failed for '" + map.Manifest.id + "': " + ex.Message);
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00003BBC File Offset: 0x00001DBC
		private static string SafeTag(GameObject go)
		{
			if (go == null)
			{
				return "<null>";
			}
			string text;
			try
			{
				text = go.tag;
			}
			catch
			{
				text = "<unavailable>";
			}
			return text;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003BFC File Offset: 0x00001DFC
		private void PatchNativeIslandSpawnerTrigger()
		{
			try
			{
				Type type = AccessTools.TypeByName("IslandSpawner");
				if (type == null)
				{
					base.Logger.LogWarning("[PATCH] IslandSpawner type not found.");
				}
				else
				{
					int num = 0;
					foreach (string text in new string[] { "OnTriggerEnter", "OnTriggerStay" })
					{
						MethodInfo method = type.GetMethod(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(Collider) }, null);
						if (!(method == null))
						{
							this._harmony.Patch(method, null, new HarmonyMethod(typeof(Plugin), "NativeIslandSpawnerTriggerPostfix", null), null, null, null);
							num++;
							base.Logger.LogInfo("[PATCH] IslandSpawner." + text + "(Collider) patched.");
						}
					}
					if (num == 0)
					{
						base.Logger.LogWarning("[PATCH] No IslandSpawner trigger callbacks were found.");
					}
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogError(string.Format("[PATCH] IslandSpawner trigger patch failed: {0}", ex));
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003D14 File Offset: 0x00001F14
		private static void NativeIslandSpawnerTriggerPostfix(object __instance, Collider other)
		{
			Plugin instance = Plugin.Instance;
			if (instance == null || !instance._enabled.Value || __instance == null || other == null)
			{
				return;
			}
			FieldInfo fieldInfo = Plugin.FindField(__instance.GetType(), "_islandIndex");
			if (fieldInfo == null)
			{
				return;
			}
			byte b;
			try
			{
				b = Convert.ToByte(fieldInfo.GetValue(__instance));
			}
			catch
			{
				return;
			}
			Plugin.RegisteredMap registeredMap;
			if (!instance._mapsByIndex.TryGetValue(b, out registeredMap))
			{
				return;
			}
			ManualLogSource logger = instance.Logger;
			string[] array = new string[5];
			array[0] = string.Format("[ISLAND SPAWNER HIT] map='{0}' index={1} ", registeredMap.Manifest.id, b);
			array[1] = "spawner='";
			int num = 2;
			Component component = __instance as Component;
			string text;
			if (component == null)
			{
				text = null;
			}
			else
			{
				GameObject gameObject = component.gameObject;
				text = ((gameObject != null) ? gameObject.name : null);
			}
			array[num] = text;
			array[3] = "' ";
			array[4] = string.Format("other='{0}' otherLayer={1}.", other.name, other.gameObject.layer);
			logger.LogWarning(string.Concat(array));
			if (instance.IsCustomMapAlreadyActive(registeredMap) || instance._transitioning)
			{
				return;
			}
			if (!Plugin.IsGameplayBoatOrPlayerCollider(other))
			{
				if (instance._verbose.Value)
				{
					instance.Logger.LogInfo(string.Concat(new string[]
					{
						"[ISLAND SPAWNER] '",
						registeredMap.Manifest.id,
						"' ignored collider '",
						other.name,
						"'."
					}));
				}
				return;
			}
			object obj = Plugin.FindLiveObjectByTypeName("IslandManager");
			if (obj == null)
			{
				return;
			}
			FieldInfo fieldInfo2 = Plugin.FindField(obj.GetType(), "_hasQueuedIsland");
			try
			{
				if (fieldInfo2 != null && fieldInfo2.FieldType == typeof(bool) && (bool)fieldInfo2.GetValue(obj))
				{
					return;
				}
			}
			catch
			{
			}
			MethodInfo methodInfo = AccessTools.Method(obj.GetType(), "QueueRequest", new Type[] { typeof(byte) }, null);
			if (methodInfo == null)
			{
				return;
			}
			instance.Logger.LogWarning("[ISLAND SPAWNER] Native trigger reached custom map " + string.Format("'{0}' index={1} with collider ", registeredMap.Manifest.id, b) + string.Format("'{0}'. Forwarding QueueRequest({1}).", other.name, b));
			try
			{
				methodInfo.Invoke(obj, new object[] { b });
			}
			catch (Exception ex)
			{
				instance.Logger.LogWarning("[ISLAND SPAWNER] QueueRequest repair failed: " + ex.Message);
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00003FBC File Offset: 0x000021BC
		private static bool IsGameplayBoatOrPlayerCollider(Collider other)
		{
			Transform transform = ((other != null) ? other.transform : null);
			Type type = AccessTools.TypeByName("Boat");
			Type type2 = AccessTools.TypeByName("Player");
			while (transform != null)
			{
				GameObject gameObject = transform.gameObject;
				string text = gameObject.name ?? "";
				if (string.Equals(text, "MenuBoat", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				if (type != null && gameObject.GetComponent(type) != null)
				{
					return true;
				}
				if (type2 != null && gameObject.GetComponent(type2) != null)
				{
					return true;
				}
				if (text.IndexOf("HostBoatCol", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("LocalPlayer", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
				transform = transform.parent;
			}
			return false;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000408C File Offset: 0x0000228C
		private void PatchQueueRequest()
		{
			try
			{
				Type type = AccessTools.TypeByName("IslandManager");
				MethodInfo methodInfo = ((type != null) ? AccessTools.Method(type, "QueueRequest", new Type[] { typeof(byte) }, null) : null);
				if (methodInfo == null)
				{
					base.Logger.LogError("[PATCH] IslandManager.QueueRequest(byte) not found.");
				}
				else
				{
					this._harmony = new Harmony("com.howtofish.maploader");
					this._harmony.Patch(methodInfo, new HarmonyMethod(typeof(Plugin), "QueueRequestPrefix", null), null, null, null, null);
					base.Logger.LogInfo("[PATCH] IslandManager.QueueRequest(byte) patched.");
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogError(string.Format("[PATCH] QueueRequest failed: {0}", ex));
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000415C File Offset: 0x0000235C
		private static bool QueueRequestPrefix(object __instance, byte islandId)
		{
			if (Plugin.Instance == null || !Plugin.Instance._enabled.Value || Plugin.Instance._bypassQueuePatch)
			{
				return true;
			}
			Plugin.RegisteredMap registeredMap;
			if (Plugin.Instance._mapsByIndex.TryGetValue(islandId, out registeredMap))
			{
				if (Plugin.Instance.IsCustomMapAlreadyActive(registeredMap))
				{
					if (Plugin.Instance._verbose.Value)
					{
						Plugin.Instance.Logger.LogInfo(string.Format("[QUEUE] Ignoring duplicate QueueRequest({0}) ", islandId) + "for already active map '" + registeredMap.Manifest.id + "'.");
					}
					return false;
				}
				if (!Plugin.Instance._transitioning)
				{
					Plugin.Instance.StartCoroutine(Plugin.Instance.TransitionToCustomMap(registeredMap));
				}
				return false;
			}
			else
			{
				if (Plugin.Instance._currentMap != null)
				{
					if (!Plugin.Instance._transitioning)
					{
						Plugin.Instance.StartCoroutine(Plugin.Instance.LeaveCustomAndQueueNative(__instance, islandId));
					}
					return false;
				}
				return true;
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00004258 File Offset: 0x00002458
		private bool IsCustomMapAlreadyActive(Plugin.RegisteredMap target)
		{
			if (target == null)
			{
				return false;
			}
			if (this._currentMap != null && this._currentMap == target)
			{
				Scene sceneByName = SceneManager.GetSceneByName(target.SceneName);
				if (sceneByName.IsValid() && sceneByName.isLoaded)
				{
					return true;
				}
			}
			if (!string.IsNullOrWhiteSpace(target.SceneName))
			{
				Scene sceneByName2 = SceneManager.GetSceneByName(target.SceneName);
				if (sceneByName2.IsValid() && sceneByName2.isLoaded)
				{
					this._currentMap = target;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000042D0 File Offset: 0x000024D0
		private IEnumerator TransitionToCustomMap(Plugin.RegisteredMap target)
		{
			Plugin.<TransitionToCustomMap>d__64 <TransitionToCustomMap>d__ = new Plugin.<TransitionToCustomMap>d__64(0);
			<TransitionToCustomMap>d__.<>4__this = this;
			<TransitionToCustomMap>d__.target = target;
			return <TransitionToCustomMap>d__;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x000042E6 File Offset: 0x000024E6
		private IEnumerator LeaveCustomAndQueueNative(object islandManager, byte islandId)
		{
			Plugin.<LeaveCustomAndQueueNative>d__65 <LeaveCustomAndQueueNative>d__ = new Plugin.<LeaveCustomAndQueueNative>d__65(0);
			<LeaveCustomAndQueueNative>d__.<>4__this = this;
			<LeaveCustomAndQueueNative>d__.islandManager = islandManager;
			<LeaveCustomAndQueueNative>d__.islandId = islandId;
			return <LeaveCustomAndQueueNative>d__;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00004303 File Offset: 0x00002503
		private IEnumerator UnloadAllGameplayIslandsExceptGame()
		{
			Plugin.<UnloadAllGameplayIslandsExceptGame>d__66 <UnloadAllGameplayIslandsExceptGame>d__ = new Plugin.<UnloadAllGameplayIslandsExceptGame>d__66(0);
			<UnloadAllGameplayIslandsExceptGame>d__.<>4__this = this;
			return <UnloadAllGameplayIslandsExceptGame>d__;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00004312 File Offset: 0x00002512
		private IEnumerator UnloadMapScene(Plugin.RegisteredMap map)
		{
			Plugin.<UnloadMapScene>d__67 <UnloadMapScene>d__ = new Plugin.<UnloadMapScene>d__67(0);
			<UnloadMapScene>d__.map = map;
			return <UnloadMapScene>d__;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00004324 File Offset: 0x00002524
		private bool EnsureBundleLoaded(Plugin.RegisteredMap map)
		{
			if (map.Bundle != null && !string.IsNullOrWhiteSpace(map.SceneName))
			{
				return true;
			}
			bool flag;
			try
			{
				map.Bundle = AssetBundle.LoadFromFile(map.BundlePath);
				if (map.Bundle == null)
				{
					base.Logger.LogError("[BUNDLE] LoadFromFile failed: " + map.BundlePath);
					flag = false;
				}
				else
				{
					string[] allScenePaths = map.Bundle.GetAllScenePaths();
					if (allScenePaths == null || allScenePaths.Length == 0)
					{
						base.Logger.LogError("[BUNDLE] '" + map.Manifest.id + "' contains no scenes.");
						flag = false;
					}
					else
					{
						string requested = map.Manifest.scene ?? "";
						string text = null;
						if (!string.IsNullOrWhiteSpace(requested))
						{
							text = allScenePaths.FirstOrDefault<string>((string p) => string.Equals(Path.GetFileNameWithoutExtension(p), requested, StringComparison.OrdinalIgnoreCase) || string.Equals(p, requested, StringComparison.OrdinalIgnoreCase));
						}
						if (text == null)
						{
							base.Logger.LogError(string.Concat(new string[]
							{
								"[BUNDLE] '",
								map.Manifest.id,
								"' requested scene '",
								requested,
								"', but that scene was not found in the bundle."
							}));
							base.Logger.LogError("[BUNDLE] Available scenes: " + string.Join(", ", allScenePaths));
							flag = false;
						}
						else
						{
							map.ScenePath = text;
							map.SceneName = Path.GetFileNameWithoutExtension(text);
							base.Logger.LogWarning(string.Concat(new string[]
							{
								"[BUNDLE] '",
								map.Manifest.id,
								"' -> scene '",
								map.SceneName,
								"'."
							}));
							flag = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogError(string.Format("[BUNDLE] '{0}' failed: {1}", map.Manifest.id, ex));
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00004524 File Offset: 0x00002724
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.IsValid() && scene.isLoaded && (string.Equals(scene.name, "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(scene.name, "DevIsland", StringComparison.OrdinalIgnoreCase) || scene.name.StartsWith("Island", StringComparison.OrdinalIgnoreCase)))
			{
				this.ResetRadarReferences();
			}
			Plugin.RegisteredMap registeredMap = this._maps.FirstOrDefault<Plugin.RegisteredMap>((Plugin.RegisteredMap m) => !string.IsNullOrWhiteSpace(m.SceneName) && string.Equals(m.SceneName, scene.name, StringComparison.OrdinalIgnoreCase));
			if (registeredMap == null)
			{
				return;
			}
			base.Logger.LogWarning("[RUNTIME] Preparing custom scene '" + scene.name + "'.");
			this.PositionMapScene(registeredMap, scene);
			if (this._repairRuntime.Value)
			{
				this.PrepareMapRuntime(registeredMap, scene);
			}
			if (this._enableMapWildlife.Value)
			{
				this.ApplyOptionalWildlife(registeredMap, scene);
			}
			this.ResetRadarReferences();
		}

		// Token: 0x06000028 RID: 40 RVA: 0x0000462E File Offset: 0x0000282E
		private void OnSceneUnloaded(Scene scene)
		{
			if (this._currentMap != null && string.Equals(this._currentMap.SceneName, scene.name, StringComparison.OrdinalIgnoreCase))
			{
				this._currentMap = null;
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x0000465C File Offset: 0x0000285C
		private void PositionMapScene(Plugin.RegisteredMap map, Scene scene)
		{
			if (map == null || !map.Manifest.moveScene)
			{
				ManualLogSource logger = base.Logger;
				string text = "[RUNTIME] '";
				string text2;
				if (map == null)
				{
					text2 = null;
				}
				else
				{
					Plugin.MapManifest manifest = map.Manifest;
					text2 = ((manifest != null) ? manifest.id : null);
				}
				logger.LogInfo(text + text2 + "' moveScene=false; scene coordinates preserved.");
				return;
			}
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			GameObject gameObject = rootGameObjects.FirstOrDefault<GameObject>((GameObject r) => r != null && string.Equals(r.name, "IslandHolder", StringComparison.OrdinalIgnoreCase));
			if (gameObject != null)
			{
				Vector3 position = gameObject.transform.position;
				gameObject.transform.position = map.Position;
				base.Logger.LogWarning(string.Format("[RUNTIME] '{0}' IslandHolder moved {1} -> ", map.Manifest.id, position) + string.Format("{0} (requested {1}).", gameObject.transform.position, map.Position));
				return;
			}
			foreach (GameObject gameObject2 in rootGameObjects)
			{
				if (gameObject2 != null)
				{
					gameObject2.transform.position += map.Position;
				}
			}
			base.Logger.LogWarning("[RUNTIME] '" + map.Manifest.id + "' has no IslandHolder; moved " + string.Format("{0} root(s) by {1}.", rootGameObjects.Length, map.Position));
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000047D4 File Offset: 0x000029D4
		private void PrepareMapRuntime(Plugin.RegisteredMap map, Scene scene)
		{
			GameObject gameObject = scene.GetRootGameObjects().FirstOrDefault<GameObject>((GameObject r) => r != null && string.Equals(r.name, "IslandHolder", StringComparison.OrdinalIgnoreCase));
			if (gameObject == null)
			{
				gameObject = new GameObject("IslandHolder");
				SceneManager.MoveGameObjectToScene(gameObject, scene);
				gameObject.transform.position = (map.Manifest.moveScene ? map.Position : Vector3.zero);
			}
			this.AdaptLevelPhysics(gameObject.transform);
			this.EnsureIslandComponent(map, gameObject.transform);
			this.EnsureSpawnManager(map, gameObject.transform);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00004874 File Offset: 0x00002A74
		private void EnsureIslandComponent(Plugin.RegisteredMap map, Transform holder)
		{
			Type type = AccessTools.TypeByName("Island");
			if (type == null)
			{
				return;
			}
			Transform transform = Plugin.FindChildRecursive(holder, "IslandManager");
			GameObject gameObject;
			if (transform != null)
			{
				gameObject = transform.gameObject;
			}
			else
			{
				gameObject = new GameObject("IslandManager");
				gameObject.transform.SetParent(holder, false);
			}
			Component component = gameObject.GetComponent(type);
			if (component == null)
			{
				component = gameObject.AddComponent(type);
			}
			Plugin.SetFieldIfExists(component, "_islandSize", map.Manifest.islandSize);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00004904 File Offset: 0x00002B04
		private void EnsureSpawnManager(Plugin.RegisteredMap map, Transform holder)
		{
			Type type = AccessTools.TypeByName("SpawnManager");
			if (type == null)
			{
				return;
			}
			Transform transform = Plugin.FindChildRecursive(holder, "SpawnManager");
			GameObject gameObject;
			if (transform != null)
			{
				gameObject = transform.gameObject;
			}
			else
			{
				gameObject = new GameObject("SpawnManager");
				gameObject.transform.SetParent(holder, false);
			}
			gameObject.SetActive(false);
			Plugin.RuntimeManifest runtime = map.Manifest.runtime;
			string text = ((runtime != null) ? runtime.playerSpawn : null) ?? "PlayerSpawnPoint";
			Plugin.RuntimeManifest runtime2 = map.Manifest.runtime;
			string text2 = ((runtime2 != null) ? runtime2.boatSpawn : null) ?? "BoatSpawnPoint";
			Transform transform2 = Plugin.FindChildRecursive(holder, text);
			if (transform2 == null)
			{
				GameObject gameObject2 = new GameObject(text);
				gameObject2.transform.SetParent(gameObject.transform, false);
				Transform transform3 = gameObject2.transform;
				Plugin.RuntimeManifest runtime3 = map.Manifest.runtime;
				Vector3? vector;
				if (runtime3 == null)
				{
					vector = null;
				}
				else
				{
					Plugin.SerializableVector3 defaultPlayerSpawn = runtime3.defaultPlayerSpawn;
					vector = ((defaultPlayerSpawn != null) ? new Vector3?(defaultPlayerSpawn.ToVector3()) : null);
				}
				transform3.localPosition = vector ?? new Vector3(0f, 6f, 0f);
				transform2 = gameObject2.transform;
			}
			Transform transform4 = Plugin.FindChildRecursive(holder, text2);
			if (transform4 == null)
			{
				GameObject gameObject3 = new GameObject(text2);
				gameObject3.transform.SetParent(gameObject.transform, false);
				Transform transform5 = gameObject3.transform;
				Plugin.RuntimeManifest runtime4 = map.Manifest.runtime;
				Vector3? vector2;
				if (runtime4 == null)
				{
					vector2 = null;
				}
				else
				{
					Plugin.SerializableVector3 defaultBoatSpawn = runtime4.defaultBoatSpawn;
					vector2 = ((defaultBoatSpawn != null) ? new Vector3?(defaultBoatSpawn.ToVector3()) : null);
				}
				transform5.localPosition = vector2 ?? new Vector3(0f, 0.5f, -25f);
				transform4 = gameObject3.transform;
			}
			Component component = gameObject.GetComponent(type);
			if (component == null)
			{
				component = gameObject.AddComponent(type);
			}
			Plugin.SetFieldIfExists(component, "_testPrint", "HTF Map Loader: " + map.Manifest.id);
			Plugin.SetFieldIfExists(component, "_playerSpawnPoint", transform2);
			Plugin.SetFieldIfExists(component, "_boatSpawnPoint", transform4);
			object obj = this.FindBoatPrefabInMemory();
			if (obj != null)
			{
				Plugin.SetFieldIfExists(component, "_boatPrefab", obj);
			}
			gameObject.SetActive(true);
			base.Logger.LogInfo(string.Format("[RUNTIME] '{0}' PlayerSpawn={1}, BoatSpawn={2}.", map.Manifest.id, transform2.position, transform4.position));
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00004BA4 File Offset: 0x00002DA4
		private void ApplyOptionalWildlife(Plugin.RegisteredMap map, Scene scene)
		{
			Plugin.WildlifeManifest wildlife = map.Manifest.wildlife;
			if (wildlife == null)
			{
				return;
			}
			if (wildlife.seagulls)
			{
				this.EnsureSeagullSpawner(scene, Mathf.Clamp(wildlife.seagullMax, 1, 20), Mathf.Max(1f, wildlife.seagullDelay));
			}
			if (wildlife.clams)
			{
				this.EnsureClamSpawner(scene, Mathf.Clamp(wildlife.clamMax, 1, 20), Mathf.Max(1f, wildlife.clamDelay));
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00004C1C File Offset: 0x00002E1C
		private void EnsureSeagullSpawner(Scene scene, int maxCount, float delay)
		{
			GameObject gameObject = this.FindIslandHolder(scene);
			if (gameObject == null || Plugin.FindChildRecursive(gameObject.transform, "SeagullSpawner") != null)
			{
				return;
			}
			Type type = AccessTools.TypeByName("ItemSpawner");
			Type type2 = AccessTools.TypeByName("Bird");
			if (type == null || type2 == null)
			{
				return;
			}
			Object @object = this.FindNamedComponentPrefab(type2, "Seagull");
			if (@object == null)
			{
				base.Logger.LogWarning("[WILDLIFE] Seagull prefab not available in memory.");
				return;
			}
			Transform orCreateChild = this.GetOrCreateChild(gameObject.transform, "CreatureSpawners");
			GameObject gameObject2 = new GameObject("SeagullSpawner");
			gameObject2.transform.SetParent(orCreateChild, false);
			gameObject2.transform.localPosition = new Vector3(0f, 20f, 0f);
			gameObject2.SetActive(false);
			BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
			boxCollider.isTrigger = true;
			boxCollider.size = new Vector3(70f, 12f, 70f);
			Component component = gameObject2.AddComponent(type);
			this.ConfigureItemSpawner(component, boxCollider, @object, false, true, maxCount, delay, 1);
			gameObject2.SetActive(true);
			base.Logger.LogInfo(string.Format("[WILDLIFE] SeagullSpawner added max={0}, delay={1:0.#}.", maxCount, delay));
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00004D60 File Offset: 0x00002F60
		private void EnsureClamSpawner(Scene scene, int maxCount, float delay)
		{
			GameObject gameObject = this.FindIslandHolder(scene);
			if (gameObject == null || Plugin.FindChildRecursive(gameObject.transform, "ClamSpawner") != null)
			{
				return;
			}
			Type type = AccessTools.TypeByName("ItemSpawner");
			Type type2 = AccessTools.TypeByName("Creature");
			if (type == null || type2 == null)
			{
				return;
			}
			Object @object = this.FindNamedComponentPrefab(type2, "Clam");
			if (@object == null)
			{
				base.Logger.LogWarning("[WILDLIFE] Clam prefab not available in memory.");
				return;
			}
			Transform orCreateChild = this.GetOrCreateChild(gameObject.transform, "CreatureSpawners");
			GameObject gameObject2 = new GameObject("ClamSpawner");
			gameObject2.transform.SetParent(orCreateChild, false);
			gameObject2.transform.localPosition = new Vector3(0f, 1.5f, 0f);
			gameObject2.SetActive(false);
			BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
			boxCollider.isTrigger = true;
			boxCollider.center = new Vector3(0f, -1.25f, 0f);
			boxCollider.size = new Vector3(28f, 3f, 28f);
			Component component = gameObject2.AddComponent(type);
			this.ConfigureItemSpawner(component, boxCollider, @object, true, false, maxCount, delay, 2);
			gameObject2.SetActive(true);
			base.Logger.LogInfo(string.Format("[WILDLIFE] ClamSpawner added max={0}, delay={1:0.#}.", maxCount, delay));
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00004EC0 File Offset: 0x000030C0
		private void ConfigureItemSpawner(Component spawner, BoxCollider box, Object prefab, bool deadCreature, bool spawnInAir, int max, float delay, int count)
		{
			Plugin.SetFieldIfExists(spawner, "_isActive", true);
			Plugin.SetFieldIfExists(spawner, "_itemToSpawn", prefab);
			Plugin.SetFieldIfExists(spawner, "_spawnsDeadCreature", deadCreature);
			Plugin.SetFieldIfExists(spawner, "_startFrequencyForRadio", 98);
			Plugin.SetFieldIfExists(spawner, "_useRandomRotation", true);
			Plugin.SetFieldIfExists(spawner, "_maxItemsSpawned", max);
			Plugin.SetFieldIfExists(spawner, "_spawnInstant", true);
			Plugin.SetFieldIfExists(spawner, "_onlySpawnOnce", false);
			Plugin.SetFieldIfExists(spawner, "_spawnInAir", spawnInAir);
			Plugin.SetFieldIfExists(spawner, "_spawnDelay", delay);
			Plugin.SetFieldIfExists(spawner, "_spawnCount", count);
			Plugin.SetFieldIfExists(spawner, "_spawnBox", box);
			Plugin.SetFieldIfExists(spawner, "_hasSpawned", false);
			FieldInfo fieldInfo = Plugin.FindField(spawner.GetType(), "_itemsSpawned");
			if (fieldInfo != null)
			{
				try
				{
					fieldInfo.SetValue(spawner, Activator.CreateInstance(fieldInfo.FieldType));
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00004FF4 File Offset: 0x000031F4
		private void VerifyRegisteredNavigationTriggers()
		{
			Type type = AccessTools.TypeByName("IslandSpawner");
			if (type == null)
			{
				return;
			}
			Object[] array = Resources.FindObjectsOfTypeAll(type);
			using (Dictionary<byte, Plugin.RegisteredMap>.ValueCollection.Enumerator enumerator = this._mapsByIndex.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Plugin.RegisteredMap map = enumerator.Current;
					Component component = array.OfType<Component>().FirstOrDefault<Component>(delegate(Component c)
					{
						if (c == null || c.gameObject == null)
						{
							return false;
						}
						FieldInfo fieldInfo = Plugin.FindField(c.GetType(), "_islandIndex");
						if (fieldInfo == null)
						{
							return false;
						}
						bool flag;
						try
						{
							flag = Convert.ToByte(fieldInfo.GetValue(c)) == map.LogicalIndex;
						}
						catch
						{
							flag = false;
						}
						return flag;
					});
					if (component == null)
					{
						base.Logger.LogError(string.Format("[TRIGGER VERIFY] '{0}' index={1}: ", map.Manifest.id, map.LogicalIndex) + "NO live IslandSpawner found.");
					}
					else
					{
						Transform parent = component.gameObject.transform.parent;
						Collider[] array2 = ((parent != null) ? parent.GetComponentsInChildren<Collider>(true) : null) ?? Array.Empty<Collider>();
						base.Logger.LogWarning(string.Format("[TRIGGER VERIFY] '{0}' index={1}: ", map.Manifest.id, map.LogicalIndex) + string.Format("spawner='{0}' active={1} ", component.gameObject.name, component.gameObject.activeInHierarchy) + string.Format("pos={0} colliders={1}.", component.transform.position, array2.Length));
					}
				}
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00005194 File Offset: 0x00003394
		private bool IsGameplayWorldReady()
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene scene = SceneManager.GetSceneAt(i);
				if (scene.IsValid() && scene.isLoaded)
				{
					if (string.Equals(scene.name, "Game", StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
					}
					else if (scene.name.StartsWith("Island", StringComparison.OrdinalIgnoreCase) || string.Equals(scene.name, "DevIsland", StringComparison.OrdinalIgnoreCase) || this._maps.Any<Plugin.RegisteredMap>((Plugin.RegisteredMap m) => !string.IsNullOrWhiteSpace(m.SceneName) && string.Equals(m.SceneName, scene.name, StringComparison.OrdinalIgnoreCase)))
					{
						flag2 = true;
					}
				}
			}
			if (!flag || !flag2)
			{
				return false;
			}
			object obj = Plugin.FindLiveObjectByTypeName("IslandManager");
			if (obj == null)
			{
				return false;
			}
			FieldInfo fieldInfo = Plugin.FindField(obj.GetType(), "_islandInfos");
			Array infos = ((fieldInfo != null) ? fieldInfo.GetValue(obj) : null) as Array;
			return infos != null && this._mapsByIndex.Keys.All<byte>((byte index) => (int)index < infos.Length);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000052CC File Offset: 0x000034CC
		private void UpdateRadar()
		{
			if (this._mapsByIndex.Count == 0)
			{
				return;
			}
			if (this._radarUi == null || this._maps.Any<Plugin.RegisteredMap>((Plugin.RegisteredMap m) => this._mapsByIndex.ContainsKey(m.LogicalIndex) && m.MapDot == null))
			{
				this.EnsureRadarDots();
			}
			if (this._radarUi == null)
			{
				return;
			}
			if (!this.IsNativeRadarCurrentlyVisible())
			{
				foreach (Plugin.RegisteredMap registeredMap in this._maps)
				{
					this.SetMapDotVisualActive(registeredMap.MapDot, false);
				}
				return;
			}
			foreach (Plugin.RegisteredMap registeredMap2 in this._maps)
			{
				if (registeredMap2.MapDot != null)
				{
					try
					{
						if (this._radarUpdateDotMethod == null)
						{
							Type type = registeredMap2.MapDot.GetType();
							this._radarUpdateDotMethod = this._radarUi.GetType().GetMethod("UpdateDot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]
							{
								type,
								typeof(Vector3)
							}, null);
						}
						MethodInfo radarUpdateDotMethod = this._radarUpdateDotMethod;
						if (radarUpdateDotMethod != null)
						{
							radarUpdateDotMethod.Invoke(this._radarUi, new object[] { registeredMap2.MapDot, registeredMap2.Position });
						}
						this.SetMapDotVisualActive(registeredMap2.MapDot, true);
						this.ApplyMapDotColor(registeredMap2);
					}
					catch
					{
						this.ResetRadarReferences();
						break;
					}
				}
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x0000546C File Offset: 0x0000366C
		private void EnsureRadarDots()
		{
			if (Time.unscaledTime < this._nextRadarAcquire)
			{
				return;
			}
			this._nextRadarAcquire = Time.unscaledTime + 1f;
			object obj = Plugin.FindLiveObjectByTypeName("RadarUI");
			if (obj == null)
			{
				return;
			}
			FieldInfo fieldInfo = Plugin.FindField(obj.GetType(), "_islandDots");
			Array array = ((fieldInfo != null) ? fieldInfo.GetValue(obj) : null) as Array;
			if (array == null)
			{
				return;
			}
			Type elementType = array.GetType().GetElementType();
			if (elementType == null)
			{
				return;
			}
			int num = (from m in this._maps
				where this._mapsByIndex.ContainsKey(m.LogicalIndex)
				select (int)(m.LogicalIndex + 1)).DefaultIfEmpty(array.Length).Max();
			Array array2 = Array.CreateInstance(elementType, Math.Max(array.Length, num));
			Array.Copy(array, array2, array.Length);
			object obj2 = null;
			for (int i = 0; i < array.Length; i++)
			{
				object value = array.GetValue(i);
				if (value != null)
				{
					obj2 = value;
					break;
				}
			}
			if (obj2 == null)
			{
				return;
			}
			foreach (Plugin.RegisteredMap registeredMap in this._maps)
			{
				int logicalIndex = (int)registeredMap.LogicalIndex;
				object obj3 = ((logicalIndex < array.Length) ? array.GetValue(logicalIndex) : null);
				if (obj3 != null && registeredMap.MapDot == null && this.IsOurMapDot(obj3, registeredMap.Manifest.id))
				{
					registeredMap.MapDot = obj3;
				}
				if (registeredMap.MapDot == null)
				{
					registeredMap.MapDot = this.CloneMapDot(obj2, "HTFML_Radar_" + registeredMap.Manifest.id);
				}
				if (registeredMap.MapDot != null)
				{
					array2.SetValue(registeredMap.MapDot, logicalIndex);
					this.ApplyMapDotColor(registeredMap);
				}
			}
			fieldInfo.SetValue(obj, array2);
			this._radarUi = obj;
			this._radarUpdateDotMethod = null;
			base.Logger.LogWarning(string.Format("[RADAR] MapDot array {0} -> {1}. ", array.Length, array2.Length) + "Custom=" + string.Join(", ", this._mapsByIndex.Values.Select<Plugin.RegisteredMap, string>((Plugin.RegisteredMap m) => string.Format("{0}@{1}:{2}", m.Manifest.id, m.LogicalIndex, m.Position))));
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000056F0 File Offset: 0x000038F0
		private bool IsOurMapDot(object mapDot, string mapId)
		{
			if (mapDot == null)
			{
				return false;
			}
			bool flag;
			try
			{
				FieldInfo fieldInfo = Plugin.FindField(mapDot.GetType(), "_dot");
				Component component = ((fieldInfo != null) ? fieldInfo.GetValue(mapDot) : null) as Component;
				flag = component != null && component.gameObject != null && string.Equals(component.gameObject.name, "HTFML_Radar_" + mapId, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00005778 File Offset: 0x00003978
		private object CloneMapDot(object template, string name)
		{
			MethodInfo method = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
			object obj = ((method != null) ? method.Invoke(template, null) : null);
			if (obj == null)
			{
				return null;
			}
			FieldInfo fieldInfo = Plugin.FindField(obj.GetType(), "_dot");
			Component component = ((fieldInfo != null) ? fieldInfo.GetValue(template) : null) as Component;
			if (component == null || component.transform.parent == null)
			{
				return null;
			}
			GameObject gameObject = Object.Instantiate<GameObject>(component.gameObject);
			gameObject.name = name;
			gameObject.transform.SetParent(component.transform.parent, false);
			gameObject.transform.localPosition = component.transform.localPosition;
			gameObject.transform.localRotation = component.transform.localRotation;
			gameObject.transform.localScale = component.transform.localScale;
			Component component2 = gameObject.GetComponent(fieldInfo.FieldType);
			if (component2 == null)
			{
				Object.Destroy(gameObject);
				return null;
			}
			fieldInfo.SetValue(obj, component2);
			gameObject.SetActive(component.gameObject.activeInHierarchy);
			return obj;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00005898 File Offset: 0x00003A98
		private bool IsNativeRadarCurrentlyVisible()
		{
			if (this._radarUi == null)
			{
				return false;
			}
			FieldInfo fieldInfo = Plugin.FindField(this._radarUi.GetType(), "_islandDots");
			Array array = ((fieldInfo != null) ? fieldInfo.GetValue(this._radarUi) : null) as Array;
			if (array == null)
			{
				return false;
			}
			int num = Math.Min(5, array.Length);
			for (int i = 0; i < num; i++)
			{
				object value = array.GetValue(i);
				FieldInfo fieldInfo2 = ((value != null) ? Plugin.FindField(value.GetType(), "_dot") : null);
				Component component = ((fieldInfo2 != null) ? fieldInfo2.GetValue(value) : null) as Component;
				if (component != null && component.gameObject.activeInHierarchy)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00005948 File Offset: 0x00003B48
		private void ApplyMapDotColor(Plugin.RegisteredMap map)
		{
			if (map == null || map.MapDot == null)
			{
				return;
			}
			FieldInfo fieldInfo = Plugin.FindField(map.MapDot.GetType(), "_dot");
			Component component = ((fieldInfo != null) ? fieldInfo.GetValue(map.MapDot) : null) as Component;
			PropertyInfo propertyInfo = ((component != null) ? component.GetType().GetProperty("color", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : null);
			if (propertyInfo != null && propertyInfo.CanWrite && propertyInfo.PropertyType == typeof(Color))
			{
				try
				{
					propertyInfo.SetValue(component, map.RadarColor, null);
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000059F8 File Offset: 0x00003BF8
		private void SetMapDotVisualActive(object mapDot, bool active)
		{
			if (mapDot == null)
			{
				return;
			}
			FieldInfo fieldInfo = Plugin.FindField(mapDot.GetType(), "_dot");
			Component component = ((fieldInfo != null) ? fieldInfo.GetValue(mapDot) : null) as Component;
			if (component != null && component.gameObject.activeSelf != active)
			{
				component.gameObject.SetActive(active);
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00005A50 File Offset: 0x00003C50
		private void ResetRadarReferences()
		{
			this._radarUi = null;
			this._radarUpdateDotMethod = null;
			this._nextRadarAcquire = Time.unscaledTime + 0.75f;
			foreach (Plugin.RegisteredMap registeredMap in this._maps)
			{
				registeredMap.MapDot = null;
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00005AC0 File Offset: 0x00003CC0
		private void CacheNativeGroundCollision()
		{
			Collider collider = null;
			float num = -1f;
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene sceneAt = SceneManager.GetSceneAt(i);
				if (sceneAt.IsValid() && sceneAt.isLoaded && (sceneAt.name.StartsWith("Island", StringComparison.OrdinalIgnoreCase) || string.Equals(sceneAt.name, "DevIsland", StringComparison.OrdinalIgnoreCase)))
				{
					GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
					for (int j = 0; j < rootGameObjects.Length; j++)
					{
						foreach (Collider collider2 in rootGameObjects[j].GetComponentsInChildren<Collider>(true))
						{
							if (!(collider2 == null) && collider2.enabled && !collider2.isTrigger)
							{
								Vector3 size = collider2.bounds.size;
								float num2 = size.x * size.z;
								if (num2 > num)
								{
									num = num2;
									collider = collider2;
								}
							}
						}
					}
				}
			}
			if (collider == null)
			{
				return;
			}
			this._nativeGroundLayer = collider.gameObject.layer;
			this._nativeGroundMaterial = collider.sharedMaterial;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00005BEC File Offset: 0x00003DEC
		private void AdaptLevelPhysics(Transform holder)
		{
			if (holder == null)
			{
				return;
			}
			int num = LayerMask.NameToLayer("Level");
			if (num < 0)
			{
				num = ((this._nativeGroundLayer >= 0) ? this._nativeGroundLayer : 8);
			}
			Collider[] componentsInChildren = holder.GetComponentsInChildren<Collider>(true);
			int num2 = 0;
			foreach (Collider collider in componentsInChildren)
			{
				if (!(collider == null) && !collider.isTrigger)
				{
					Rigidbody component = collider.gameObject.GetComponent<Rigidbody>();
					if (!(component != null) || component.isKinematic)
					{
						collider.gameObject.layer = num;
						this.TrySetLevelTag(collider.gameObject);
						collider.enabled = true;
						if (this._nativeGroundMaterial != null && collider.sharedMaterial == null)
						{
							collider.sharedMaterial = this._nativeGroundMaterial;
						}
						num2++;
					}
				}
			}
			if (this._verbose.Value)
			{
				base.Logger.LogInfo(string.Format("[PHYSICS] Adapted {0} static collider(s) to Level.", num2));
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00005CF8 File Offset: 0x00003EF8
		private bool TrySetLevelTag(GameObject go)
		{
			bool flag;
			try
			{
				if (!go.CompareTag("Level"))
				{
					go.tag = "Level";
				}
				flag = true;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00005D38 File Offset: 0x00003F38
		private object FindBoatPrefabInMemory()
		{
			Type type = AccessTools.TypeByName("Boat");
			if (type == null)
			{
				return null;
			}
			Object[] array = Resources.FindObjectsOfTypeAll(type);
			foreach (Object @object in array)
			{
				Component component = @object as Component;
				if (!(component == null))
				{
					Scene scene = component.gameObject.scene;
					if (!scene.IsValid() || scene.buildIndex < 0)
					{
						return @object;
					}
				}
			}
			return array.FirstOrDefault<Object>((Object o) => o != null);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00005DD4 File Offset: 0x00003FD4
		private Object FindNamedComponentPrefab(Type componentType, string wantedName)
		{
			Object[] array = Resources.FindObjectsOfTypeAll(componentType);
			Object @object = null;
			Object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Component component = array2[i] as Component;
				if (!(component == null) && (component.gameObject.name ?? "").IndexOf(wantedName, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					Scene scene = component.gameObject.scene;
					if (!scene.IsValid() || scene.buildIndex < 0)
					{
						return component;
					}
					if (@object == null)
					{
						@object = component;
					}
				}
			}
			return @object;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00005E58 File Offset: 0x00004058
		private GameObject FindIslandHolder(Scene scene)
		{
			if (!scene.IsValid() || !scene.isLoaded)
			{
				return null;
			}
			return scene.GetRootGameObjects().FirstOrDefault<GameObject>((GameObject r) => r != null && string.Equals(r.name, "IslandHolder", StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00005EA4 File Offset: 0x000040A4
		private Transform GetOrCreateChild(Transform parent, string name)
		{
			Transform transform = Plugin.FindChildRecursive(parent, name);
			if (transform != null)
			{
				return transform;
			}
			GameObject gameObject = new GameObject(name);
			gameObject.transform.SetParent(parent, false);
			return gameObject.transform;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00005EDC File Offset: 0x000040DC
		private static object FindLiveObjectByTypeName(string typeName)
		{
			Type type = AccessTools.TypeByName(typeName);
			if (type == null)
			{
				return null;
			}
			Object[] array = Resources.FindObjectsOfTypeAll(type);
			Object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Component component = array2[i] as Component;
				if (!(component == null))
				{
					Scene scene = component.gameObject.scene;
					if (scene.IsValid() && scene.isLoaded)
					{
						return component;
					}
				}
			}
			return array.FirstOrDefault<Object>((Object o) => o != null);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00005F70 File Offset: 0x00004170
		private static FieldInfo FindField(Type type, string name)
		{
			Type type2 = type;
			while (type2 != null)
			{
				FieldInfo field = type2.GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					return field;
				}
				type2 = type2.BaseType;
			}
			return null;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00005FA8 File Offset: 0x000041A8
		private static Transform FindChildRecursive(Transform root, string name)
		{
			if (root == null)
			{
				return null;
			}
			if (string.Equals(root.name, name, StringComparison.Ordinal))
			{
				return root;
			}
			for (int i = 0; i < root.childCount; i++)
			{
				Transform transform = Plugin.FindChildRecursive(root.GetChild(i), name);
				if (transform != null)
				{
					return transform;
				}
			}
			return null;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00005FFC File Offset: 0x000041FC
		private static bool SetFieldIfExists(object target, string fieldName, object value)
		{
			if (target == null)
			{
				return false;
			}
			FieldInfo fieldInfo = Plugin.FindField(target.GetType(), fieldName);
			if (fieldInfo == null)
			{
				return false;
			}
			bool flag;
			try
			{
				object obj = value;
				if (value != null && !fieldInfo.FieldType.IsInstanceOfType(value))
				{
					if (fieldInfo.FieldType.IsEnum)
					{
						obj = Enum.ToObject(fieldInfo.FieldType, value);
					}
					else
					{
						obj = Convert.ChangeType(value, fieldInfo.FieldType);
					}
				}
				fieldInfo.SetValue(target, obj);
				flag = true;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x04000002 RID: 2
		public const string Guid = "com.howtofish.maploader";

		// Token: 0x04000003 RID: 3
		public const string Name = "HTF Map Loader";

		// Token: 0x04000004 RID: 4
		public const string Version = "0.2.8";

		// Token: 0x04000005 RID: 5
		internal static Plugin Instance;

		// Token: 0x04000006 RID: 6
		internal static ManualLogSource Log;

		// Token: 0x04000007 RID: 7
		private Harmony _harmony;

		// Token: 0x04000008 RID: 8
		private ConfigEntry<bool> _enabled;

		// Token: 0x04000009 RID: 9
		private ConfigEntry<bool> _verbose;

		// Token: 0x0400000A RID: 10
		private ConfigEntry<bool> _repairRuntime;

		// Token: 0x0400000B RID: 11
		private ConfigEntry<bool> _enableMapWildlife;

		// Token: 0x0400000C RID: 12
		private readonly List<Plugin.RegisteredMap> _maps = new List<Plugin.RegisteredMap>();

		// Token: 0x0400000D RID: 13
		private readonly Dictionary<byte, Plugin.RegisteredMap> _mapsByIndex = new Dictionary<byte, Plugin.RegisteredMap>();

		// Token: 0x0400000E RID: 14
		private bool _scanned;

		// Token: 0x0400000F RID: 15
		private bool _registered;

		// Token: 0x04000010 RID: 16
		private bool _loggedDisabled;

		// Token: 0x04000011 RID: 17
		private bool _verifiedRegisteredTriggers;

		// Token: 0x04000012 RID: 18
		private float _managerFirstSeenTime = -1f;

		// Token: 0x04000013 RID: 19
		private bool _bypassQueuePatch;

		// Token: 0x04000014 RID: 20
		private bool _transitioning;

		// Token: 0x04000015 RID: 21
		private Plugin.RegisteredMap _loadingMap;

		// Token: 0x04000016 RID: 22
		private Plugin.RegisteredMap _currentMap;

		// Token: 0x04000017 RID: 23
		private object _radarUi;

		// Token: 0x04000018 RID: 24
		private MethodInfo _radarUpdateDotMethod;

		// Token: 0x04000019 RID: 25
		private float _nextRadarAcquire;

		// Token: 0x0400001A RID: 26
		private int _nativeGroundLayer = -1;

		// Token: 0x0400001B RID: 27
		private PhysicsMaterial _nativeGroundMaterial;

		// Token: 0x02000005 RID: 5
		[Serializable]
		public sealed class MapManifest
		{
			// Token: 0x0400001C RID: 28
			public string id;

			// Token: 0x0400001D RID: 29
			public string name;

			// Token: 0x0400001E RID: 30
			public string author;

			// Token: 0x0400001F RID: 31
			public string bundle;

			// Token: 0x04000020 RID: 32
			public string scene;

			// Token: 0x04000021 RID: 33
			public string positionMode;

			// Token: 0x04000022 RID: 34
			public Plugin.SerializableVector3 position = new Plugin.SerializableVector3();

			// Token: 0x04000023 RID: 35
			public bool moveScene = true;

			// Token: 0x04000024 RID: 36
			public string radarColor = "";

			// Token: 0x04000025 RID: 37
			public float islandSize = 55f;

			// Token: 0x04000026 RID: 38
			public Plugin.RuntimeManifest runtime = new Plugin.RuntimeManifest();

			// Token: 0x04000027 RID: 39
			public Plugin.WildlifeManifest wildlife = new Plugin.WildlifeManifest();
		}

		// Token: 0x02000006 RID: 6
		[Serializable]
		public sealed class SerializableVector3
		{
			// Token: 0x0600004A RID: 74 RVA: 0x0000613D File Offset: 0x0000433D
			public Vector3 ToVector3()
			{
				return new Vector3(this.x, this.y, this.z);
			}

			// Token: 0x04000028 RID: 40
			public float x;

			// Token: 0x04000029 RID: 41
			public float y;

			// Token: 0x0400002A RID: 42
			public float z;
		}

		// Token: 0x02000007 RID: 7
		[Serializable]
		public sealed class RuntimeManifest
		{
			// Token: 0x0400002B RID: 43
			public string playerSpawn = "PlayerSpawnPoint";

			// Token: 0x0400002C RID: 44
			public string boatSpawn = "BoatSpawnPoint";

			// Token: 0x0400002D RID: 45
			public Plugin.SerializableVector3 defaultPlayerSpawn = new Plugin.SerializableVector3
			{
				x = 0f,
				y = 6f,
				z = 0f
			};

			// Token: 0x0400002E RID: 46
			public Plugin.SerializableVector3 defaultBoatSpawn = new Plugin.SerializableVector3
			{
				x = 0f,
				y = 0.5f,
				z = -25f
			};
		}

		// Token: 0x02000008 RID: 8
		[Serializable]
		public sealed class WildlifeManifest
		{
			// Token: 0x0400002F RID: 47
			public bool seagulls;

			// Token: 0x04000030 RID: 48
			public bool clams;

			// Token: 0x04000031 RID: 49
			public int seagullMax = 3;

			// Token: 0x04000032 RID: 50
			public float seagullDelay = 60f;

			// Token: 0x04000033 RID: 51
			public int clamMax = 3;

			// Token: 0x04000034 RID: 52
			public float clamDelay = 15f;
		}

		// Token: 0x02000009 RID: 9
		private sealed class RegisteredMap
		{
			// Token: 0x0600004E RID: 78 RVA: 0x00006210 File Offset: 0x00004410
			public override string ToString()
			{
				Plugin.MapManifest manifest = this.Manifest;
				string text;
				if ((text = ((manifest != null) ? manifest.name : null)) == null)
				{
					Plugin.MapManifest manifest2 = this.Manifest;
					text = ((manifest2 != null) ? manifest2.id : null) ?? "Map";
				}
				return text + " " + string.Format("index={0} scene='{1}' pos={2}", this.LogicalIndex, this.SceneName, this.Position);
			}

			// Token: 0x04000035 RID: 53
			public Plugin.MapManifest Manifest;

			// Token: 0x04000036 RID: 54
			public string Folder;

			// Token: 0x04000037 RID: 55
			public string BundlePath;

			// Token: 0x04000038 RID: 56
			public string SceneName;

			// Token: 0x04000039 RID: 57
			public string ScenePath;

			// Token: 0x0400003A RID: 58
			public Vector3 Position;

			// Token: 0x0400003B RID: 59
			public Color RadarColor;

			// Token: 0x0400003C RID: 60
			public byte LogicalIndex;

			// Token: 0x0400003D RID: 61
			public AssetBundle Bundle;

			// Token: 0x0400003E RID: 62
			public object MapDot;
		}
	}
}
