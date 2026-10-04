using CompleteCheatMenu.Cheats;
using CompleteCheatMenu.Runtime;
using UnityEngine;

namespace CompleteCheatMenu.UI.Tabs;

internal static class WeaponsTab
{
	internal const string Title = "Weapons";

	internal static void Draw()
	{
		if (!Widgets.RequireHost(CheatGate.BlockReason()))
		{
			return;
		}
		Widgets.Section("Held weapon", delegate
		{
			if (!WeaponCheats.HoldingWeapon)
			{
				Widgets.Note("Hold a gun. Settings below are applied to whatever you're carrying and re-applied when you switch weapons.");
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(WeaponCheats.WeaponName, Theme.H1);
				GUILayout.FlexibleSpace();
				GUILayout.Label($"ammo {WeaponCheats.Ammo}", Theme.Muted);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				Widgets.ActionButton("Refill now", WeaponCheats.RefillAmmo, "Ammo refilled");
				Widgets.ActionButton("Free bullet upgrade", WeaponCheats.BuyBulletUpgrade, "Bullet upgrade applied", Theme.BtnAccent);
				GUILayout.EndHorizontal();
			}
		});
		Widgets.Section("Firing", delegate
		{
			CheatState.InfiniteAmmo = Widgets.ToggleRow("Infinite ammo", CheatState.InfiniteAmmo, CheatState.InfiniteAmmo ? "on" : "off");
			if (CheatState.InfiniteAmmo)
			{
				CheatState.AmmoTopUp = Widgets.IntSliderRow("Held at", CheatState.AmmoTopUp, 1, 999);
				Widgets.Note("Also cancels queued reloads, so the gun never stops to reload.");
			}
			CheatState.NoSpread = Widgets.ToggleRow("Perfect accuracy", CheatState.NoSpread, CheatState.NoSpread ? "on" : "off");
			CheatState.FireWhereLooking = Widgets.ToggleRow("Fire where you're looking", CheatState.FireWhereLooking, CheatState.FireWhereLooking ? "on" : "off");
			Widgets.Note("Aligns the gun's fire point with the camera, removing the barrel-to-eye offset that makes hipfire drift off the crosshair. Pair with perfect accuracy for pinpoint hipfire.");
			CheatState.ZeroRecoil = Widgets.ToggleRow("Zero recoil", CheatState.ZeroRecoil, CheatState.ZeroRecoil ? "on" : "off");
			Widgets.Note("Clears screen kick, weapon kick and the knockback the shot applies to you.");
			CheatState.RapidFire = Widgets.ToggleRow("Rapid fire", CheatState.RapidFire, CheatState.RapidFire ? "on" : "off");
			if (CheatState.RapidFire)
			{
				CheatState.FireInterval = Widgets.SliderRow("Seconds between shots", CheatState.FireInterval, 0.01f, 0.5f, "0.###", 0.05f);
			}
		});
		Widgets.Section("Projectiles", delegate
		{
			CheatState.MultiShot = Widgets.ToggleRow("Extra projectiles", CheatState.MultiShot, CheatState.MultiShot ? "on" : "off");
			if (CheatState.MultiShot)
			{
				CheatState.ProjectileCount = Widgets.SliderRow("Per shot", CheatState.ProjectileCount, 1f, 30f, "0", 1f);
				Widgets.Note("Turns any gun into a shotgun. Combine with perfect accuracy to stack every pellet on one point.");
			}
			CheatState.FastBullets = Widgets.ToggleRow("Bullet speed", CheatState.FastBullets, CheatState.FastBullets ? "on" : "off");
			if (CheatState.FastBullets)
			{
				CheatState.ProjectileSpeed = Widgets.SliderRow("Speed", CheatState.ProjectileSpeed, 20f, 2000f, "0", 300f);
			}
			GUILayout.Space(4f);
			Widgets.ActionButton("Restore this weapon's originals", WeaponCheats.ResetHeldWeapon, "Original values restored", Theme.Btn);
			Widgets.Note(WeaponCheats.HasOriginals ? "The values this gun shipped with were saved before anything was changed." : "Nothing has been changed on this weapon yet.");
		});
		Widgets.Section("Aim assist", delegate
		{
			CheatState.Aimbot = Widgets.ToggleRow("Aimbot", CheatState.Aimbot, CheatState.Aimbot ? "ACTIVE" : "off");
			Widgets.Note("Points the gun's fire point at the best target while your view stays under your control — your shots go where the aimbot looks, your camera doesn't get yanked around.");
			GUILayout.BeginHorizontal();
			CheatState.AimAtCreatures = GUILayout.Toggle(CheatState.AimAtCreatures, "  Creatures", Theme.Toggle);
			CheatState.AimAtPlayers = GUILayout.Toggle(CheatState.AimAtPlayers, "  Players", Theme.Toggle);
			CheatState.AimRequireLineOfSight = GUILayout.Toggle(CheatState.AimRequireLineOfSight, "  Needs line of sight", Theme.Toggle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			CheatState.AimExcludeDead = GUILayout.Toggle(CheatState.AimExcludeDead, "  排除死亡目标", Theme.Toggle);
			CheatState.AimExcludeSeagulls = GUILayout.Toggle(CheatState.AimExcludeSeagulls, "  排除海鸥", Theme.Toggle);
			CheatState.AimExcludeHookedFish = GUILayout.Toggle(CheatState.AimExcludeHookedFish, "  排除上钩鱼", Theme.Toggle);
			GUILayout.EndHorizontal();
			CheatState.AimPrioritizeAlbatross = Widgets.ToggleRow("信天翁优先", CheatState.AimPrioritizeAlbatross, CheatState.AimPrioritizeAlbatross ? "开" : "关");
			CheatState.AimFov = Widgets.SliderRow("Cone (degrees)", CheatState.AimFov, 1f, 180f, "0", 35f);
			CheatState.AimRange = Widgets.SliderRow("Range", CheatState.AimRange, 10f, 600f, "0", 200f);
			CheatState.AimHeightOffset = Widgets.SliderRow("Aim height", CheatState.AimHeightOffset, -1f, 3f, "0.##", 0.5f);
			CheatState.AimDistanceWeight = Widgets.SliderRow("距离权重", CheatState.AimDistanceWeight, 0f, 3f, "0.##", 1f);
			CheatState.AimAngleWeight = Widgets.SliderRow("角度权重", CheatState.AimAngleWeight, 0f, 3f, "0.##", 0.5f);
			CheatState.AimLockDuration = Widgets.SliderRow("目标保持时间", CheatState.AimLockDuration, 0f, 1f, "0.##", 0.2f);
			CheatState.AimSwitchPenalty = Widgets.SliderRow("切换目标惩罚", CheatState.AimSwitchPenalty, 0f, 30f, "0.#", 8f);
			CompleteCheatMenu.Targeting.TargetSolution currentTarget = WeaponCheats.CurrentTarget;
			GUILayout.Space(4f);
			if (currentTarget != null)
			{
				string targetName = (currentTarget.Entity as Object)?.name ?? currentTarget.Transform?.name ?? "-";
				GUILayout.Label($"目标：{targetName}   {currentTarget.Distance:0}米   偏离中心 {currentTarget.Angle:0}°   瞄准点：{currentTarget.Bone}", Theme.Body);
			}
			else
			{
				GUILayout.Label(CheatState.Aimbot ? "No target in the cone." : "Aimbot is off.", Theme.Muted);
			}
			Widgets.Note("Aim height lifts the point of aim off the target's origin — raise it for head shots on tall creatures.");
		});
	}
}
