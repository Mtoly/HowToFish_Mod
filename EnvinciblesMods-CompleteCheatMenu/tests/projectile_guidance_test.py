from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"

guidance_path = SRC / "Targeting" / "ProjectileGuidance.cs"
math_path = SRC / "Targeting" / "ProjectileGuidanceMath.cs"
update_patch_path = SRC / "Patches" / "ProjectileUpdateScan_Patch.cs"
remove_patch_path = SRC / "Patches" / "ProjectileRemove_Patch.cs"

for path in (guidance_path, math_path, update_patch_path, remove_patch_path):
    assert path.exists(), path

guidance = guidance_path.read_text(encoding="utf-8")
math_source = math_path.read_text(encoding="utf-8")
update_patch = update_patch_path.read_text(encoding="utf-8")
remove_patch = remove_patch_path.read_text(encoding="utf-8")
state = (SRC / "Runtime" / "CheatState.cs").read_text(encoding="utf-8")
presets = (SRC / "Runtime" / "Presets.cs").read_text(encoding="utf-8")
weapons = (SRC / "UI" / "Tabs" / "WeaponsTab.cs").read_text(encoding="utf-8")
diagnostics = (SRC / "UI" / "Tabs" / "DiagnosticsTab.cs").read_text(encoding="utf-8")
plugin = (SRC / "Plugin.cs").read_text(encoding="utf-8")
predictor = (SRC / "Targeting" / "BallisticPredictor.cs").read_text(encoding="utf-8")
single_patch = (SRC / "Patches" / "WeaponAddProjectile_Patch.cs").read_text(encoding="utf-8")
multi_patch = (SRC / "Patches" / "WeaponAddProjectiles_Patch.cs").read_text(encoding="utf-8")

# Seam 1: UpdateProjectileScan steers tracked local projectiles before collision scanning.
assert 'GameBinder.Method("ProjectileManager", "UpdateProjectileScan", 2)' in update_patch
assert "ProjectileGuidance.Step(projectile" in update_patch
assert "Time.unscaledTime" in update_patch
assert "Time.fixedDeltaTime" in update_patch

# Seam 2: guidance is bounded by duration and update interval and never reacquires a target.
for token in (
    "AimProjectileTrackingTime",
    "AimProjectileUpdateInterval",
    "AimProjectileTurnSpeed",
    "ProjectileTracker.TryGetTarget",
    "ProjectileTracker.Unregister",
    "NextUpdate",
    "StartTime",
):
    assert token in guidance, token
assert "Acquire(" not in guidance

# Seam 3: target velocity prefers Rigidbody.GetPointVelocity and falls back to VelocityTracker.
assert "GetComponentInParent<Rigidbody>" in guidance
assert "GetPointVelocity" in guidance
assert "VelocityTracker" in guidance
assert "ProjectileGuidanceMath.TrySteer" in guidance
assert "projectile.GravityForce" in guidance

# Seam 4: steering preserves speed and applies a degrees-per-second turn cap.
for token in (
    "currentVelocity.magnitude",
    "Vector3.RotateTowards",
    "Mathf.Deg2Rad",
    "direction.normalized * speed",
    "BallisticPredictor.PredictProjectile",
):
    assert token in math_source, token
assert "for (int i = 0; i < 3; i++)" in predictor
assert "maxPredictionTime" in predictor

# Seam 5: removal, unload, settings, presets, UI and diagnostics own the full lifecycle.
assert 'GameBinder.Method("ProjectileManager", "AddToRemoveQueue", 1)' in remove_patch
assert "ProjectileTracker.Unregister(projectile)" in remove_patch
assert "ProjectileGuidance.Forget(projectile)" in remove_patch
assert "ProjectileGuidance.Clear" in plugin
assert "飞行中制导绑定" in diagnostics
for field, key, label in (
    ("AimMaxPredictionTime", "aimMaxPredictionTime", "最大预测时间"),
    ("AimProjectileTrackingTime", "aimProjectileTrackingTime", "弹丸追踪时长"),
    ("AimProjectileUpdateInterval", "aimProjectileUpdateInterval", "制导更新间隔"),
    ("AimProjectileTurnSpeed", "aimProjectileTurnSpeed", "最大转向速度"),
):
    assert field in state
    assert key in presets
    assert label in weapons
assert "weaponInfo.ProjectileGravity" in single_patch
assert "weaponInfo.ProjectileGravity" in multi_patch

print("PROJECTILE_GUIDANCE_TESTS=PASS cases=5")
