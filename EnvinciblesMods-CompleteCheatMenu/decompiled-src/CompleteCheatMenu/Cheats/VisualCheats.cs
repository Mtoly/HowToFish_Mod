using System;
using System.Collections.Generic;
using System.Reflection;
using CompleteCheatMenu.Game;
using CompleteCheatMenu.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace CompleteCheatMenu.Cheats;

internal static class VisualCheats
{
	internal enum EspKind
	{
		Creature,
		Item,
		Player
	}

	internal struct EspTarget
	{
		internal Transform Transform;

		internal string Label;

		internal EspKind Kind;
	}

	internal const float DefaultFov = 60f;

	private static Behaviour _playerCamComponent;

	private static Transform _camTransform;

	private static Vector3 _savedPos;

	private static Quaternion _savedRot;

	private static bool _freeCamActive;

	private static bool _bodyShown;

	private static readonly string[] BodyRenderers = new string[6] { "_bodyRenderer", "_leftHand", "_rightHand", "_outfitRenderer", "_hatRenderer", "_accessoryRenderer" };

	private static readonly string[] LocalHandRenderers = new string[2] { "_localLeftHand", "_localRightHand" };

	private static readonly List<EspTarget> _targets = new List<EspTarget>();

	private static int _espSnapshotRevision = -1;

	internal static bool FovAvailable => GameBinder.Method("PlayerCamera", "SetFOV", 1) != null;

	internal static bool DamageNumbers => ReadBool("DecalManager", "UseDamageNumbers", fallback: true);

	internal static bool Blood => ReadBool("DecalManager", "UseBlood", fallback: true);

	internal static bool FreeCamActive => _freeCamActive;

	internal static bool FreeCamAvailable
	{
		get
		{
			if (Refs.LocalCamera is Behaviour)
			{
				return GameBinder.Property("PlayerCamera", "CamTransform") != null;
			}
			return false;
		}
	}

	internal static bool BodyShown => _bodyShown;

	private static object LocalSkin
	{
		get
		{
			object localPlayer = Refs.LocalPlayer;
			if (localPlayer == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = GameBinder.Property("Player", "Skin");
			try
			{
				object obj = propertyInfo?.GetValue(localPlayer);
				return Refs.IsAlive(obj) ? obj : null;
			}
			catch
			{
				return null;
			}
		}
	}

	internal static bool BodyAvailable
	{
		get
		{
			if (LocalSkin != null)
			{
				return GameBinder.Method("PlayerSkin", "InitializeOther", 0) != null;
			}
			return false;
		}
	}

	internal static IReadOnlyList<EspTarget> Targets => _targets;

	internal static bool SetFov(float fov)
	{
		return GameBinder.TryInvoke(GameBinder.Method("PlayerCamera", "SetFOV", 1), null, fov);
	}

	private static bool ReadBool(string type, string prop, bool fallback)
	{
		PropertyInfo propertyInfo = GameBinder.Property(type, prop);
		if (propertyInfo == null)
		{
			return fallback;
		}
		try
		{
			return (bool)propertyInfo.GetValue(null);
		}
		catch
		{
			return fallback;
		}
	}

	internal static bool SetDamageNumbers(bool to)
	{
		return GameBinder.TryInvoke(GameBinder.Method("DecalManager", "ToggleDamageNumbers", 1), null, to);
	}

	internal static bool SetBlood(bool to)
	{
		return GameBinder.TryInvoke(GameBinder.Method("DecalManager", "ToggleBlood", 1), null, to);
	}

	internal static bool SetDecals(bool to)
	{
		return GameBinder.TryInvoke(GameBinder.Method("DecalManager", "ToggleDecals", 1), null, to);
	}

	internal static bool ClearDecals()
	{
		return GameBinder.TryInvoke(GameBinder.Method("DecalManager", "ClearDecals", 0), null);
	}

	internal static bool ToggleFreeCam(bool on)
	{
		if (on == _freeCamActive)
		{
			return true;
		}
		if (!on)
		{
			return ExitFreeCam();
		}
		return EnterFreeCam();
	}

	private static bool EnterFreeCam()
	{
		Behaviour behaviour = Refs.LocalCamera as Behaviour;
		if (behaviour == null)
		{
			return false;
		}
		PropertyInfo propertyInfo = GameBinder.Property("PlayerCamera", "CamTransform");
		try
		{
			_camTransform = propertyInfo?.GetValue(behaviour) as Transform;
		}
		catch
		{
			_camTransform = null;
		}
		if (_camTransform == null)
		{
			return false;
		}
		_playerCamComponent = behaviour;
		_savedPos = _camTransform.position;
		_savedRot = _camTransform.rotation;
		behaviour.enabled = false;
		_freeCamActive = true;
		return true;
	}

	private static bool ExitFreeCam()
	{
		_freeCamActive = false;
		if (_camTransform != null)
		{
			_camTransform.position = _savedPos;
			_camTransform.rotation = _savedRot;
		}
		if (_playerCamComponent != null)
		{
			_playerCamComponent.enabled = true;
		}
		_playerCamComponent = null;
		_camTransform = null;
		return true;
	}

	internal static bool ShowOwnBody(bool show)
	{
		object localSkin = LocalSkin;
		if (localSkin == null)
		{
			return false;
		}
		if (show)
		{
			GameBinder.TryInvoke(GameBinder.Method("PlayerSkin", "InitializeOther", 0), localSkin);
		}
		string[] bodyRenderers = BodyRenderers;
		foreach (string fieldName in bodyRenderers)
		{
			SetRenderer(localSkin, fieldName, show);
		}
		bodyRenderers = LocalHandRenderers;
		foreach (string fieldName2 in bodyRenderers)
		{
			SetRenderer(localSkin, fieldName2, !show);
		}
		_bodyShown = show;
		return true;
	}

	private static void SetRenderer(object skin, string fieldName, bool enabled)
	{
		FieldInfo fieldInfo = GameBinder.Field("PlayerSkin", fieldName);
		if (fieldInfo == null)
		{
			return;
		}
		try
		{
			if (!(fieldInfo.GetValue(skin) is Renderer renderer) || renderer == null)
			{
				if (enabled)
				{
					Plugin.Log.LogWarning((object)("PlayerSkin." + fieldName + " is empty on the local player."));
				}
				return;
			}
			if (renderer.gameObject != null && renderer.gameObject.activeSelf != enabled)
			{
				renderer.gameObject.SetActive(enabled);
			}
			renderer.enabled = enabled;
			if (enabled)
			{
				renderer.shadowCastingMode = ShadowCastingMode.On;
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)(fieldName + ": " + ex.Message));
		}
	}

	internal static void FreeCamStep(float speed, float sensitivity)
	{
		if (_freeCamActive && !(_camTransform == null))
		{
			float num = Input.GetAxisRaw("Mouse X") * sensitivity;
			float num2 = Input.GetAxisRaw("Mouse Y") * sensitivity;
			Vector3 eulerAngles = _camTransform.eulerAngles;
			eulerAngles.x = NormalizePitch(eulerAngles.x - num2);
			eulerAngles.y += num;
			eulerAngles.z = 0f;
			_camTransform.eulerAngles = eulerAngles;
			Vector3 zero = Vector3.zero;
			if (Input.GetKey(KeyCode.W))
			{
				zero += _camTransform.forward;
			}
			if (Input.GetKey(KeyCode.S))
			{
				zero -= _camTransform.forward;
			}
			if (Input.GetKey(KeyCode.D))
			{
				zero += _camTransform.right;
			}
			if (Input.GetKey(KeyCode.A))
			{
				zero -= _camTransform.right;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				zero += Vector3.up;
			}
			if (Input.GetKey(KeyCode.LeftControl))
			{
				zero -= Vector3.up;
			}
			float num3 = (Input.GetKey(KeyCode.LeftShift) ? 3f : 1f);
			if (zero.sqrMagnitude > 0.001f)
			{
				_camTransform.position += zero.normalized * speed * num3 * Time.unscaledDeltaTime;
			}
		}
	}

	private static float NormalizePitch(float pitch)
	{
		if (pitch > 180f)
		{
			pitch -= 360f;
		}
		return Mathf.Clamp(pitch, -89f, 89f);
	}

	internal static void Rescan(bool creatures, bool items, bool players, float interval = 0.5f)
	{
		EspSnapshot espSnapshot = EspSnapshotBuilder.Refresh(creatures, items, players, interval);
		if (_espSnapshotRevision == espSnapshot.Revision)
		{
			return;
		}
		_espSnapshotRevision = espSnapshot.Revision;
		_targets.Clear();
		AppendTargets(espSnapshot.Creatures, EspKind.Creature);
		AppendTargets(espSnapshot.Players, EspKind.Player);
		AppendTargets(espSnapshot.Items, EspKind.Item);
		AppendTargets(espSnapshot.Containers, EspKind.Item);
	}

	private static void AppendTargets(IReadOnlyList<EntityRecord> records, EspKind kind)
	{
		for (int i = 0; i < records.Count; i++)
		{
			EntityRecord entityRecord = records[i];
			if (!entityRecord.IsAlive)
			{
				continue;
			}
			_targets.Add(new EspTarget
			{
				Transform = entityRecord.Transform,
				Label = entityRecord.Name,
				Kind = kind
			});
		}
	}
}
