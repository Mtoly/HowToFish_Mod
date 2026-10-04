using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HTFMapLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HTFMapLoaderFix
{
	// Token: 0x02000004 RID: 4
	[BepInPlugin("com.lemon.htfmaploader.fix", "HTFMapLoader Performance Fix", "1.0.4")]
	[BepInDependency("com.howtofish.maploader", 1)]
	public sealed class HTFMapLoaderFixPlugin : BaseUnityPlugin
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002068 File Offset: 0x00000268
		private void Awake()
		{
			HTFMapLoaderFixPlugin._log = base.Logger;
			HTFMapLoaderFixPlugin._harmony = new Harmony("com.lemon.htfmaploader.fix");
			this.ApplyPatches();
			UnityAction<Scene> unityAction;
			if ((unityAction = HTFMapLoaderFixPlugin.<>O.<0>__OnSceneUnloaded) == null)
			{
				unityAction = (HTFMapLoaderFixPlugin.<>O.<0>__OnSceneUnloaded = new UnityAction<Scene>(HTFMapLoaderFixPlugin.OnSceneUnloaded));
			}
			SceneManager.sceneUnloaded += unityAction;
			base.Logger.LogInfo("[HTFMapLoader Performance Fix] v1.0.4 已加载。");
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020C5 File Offset: 0x000002C5
		private void OnDestroy()
		{
			UnityAction<Scene> unityAction;
			if ((unityAction = HTFMapLoaderFixPlugin.<>O.<0>__OnSceneUnloaded) == null)
			{
				unityAction = (HTFMapLoaderFixPlugin.<>O.<0>__OnSceneUnloaded = new UnityAction<Scene>(HTFMapLoaderFixPlugin.OnSceneUnloaded));
			}
			SceneManager.sceneUnloaded -= unityAction;
			Harmony harmony = HTFMapLoaderFixPlugin._harmony;
			if (harmony == null)
			{
				return;
			}
			harmony.UnpatchSelf();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000020F6 File Offset: 0x000002F6
		private static void OnSceneUnloaded(Scene scene)
		{
			HTFMapLoaderFixPlugin._liveObjectCache.Clear();
			HTFMapLoaderFixPlugin._worldReadyCachedTime = -999f;
			HTFMapLoaderFixPlugin._cachedMapInfos = null;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002114 File Offset: 0x00000314
		private void ApplyPatches()
		{
			Type typeFromHandle = typeof(Plugin);
			int num = 0;
			num += this.TryPatch(typeFromHandle, "FindLiveObjectByTypeName", BindingFlags.Static | BindingFlags.NonPublic, "FindLiveObject_Prefix", null);
			num += this.TryPatch(typeFromHandle, "FindField", BindingFlags.Static | BindingFlags.NonPublic, "FindField_Prefix", null);
			num += this.TryPatch(typeFromHandle, "IsGameplayWorldReady", BindingFlags.Instance | BindingFlags.NonPublic, "WorldReady_Prefix", "WorldReady_Postfix");
			num += this.TryPatch(typeFromHandle, "UpdateRadar", BindingFlags.Instance | BindingFlags.NonPublic, "UpdateRadar_Prefix", null);
			num += this.TryPatch(typeof(RadarUI), "UpdateAllDots", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, "RadarUI_UpdateAllDots_Postfix");
			base.Logger.LogInfo(string.Format("[{0}] 成功注入 {1}/5 个核心补丁。", "HTFMapLoader Performance Fix", num));
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000021D0 File Offset: 0x000003D0
		private int TryPatch(Type targetType, string methodName, BindingFlags flags, string prefixName, string postfixName)
		{
			int num;
			try
			{
				MethodInfo method = targetType.GetMethod(methodName, flags);
				if (method == null)
				{
					base.Logger.LogWarning("[PATCH] 未找到 " + targetType.Name + "." + methodName);
					num = 0;
				}
				else
				{
					HarmonyMethod harmonyMethod = ((prefixName != null) ? new HarmonyMethod(typeof(HTFMapLoaderFixPlugin), prefixName, null) : null);
					HarmonyMethod harmonyMethod2 = ((postfixName != null) ? new HarmonyMethod(typeof(HTFMapLoaderFixPlugin), postfixName, null) : null);
					HTFMapLoaderFixPlugin._harmony.Patch(method, harmonyMethod, harmonyMethod2, null, null, null);
					num = 1;
				}
			}
			catch (Exception ex)
			{
				base.Logger.LogError(string.Concat(new string[] { "[PATCH] ", targetType.Name, ".", methodName, " 失败: ", ex.Message }));
				num = 0;
			}
			return num;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000022BC File Offset: 0x000004BC
		private static bool FindLiveObject_Prefix(string typeName, ref object __result)
		{
			float unscaledTime = Time.unscaledTime;
			HTFMapLoaderFixPlugin.LiveObjectEntry liveObjectEntry;
			if (HTFMapLoaderFixPlugin._liveObjectCache.TryGetValue(typeName, out liveObjectEntry) && unscaledTime - liveObjectEntry.CachedTime < 2f && HTFMapLoaderFixPlugin.IsUnityAlive(liveObjectEntry.Instance))
			{
				__result = liveObjectEntry.Instance;
				return false;
			}
			foreach (MonoBehaviour monoBehaviour in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
			{
				if (monoBehaviour != null && monoBehaviour.gameObject.scene.isLoaded && monoBehaviour.GetType().Name == typeName)
				{
					HTFMapLoaderFixPlugin._liveObjectCache[typeName] = new HTFMapLoaderFixPlugin.LiveObjectEntry
					{
						Instance = monoBehaviour,
						CachedTime = unscaledTime
					};
					__result = monoBehaviour;
					return false;
				}
			}
			foreach (ScriptableObject scriptableObject in Resources.FindObjectsOfTypeAll<ScriptableObject>())
			{
				if (scriptableObject != null && scriptableObject.GetType().Name == typeName)
				{
					HTFMapLoaderFixPlugin._liveObjectCache[typeName] = new HTFMapLoaderFixPlugin.LiveObjectEntry
					{
						Instance = scriptableObject,
						CachedTime = unscaledTime
					};
					__result = scriptableObject;
					return false;
				}
			}
			__result = null;
			return false;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000023F0 File Offset: 0x000005F0
		private static bool IsUnityAlive(object obj)
		{
			Object @object = obj as Object;
			if (@object != null)
			{
				return @object != null;
			}
			return obj != null;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002414 File Offset: 0x00000614
		private static bool FindField_Prefix(Type type, string name, ref FieldInfo __result)
		{
			if (type == null)
			{
				__result = null;
				return false;
			}
			long num = ((long)type.MetadataToken << 32) | (long)((ulong)((name != null) ? name.GetHashCode() : 0));
			FieldInfo fieldInfo;
			if (HTFMapLoaderFixPlugin._fieldCache.TryGetValue(num, out fieldInfo))
			{
				__result = fieldInfo;
				return false;
			}
			Type type2 = type;
			while (type2 != null)
			{
				FieldInfo field = type2.GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					HTFMapLoaderFixPlugin._fieldCache[num] = field;
					__result = field;
					return false;
				}
				type2 = type2.BaseType;
			}
			HTFMapLoaderFixPlugin._fieldCache[num] = null;
			__result = null;
			return false;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000024A4 File Offset: 0x000006A4
		private static bool WorldReady_Prefix(ref bool __result)
		{
			if (Time.unscaledTime - HTFMapLoaderFixPlugin._worldReadyCachedTime < 0.5f)
			{
				__result = HTFMapLoaderFixPlugin._worldReadyCachedResult;
				return false;
			}
			return true;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000024C2 File Offset: 0x000006C2
		private static void WorldReady_Postfix(bool __result)
		{
			HTFMapLoaderFixPlugin._worldReadyCachedResult = __result;
			HTFMapLoaderFixPlugin._worldReadyCachedTime = Time.unscaledTime;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000024D4 File Offset: 0x000006D4
		private static bool UpdateRadar_Prefix()
		{
			return false;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000024D8 File Offset: 0x000006D8
		private static HTFMapLoaderFixPlugin.CustomMapInfo[] GetCustomMaps()
		{
			float unscaledTime = Time.unscaledTime;
			if (HTFMapLoaderFixPlugin._cachedMapInfos != null && unscaledTime - HTFMapLoaderFixPlugin._lastMapExtractTime < 2f)
			{
				return HTFMapLoaderFixPlugin._cachedMapInfos;
			}
			HTFMapLoaderFixPlugin._lastMapExtractTime = unscaledTime;
			FieldInfo instanceField = HTFMapLoaderFixPlugin._instanceField;
			object obj = ((instanceField != null) ? instanceField.GetValue(null) : null) ?? Object.FindObjectOfType<Plugin>();
			if (obj == null || HTFMapLoaderFixPlugin._mapsField == null)
			{
				return HTFMapLoaderFixPlugin._cachedMapInfos;
			}
			IEnumerable enumerable = HTFMapLoaderFixPlugin._mapsField.GetValue(obj) as IEnumerable;
			if (enumerable == null)
			{
				return HTFMapLoaderFixPlugin._cachedMapInfos;
			}
			List<HTFMapLoaderFixPlugin.CustomMapInfo> list = new List<HTFMapLoaderFixPlugin.CustomMapInfo>();
			foreach (object obj2 in enumerable)
			{
				if (obj2 != null)
				{
					Type type = obj2.GetType();
					if (HTFMapLoaderFixPlugin._positionField == null)
					{
						HTFMapLoaderFixPlugin._positionField = AccessTools.Field(type, "Position") ?? type.GetField("Position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
					if (HTFMapLoaderFixPlugin._radarColorField == null)
					{
						HTFMapLoaderFixPlugin._radarColorField = AccessTools.Field(type, "RadarColor") ?? type.GetField("RadarColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
					if (HTFMapLoaderFixPlugin._logicalIndexField == null)
					{
						HTFMapLoaderFixPlugin._logicalIndexField = AccessTools.Field(type, "LogicalIndex") ?? type.GetField("LogicalIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
					if (HTFMapLoaderFixPlugin._manifestField == null)
					{
						HTFMapLoaderFixPlugin._manifestField = AccessTools.Field(type, "Manifest") ?? type.GetField("Manifest", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
					Vector3 vector = Vector3.zero;
					Color red = Color.red;
					byte b = 0;
					string text = "custom_map";
					if (HTFMapLoaderFixPlugin._positionField != null)
					{
						object obj3 = HTFMapLoaderFixPlugin._positionField.GetValue(obj2);
						if (obj3 is Vector3)
						{
							Vector3 vector2 = (Vector3)obj3;
							vector = vector2;
						}
					}
					if (HTFMapLoaderFixPlugin._radarColorField != null)
					{
						object obj3 = HTFMapLoaderFixPlugin._radarColorField.GetValue(obj2);
						if (obj3 is Color)
						{
							Color color = (Color)obj3;
							if (color.a > 0.01f)
							{
								red..ctor(color.r, color.g, color.b, 1f);
							}
						}
					}
					if (HTFMapLoaderFixPlugin._logicalIndexField != null)
					{
						object obj3 = HTFMapLoaderFixPlugin._logicalIndexField.GetValue(obj2);
						if (obj3 is byte)
						{
							byte b2 = (byte)obj3;
							b = b2;
						}
					}
					if (HTFMapLoaderFixPlugin._manifestField != null)
					{
						object value = HTFMapLoaderFixPlugin._manifestField.GetValue(obj2);
						if (value != null)
						{
							FieldInfo field = value.GetType().GetField("id");
							string text2 = ((field != null) ? field.GetValue(value) : null) as string;
							if (text2 != null && !string.IsNullOrEmpty(text2))
							{
								text = text2;
							}
						}
					}
					list.Add(new HTFMapLoaderFixPlugin.CustomMapInfo
					{
						Position = vector,
						Color = red,
						LogicalIndex = b,
						Id = text
					});
				}
			}
			if (list.Count > 0)
			{
				HTFMapLoaderFixPlugin._cachedMapInfos = list.ToArray();
				if (HTFMapLoaderFixPlugin._log != null)
				{
					foreach (HTFMapLoaderFixPlugin.CustomMapInfo customMapInfo in HTFMapLoaderFixPlugin._cachedMapInfos)
					{
						HTFMapLoaderFixPlugin._log.LogInfo(string.Format("[HTFMapLoaderFix] 已捕获自定义地图: {0} (Index={1}, Pos={2}, Color={3})", new object[] { customMapInfo.Id, customMapInfo.LogicalIndex, customMapInfo.Position, customMapInfo.Color }));
					}
				}
			}
			return HTFMapLoaderFixPlugin._cachedMapInfos;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002870 File Offset: 0x00000A70
		private static HTFMapLoaderFixPlugin.RadarState EnsureRadarDots(RadarUI radar, HTFMapLoaderFixPlugin.CustomMapInfo[] maps)
		{
			if (radar == null || maps == null || maps.Length == 0)
			{
				return null;
			}
			HTFMapLoaderFixPlugin.RadarState radarState;
			if (HTFMapLoaderFixPlugin._radarTable.TryGetValue(radar, out radarState) && ((radarState != null) ? radarState.Dots : null) != null && radarState.Dots.Length == maps.Length && radarState.Dots[0].Image != null)
			{
				return radarState;
			}
			FieldInfo islandDotsField = HTFMapLoaderFixPlugin._islandDotsField;
			MapDot[] array = ((islandDotsField != null) ? islandDotsField.GetValue(radar) : null) as MapDot[];
			if (array == null || array.Length == 0 || array[0] == null)
			{
				return null;
			}
			MapDot mapDot = array[0];
			FieldInfo mapDotImageField = HTFMapLoaderFixPlugin._mapDotImageField;
			Image image = ((mapDotImageField != null) ? mapDotImageField.GetValue(mapDot) : null) as Image;
			if (image == null || image.transform.parent == null)
			{
				return null;
			}
			if (HTFMapLoaderFixPlugin._leanTweenCancelMethod == null)
			{
				Type type = Type.GetType("LeanTween, Assembly-CSharp") ?? AccessTools.TypeByName("LeanTween");
				if (type != null)
				{
					HTFMapLoaderFixPlugin._leanTweenCancelMethod = type.GetMethod("cancel", new Type[] { typeof(GameObject) });
				}
			}
			HTFMapLoaderFixPlugin.RadarDotInstance[] array2 = new HTFMapLoaderFixPlugin.RadarDotInstance[maps.Length];
			for (int i = 0; i < maps.Length; i++)
			{
				HTFMapLoaderFixPlugin.CustomMapInfo customMapInfo = maps[i];
				MapDot mapDot2 = (MapDot)HTFMapLoaderFixPlugin._memberwiseCloneMethod.Invoke(mapDot, null);
				GameObject gameObject = Object.Instantiate<GameObject>(image.gameObject, image.transform.parent, false);
				gameObject.name = "HTFML_Radar_" + customMapInfo.Id;
				Image component = gameObject.GetComponent<Image>();
				HTFMapLoaderFixPlugin._mapDotImageField.SetValue(mapDot2, component);
				if (HTFMapLoaderFixPlugin._leanTweenCancelMethod != null)
				{
					HTFMapLoaderFixPlugin._leanTweenCancelMethod.Invoke(null, new object[] { gameObject });
				}
				component.color = customMapInfo.Color;
				gameObject.SetActive(true);
				array2[i] = new HTFMapLoaderFixPlugin.RadarDotInstance
				{
					Dot = mapDot2,
					Image = component,
					Position = customMapInfo.Position,
					Color = customMapInfo.Color,
					LogicalIndex = customMapInfo.LogicalIndex
				};
			}
			HTFMapLoaderFixPlugin.RadarState radarState2 = new HTFMapLoaderFixPlugin.RadarState
			{
				Dots = array2
			};
			HTFMapLoaderFixPlugin._radarTable.AddOrUpdate(radar, radarState2);
			return radarState2;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002AA4 File Offset: 0x00000CA4
		private static void RadarUI_UpdateAllDots_Postfix(RadarUI __instance)
		{
			if (__instance == null)
			{
				return;
			}
			HTFMapLoaderFixPlugin.CustomMapInfo[] customMaps = HTFMapLoaderFixPlugin.GetCustomMaps();
			if (customMaps == null || customMaps.Length == 0)
			{
				return;
			}
			HTFMapLoaderFixPlugin.RadarState radarState = HTFMapLoaderFixPlugin.EnsureRadarDots(__instance, customMaps);
			if (radarState == null || radarState.Dots == null)
			{
				return;
			}
			Vector3 vector = Vector3.zero;
			bool flag = false;
			if (Player.LocalPlayer != null && Player.LocalPlayer.Transform != null)
			{
				vector = Player.LocalPlayer.Transform.position;
				flag = true;
			}
			else if (Camera.main != null)
			{
				vector = Camera.main.transform.position;
				flag = true;
			}
			for (int i = 0; i < radarState.Dots.Length; i++)
			{
				HTFMapLoaderFixPlugin.RadarDotInstance radarDotInstance = radarState.Dots[i];
				if (radarDotInstance.Dot != null && !(radarDotInstance.Image == null))
				{
					MethodInfo moveDotMethod = HTFMapLoaderFixPlugin._moveDotMethod;
					if (moveDotMethod != null)
					{
						moveDotMethod.Invoke(__instance, new object[] { radarDotInstance.Dot, radarDotInstance.Position });
					}
					if (HTFMapLoaderFixPlugin._leanTweenCancelMethod != null)
					{
						HTFMapLoaderFixPlugin._leanTweenCancelMethod.Invoke(null, new object[] { radarDotInstance.Image.gameObject });
					}
					radarDotInstance.Image.rectTransform.localPosition = radarDotInstance.Dot.RealtimeLocalPos;
					radarDotInstance.Image.color = radarDotInstance.Color;
					if (!radarDotInstance.Image.gameObject.activeSelf)
					{
						radarDotInstance.Image.gameObject.SetActive(true);
					}
					if (flag)
					{
						float num = vector.x - radarDotInstance.Position.x;
						float num2 = vector.z - radarDotInstance.Position.z;
						if (num * num + num2 * num2 <= 122500f)
						{
							HTFMapLoaderFixPlugin.TryAutoTriggerCustomIslandLoad(radarDotInstance.LogicalIndex);
						}
					}
				}
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002C84 File Offset: 0x00000E84
		private static void TryAutoTriggerCustomIslandLoad(byte logicalIndex)
		{
			if (Time.unscaledTime - HTFMapLoaderFixPlugin._lastTriggerAttemptTime < 2.5f)
			{
				return;
			}
			try
			{
				IslandManager islandManager = Object.FindObjectOfType<IslandManager>();
				if (islandManager != null)
				{
					FieldInfo fieldInfo = AccessTools.Field(typeof(IslandManager), "_curIsland");
					if (fieldInfo != null)
					{
						object value = fieldInfo.GetValue(islandManager);
						if (value is byte)
						{
							byte b = (byte)value;
							if (b == logicalIndex)
							{
								return;
							}
						}
					}
					MethodInfo method = typeof(IslandManager).GetMethod("QueueRequest", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null)
					{
						HTFMapLoaderFixPlugin._lastTriggerAttemptTime = Time.unscaledTime;
						method.Invoke(islandManager, new object[] { logicalIndex });
						ManualLogSource log = HTFMapLoaderFixPlugin._log;
						if (log != null)
						{
							log.LogInfo(string.Format("[HTFMapLoaderFix] 玩家航行到达海岛坐标区域，触发加载 QueueRequest({0})。", logicalIndex));
						}
					}
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = HTFMapLoaderFixPlugin._log;
				if (log2 != null)
				{
					log2.LogWarning("[HTFMapLoaderFix] 自动加载海岛异常: " + ex.Message);
				}
			}
		}

		// Token: 0x04000002 RID: 2
		private const string PluginGuid = "com.lemon.htfmaploader.fix";

		// Token: 0x04000003 RID: 3
		private const string PluginName = "HTFMapLoader Performance Fix";

		// Token: 0x04000004 RID: 4
		private const string PluginVersion = "1.0.4";

		// Token: 0x04000005 RID: 5
		private static ManualLogSource _log;

		// Token: 0x04000006 RID: 6
		private static Harmony _harmony;

		// Token: 0x04000007 RID: 7
		private static readonly Dictionary<string, HTFMapLoaderFixPlugin.LiveObjectEntry> _liveObjectCache = new Dictionary<string, HTFMapLoaderFixPlugin.LiveObjectEntry>();

		// Token: 0x04000008 RID: 8
		private const float LiveObjectTTL = 2f;

		// Token: 0x04000009 RID: 9
		private static readonly Dictionary<long, FieldInfo> _fieldCache = new Dictionary<long, FieldInfo>();

		// Token: 0x0400000A RID: 10
		private static float _worldReadyCachedTime = -999f;

		// Token: 0x0400000B RID: 11
		private static bool _worldReadyCachedResult;

		// Token: 0x0400000C RID: 12
		private const float WorldReadyTTL = 0.5f;

		// Token: 0x0400000D RID: 13
		private static readonly ConditionalWeakTable<RadarUI, HTFMapLoaderFixPlugin.RadarState> _radarTable = new ConditionalWeakTable<RadarUI, HTFMapLoaderFixPlugin.RadarState>();

		// Token: 0x0400000E RID: 14
		private static readonly FieldInfo _instanceField = AccessTools.Field(typeof(Plugin), "Instance");

		// Token: 0x0400000F RID: 15
		private static readonly FieldInfo _mapsField = AccessTools.Field(typeof(Plugin), "_maps");

		// Token: 0x04000010 RID: 16
		private static readonly FieldInfo _islandDotsField = AccessTools.Field(typeof(RadarUI), "_islandDots");

		// Token: 0x04000011 RID: 17
		private static readonly FieldInfo _mapDotImageField = AccessTools.Field(typeof(MapDot), "_dot");

		// Token: 0x04000012 RID: 18
		private static readonly MethodInfo _moveDotMethod = AccessTools.Method(typeof(RadarUI), "MoveDot", new Type[]
		{
			typeof(MapDot),
			typeof(Vector3)
		}, null);

		// Token: 0x04000013 RID: 19
		private static readonly MethodInfo _memberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

		// Token: 0x04000014 RID: 20
		private static MethodInfo _leanTweenCancelMethod;

		// Token: 0x04000015 RID: 21
		private static FieldInfo _positionField;

		// Token: 0x04000016 RID: 22
		private static FieldInfo _radarColorField;

		// Token: 0x04000017 RID: 23
		private static FieldInfo _logicalIndexField;

		// Token: 0x04000018 RID: 24
		private static FieldInfo _manifestField;

		// Token: 0x04000019 RID: 25
		private static HTFMapLoaderFixPlugin.CustomMapInfo[] _cachedMapInfos;

		// Token: 0x0400001A RID: 26
		private static float _lastMapExtractTime = -999f;

		// Token: 0x0400001B RID: 27
		private static float _lastTriggerAttemptTime;

		// Token: 0x02000005 RID: 5
		private struct LiveObjectEntry
		{
			// Token: 0x0400001C RID: 28
			public object Instance;

			// Token: 0x0400001D RID: 29
			public float CachedTime;
		}

		// Token: 0x02000006 RID: 6
		private sealed class CustomMapInfo
		{
			// Token: 0x0400001E RID: 30
			public Vector3 Position;

			// Token: 0x0400001F RID: 31
			public Color Color;

			// Token: 0x04000020 RID: 32
			public byte LogicalIndex;

			// Token: 0x04000021 RID: 33
			public string Id;
		}

		// Token: 0x02000007 RID: 7
		private sealed class RadarDotInstance
		{
			// Token: 0x04000022 RID: 34
			public MapDot Dot;

			// Token: 0x04000023 RID: 35
			public Image Image;

			// Token: 0x04000024 RID: 36
			public Vector3 Position;

			// Token: 0x04000025 RID: 37
			public Color Color;

			// Token: 0x04000026 RID: 38
			public byte LogicalIndex;
		}

		// Token: 0x02000008 RID: 8
		private sealed class RadarState
		{
			// Token: 0x04000027 RID: 39
			public HTFMapLoaderFixPlugin.RadarDotInstance[] Dots;
		}

		// Token: 0x02000009 RID: 9
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04000028 RID: 40
			public static UnityAction<Scene> <0>__OnSceneUnloaded;
		}
	}
}
