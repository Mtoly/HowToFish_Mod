from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"
registration = (SRC / "Targeting" / "ProjectileSpawnRegistration.cs").read_text(encoding="utf-8")
tracker = (SRC / "Targeting" / "ProjectileTracker.cs").read_text(encoding="utf-8")
single = (SRC / "Patches" / "WeaponAddProjectile_Patch.cs").read_text(encoding="utf-8")
multi = (SRC / "Patches" / "WeaponAddProjectiles_Patch.cs").read_text(encoding="utf-8")

for token in ("RawVelocity", "LaunchVelocity", "RawVelocities", "LaunchVelocities"):
    assert token in registration, token
assert "bool Register(Projectile projectile, TargetSolution target, Vector3 rawVelocity, Vector3 launchVelocity)" in tracker
assert "ProjectileToLaunchData" in tracker
assert "IdToLaunchData" in tracker
assert "TryGetLaunchData" in tracker
assert "Vector3 rawVelocity = velocity" in single
assert "RawVelocity = rawVelocity" in single
assert "LaunchVelocity = redirected" in single
assert "RegisterCreated(__instance, __state.Owner, __state.BaseId, __state.Target, __state.RawVelocity, __state.LaunchVelocity)" in single
assert "Vector3[] rawVelocities = velocities != null ? (Vector3[])velocities.Clone() : null" in multi
assert "RawVelocities = rawVelocities" in multi
assert "LaunchVelocities = redirected" in multi
assert "RegisterCreatedRange(__instance, __state.Owner, __state.BaseId, __state.Count, __state.Target, __state.RawVelocities, __state.LaunchVelocities)" in multi
print("PROJECTILE_SPAWN_METADATA_TESTS=PASS cases=4")

