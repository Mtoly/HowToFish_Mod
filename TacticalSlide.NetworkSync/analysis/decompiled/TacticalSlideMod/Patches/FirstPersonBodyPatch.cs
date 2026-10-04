using System.Collections.Generic;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace TacticalSlideMod.Patches;

public static class FirstPersonBodyPatch
{
	[HarmonyPatch(typeof(Player), "InitializePlayer")]
	public static class Player_InitializePlayer_Patch
	{
		public static void Prefix(Player __instance)
		{
			if (!(__instance == null) && TacticalSlidePlugin.ModEnabled.Value && TacticalSlidePlugin.EnableFirstPersonLegs.Value && !HasSeaLegsMod && __instance.Owner != null && __instance.Owner.IsLocalClient)
			{
				List<GameObject> list = _otherObjectsRef.Invoke(__instance);
				if (list != null && list.Count > 0)
				{
					_cachedOtherObjects = new List<GameObject>(list);
					list.Clear();
				}
			}
		}

		public static void Postfix(Player __instance)
		{
			if (__instance == null || !TacticalSlidePlugin.ModEnabled.Value || !TacticalSlidePlugin.EnableFirstPersonLegs.Value || HasSeaLegsMod || !(__instance.Owner != null) || !__instance.Owner.IsLocalClient)
			{
				return;
			}
			List<GameObject> list = _otherObjectsRef.Invoke(__instance);
			if (_cachedOtherObjects != null && list != null)
			{
				list.AddRange(_cachedOtherObjects);
				foreach (GameObject cachedOtherObject in _cachedOtherObjects)
				{
					if (cachedOtherObject != null)
					{
						cachedOtherObject.SetActive(value: true);
					}
				}
				_cachedOtherObjects = null;
			}
			PlayerLegs playerLegs = _playerLegsRef.Invoke(__instance);
			if (playerLegs != null)
			{
				playerLegs.enabled = true;
			}
			PlayerBody playerBody = _playerBodyRef.Invoke(__instance);
			if (playerBody != null)
			{
				playerBody.enabled = true;
				if (playerBody.Head != null)
				{
					playerBody.Head.localScale = Vector3.one * 0.001f;
				}
			}
			PlayerArms playerArms = _playerArmsRef.Invoke(__instance);
			if (playerArms != null)
			{
				playerArms.enabled = false;
				Renderer[] componentsInChildren = playerArms.GetComponentsInChildren<Renderer>(includeInactive: true);
				foreach (Renderer renderer in componentsInChildren)
				{
					if (renderer != null)
					{
						renderer.enabled = false;
					}
				}
				IK[] componentsInChildren2 = playerArms.GetComponentsInChildren<IK>(includeInactive: true);
				foreach (IK iK in componentsInChildren2)
				{
					if (iK != null)
					{
						iK.enabled = false;
					}
				}
			}
			PlayerSkin playerSkin = _playerSkinRef.Invoke(__instance);
			if (playerSkin != null)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = _hatRendererRef.Invoke(playerSkin);
				if (skinnedMeshRenderer != null)
				{
					skinnedMeshRenderer.enabled = false;
				}
				SkinnedMeshRenderer skinnedMeshRenderer2 = _accessoryRendererRef.Invoke(playerSkin);
				if (skinnedMeshRenderer2 != null)
				{
					skinnedMeshRenderer2.enabled = false;
				}
			}
			if (__instance.CurCam != null)
			{
				__instance.CurCam.nearClipPlane = 0.05f;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerArms), "SetIKTarget")]
	public static class PlayerArms_SetIKTarget_Patch
	{
		public static bool Prefix(PlayerArms __instance)
		{
			if (HasSeaLegsMod || __instance == null)
			{
				return true;
			}
			Player componentInParent = __instance.GetComponentInParent<Player>();
			if (componentInParent != null && componentInParent.Owner != null && componentInParent.Owner.IsLocalClient)
			{
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(IK), "LateUpdate")]
	public static class IK_LateUpdate_Patch
	{
		public static bool Prefix(IK __instance)
		{
			if (HasSeaLegsMod || __instance == null)
			{
				return true;
			}
			PlayerArms componentInParent = __instance.GetComponentInParent<PlayerArms>();
			if (componentInParent != null)
			{
				Player componentInParent2 = componentInParent.GetComponentInParent<Player>();
				if (componentInParent2 != null && componentInParent2.Owner != null && componentInParent2.Owner.IsLocalClient)
				{
					return false;
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PlayerLegs), "Update")]
	public static class PlayerLegs_Update_Bridge_Patch
	{
		public static void Prefix(PlayerLegs __instance)
		{
			if (__instance == null || !TacticalSlidePlugin.ModEnabled.Value || !TacticalSlidePlugin.EnableFirstPersonLegs.Value || HasSeaLegsMod)
			{
				return;
			}
			Player player = _legsPlayerRef.Invoke(__instance);
			if (player != null && player.Owner != null && player.Owner.IsLocalClient)
			{
				OtherPlayer other = player.Other;
				PlayerMovement movement = player.Movement;
				if (other != null && movement != null)
				{
					_otherGroundedRef.Invoke(other) = movement.Grounded;
					float magnitude = movement.Velocity.magnitude;
					_otherVelMagRef.Invoke(other) = magnitude;
					Vector3 vector = new Vector3(movement.Velocity.x, 0f, movement.Velocity.z);
					_otherFlatVelocityRef.Invoke(other) = vector;
					_otherFlatLocalVelocityRef.Invoke(other) = player.Transform.InverseTransformDirection(vector);
				}
			}
		}
	}

	private static readonly FieldRef<Player, List<GameObject>> _otherObjectsRef = AccessTools.FieldRefAccess<Player, List<GameObject>>("_otherObjects");

	private static readonly FieldRef<Player, PlayerLegs> _playerLegsRef = AccessTools.FieldRefAccess<Player, PlayerLegs>("_playerLegs");

	private static readonly FieldRef<Player, PlayerBody> _playerBodyRef = AccessTools.FieldRefAccess<Player, PlayerBody>("_playerBody");

	private static readonly FieldRef<Player, PlayerArms> _playerArmsRef = AccessTools.FieldRefAccess<Player, PlayerArms>("_playerArms");

	private static readonly FieldRef<Player, PlayerSkin> _playerSkinRef = AccessTools.FieldRefAccess<Player, PlayerSkin>("_playerSkin");

	private static readonly FieldRef<PlayerSkin, SkinnedMeshRenderer> _hatRendererRef = AccessTools.FieldRefAccess<PlayerSkin, SkinnedMeshRenderer>("_hatRenderer");

	private static readonly FieldRef<PlayerSkin, SkinnedMeshRenderer> _accessoryRendererRef = AccessTools.FieldRefAccess<PlayerSkin, SkinnedMeshRenderer>("_accessoryRenderer");

	private static readonly FieldRef<PlayerLegs, Player> _legsPlayerRef = AccessTools.FieldRefAccess<PlayerLegs, Player>("_player");

	private static readonly FieldRef<OtherPlayer, bool> _otherGroundedRef = AccessTools.FieldRefAccess<OtherPlayer, bool>("_grounded");

	private static readonly FieldRef<OtherPlayer, float> _otherVelMagRef = AccessTools.FieldRefAccess<OtherPlayer, float>("<VelMag>k__BackingField");

	private static readonly FieldRef<OtherPlayer, Vector3> _otherFlatVelocityRef = AccessTools.FieldRefAccess<OtherPlayer, Vector3>("<FlatVelocity>k__BackingField");

	private static readonly FieldRef<OtherPlayer, Vector3> _otherFlatLocalVelocityRef = AccessTools.FieldRefAccess<OtherPlayer, Vector3>("<FlatLocalVelocity>k__BackingField");

	private static List<GameObject> _cachedOtherObjects = null;

	public static bool HasSeaLegsMod => Chainloader.PluginInfos.ContainsKey("Azumatt.SeaLegs");
}
