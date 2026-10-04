using HarmonyLib;
using UnityEngine;

namespace FishAimbotMod
{
    [HarmonyPatch(typeof(PlayerCamera), "ApplyAimAssist")]
    internal static class AimAssistPatch
    {
        private static readonly AccessTools.FieldRef<PlayerCamera, Vector3> GetRot =
            AccessTools.FieldRefAccess<PlayerCamera, Vector3>("_rot");

        private const KeyCode AimKey = (KeyCode)324;
        private const float FovAngle = 180f;

        private static void Postfix(PlayerCamera __instance)
        {
            if (!Input.GetKey(AimKey))
            {
                return;
            }

            Transform camera = __instance.CamTransform;
            Vector3 cameraPosition = camera.position;
            Transform bestTarget = FishTargeting.GetBestTarget(cameraPosition, camera.forward, FovAngle);
            if (bestTarget == null)
            {
                return;
            }

            Vector3 direction = (bestTarget.position - cameraPosition).normalized;
            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float pitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
            ref Vector3 rotation = ref GetRot(__instance);
            rotation.x = Mathf.Clamp(pitch, -90f, 90f);
            rotation.y = yaw;
        }
    }
}
