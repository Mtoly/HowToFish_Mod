from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"

guidance = (SRC / "Targeting" / "ProjectileGuidance.cs").read_text(encoding="utf-8")
tracker = (SRC / "Targeting" / "ProjectileTracker.cs").read_text(encoding="utf-8")
spawn = (SRC / "Targeting" / "ProjectileSpawnRegistration.cs").read_text(encoding="utf-8")
add_projectile = (SRC / "Patches" / "WeaponAddProjectile_Patch.cs").read_text(encoding="utf-8")

# Task 1 seam: a tracked projectile must retain both launch baselines.
for token in ("RawVelocity", "LaunchVelocity", "InvalidatedAt", "ReturnStartVelocity"):
    assert token in guidance or token in spawn, f"missing guidance baseline field: {token}"
assert "LaunchVelocity" in add_projectile, "spawn patch must capture launch velocity"

# Task 1 seam: target validity must reject dead, destroying, or disabled targets.
for token in ("IsDead", "IsDestroying", "isActiveAndEnabled"):
    assert token in tracker, f"tracker lifecycle check missing: {token}"

# Task 1 seam: invalid targets transition out of Tracking without reacquiring a target.
assert "NaturalFlight" in guidance, "missing NaturalFlight state"
assert "ReturnToLaunchVelocity" in guidance, "missing opt-in return mode"
assert "Acquire(" not in guidance, "flight guidance must not reacquire a target"

# Task 1 seam: non-local/NPC projectiles remain outside the guidance path.
ownership = (SRC / "Targeting" / "ProjectileOwnership.cs").read_text(encoding="utf-8")
assert "FromNpc" in ownership and "IsLocal" in ownership and "Refs.LocalPlayer" in ownership

print("PROJECTILE_GUIDANCE_RESET_SPEC=PASS cases=5")
