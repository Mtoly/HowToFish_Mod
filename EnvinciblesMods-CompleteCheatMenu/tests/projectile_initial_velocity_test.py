from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"

redirector_path = SRC / "Targeting" / "InitialVelocityRedirector.cs"
registration_path = SRC / "Targeting" / "ProjectileSpawnRegistration.cs"
single_patch_path = SRC / "Patches" / "WeaponAddProjectile_Patch.cs"
multi_patch_path = SRC / "Patches" / "WeaponAddProjectiles_Patch.cs"

for path in (redirector_path, registration_path, single_patch_path, multi_patch_path):
    assert path.exists(), path

redirector = redirector_path.read_text(encoding="utf-8")
registration = registration_path.read_text(encoding="utf-8")
single_patch = single_patch_path.read_text(encoding="utf-8")
multi_patch = multi_patch_path.read_text(encoding="utf-8")
shoot_patch = (SRC / "Patches" / "WeaponShoot_Patch.cs").read_text(encoding="utf-8")
weapon_cheats = (SRC / "Cheats" / "WeaponCheats.cs").read_text(encoding="utf-8")
bindings = (SRC / "Targeting" / "ProjectileBindings.cs").read_text(encoding="utf-8")
context = (SRC / "Targeting" / "MagicShotContext.cs").read_text(encoding="utf-8")

# Seam 1: Weapon.Shoot owns a scoped target context and clears it in a Harmony Finalizer.
assert "Prefix(Weapon __instance)" in shoot_patch
assert "WeaponCheats.PrepareShot(__instance)" in shoot_patch
assert "MagicShotContext.Begin(__instance, solution)" in shoot_patch
assert "Finalizer(Exception __exception)" in shoot_patch
assert "MagicShotContext.End();" in shoot_patch
assert "return __exception;" in shoot_patch
assert "TargetSolution PrepareShot(Weapon weapon)" in weapon_cheats

# Seam 2: single and multi projectile entry points rewrite the real velocity arguments.
assert 'GameBinder.Method("ProjectileManager", "AddProjectile", 8)' in single_patch
assert "ref Vector3 velocity" in single_patch
assert "InitialVelocityRedirector.TryRedirect" in single_patch
assert "ProjectileSpawnRegistration.RegisterCreated" in single_patch
assert 'GameBinder.Method("ProjectileManager", "AddProjectiles", 8)' in multi_patch
assert "ref Vector3[] velocities" in multi_patch
assert "InitialVelocityRedirector.TryRedirectAll" in multi_patch
assert "ProjectileSpawnRegistration.RegisterCreatedRange" in multi_patch

# Seam 3: the redirector preserves speed, clones multi-shot arrays, and rejects invalid vectors.
for token in ("originalVelocity.magnitude", "direction.normalized * speed", "velocities.Clone()", "IsFinite"):
    assert token in redirector, token

# Seam 4: registration captures the effective local base ID before the game increments _nextId.
assert 'GameBinder.Field("ProjectileManager", "_nextId")' in bindings
assert 'GameBinder.Field("Weapon", "_weaponInfo")' in bindings
assert "CaptureBaseId" in registration
assert "ProjectileBindings.PlayerProjectiles" in registration
assert "owner.Owner" in registration
assert "ProjectileTracker.Register" in registration

# Seam 5: only the active local weapon spawn can consume the context.
assert "MatchesSpawn" in context
for token in ("isLocal", "fromNpc", "ActiveWeapon.Holder", "ActiveWeaponInfo"):
    assert token in context, token

print("PROJECTILE_INITIAL_VELOCITY_TESTS=PASS cases=5")
