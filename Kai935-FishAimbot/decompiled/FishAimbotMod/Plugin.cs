using BepInEx;
using HarmonyLib;

namespace FishAimbotMod;

[BepInPlugin("com.yourname.fishaimbot", "Fish Aimbot", "1.0.1")]
public class Plugin : BaseUnityPlugin
{
	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		new Harmony("com.yourname.fishaimbot").PatchAll();
	}
}
