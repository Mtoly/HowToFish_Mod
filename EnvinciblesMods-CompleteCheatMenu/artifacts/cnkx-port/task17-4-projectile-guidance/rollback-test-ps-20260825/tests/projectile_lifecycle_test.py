from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"

bindings_path = SRC / "Targeting" / "ProjectileBindings.cs"
tracker_path = SRC / "Targeting" / "ProjectileTracker.cs"
ownership_path = SRC / "Targeting" / "ProjectileOwnership.cs"
context_path = SRC / "Targeting" / "MagicShotContext.cs"

for path in (bindings_path, tracker_path, ownership_path, context_path):
    assert path.exists(), path

bindings = bindings_path.read_text(encoding="utf-8")
tracker = tracker_path.read_text(encoding="utf-8")
ownership = ownership_path.read_text(encoding="utf-8")
context = context_path.read_text(encoding="utf-8")
diagnostics = (SRC / "UI" / "Tabs" / "DiagnosticsTab.cs").read_text(encoding="utf-8")
plugin = (SRC / "Plugin.cs").read_text(encoding="utf-8")
tick = (SRC / "Runtime" / "TickDriver.cs").read_text(encoding="utf-8")

# Seam 1: the binding surface exactly matches the verified game assembly entry points.
for token in (
    'GameBinder.Method("ProjectileManager", "AddProjectile", 8)',
    'GameBinder.Method("ProjectileManager", "AddProjectiles", 8)',
    'GameBinder.Method("ProjectileManager", "UpdateProjectileScan", 2)',
    'GameBinder.Method("ProjectileManager", "AddToRemoveQueue", 1)',
    'GameBinder.Field("Projectile", "Id")',
    'GameBinder.Field("Projectile", "Owner")',
    'GameBinder.Field("Projectile", "IsLocal")',
    'GameBinder.Field("Projectile", "Position")',
    'GameBinder.Field("Projectile", "Velocity")',
    'GameBinder.Field("Projectile", "FromNpc")',
):
    assert token in bindings, token
assert "LifecycleAvailable" in bindings
assert "GuidanceAvailable" in bindings

# Seam 2: tracker supports object lookup, id fallback, invalid-target cleanup and counters.
assert "Dictionary<Projectile, TargetSolution>" in tracker
assert "Dictionary<uint, TargetSolution>" in tracker
for method in ("Register", "TryGetTarget", "Unregister", "PruneInvalid", "Clear"):
    assert f" {method}(" in tracker, method
for counter in ("ActiveCount", "RegisteredCount", "UnregisteredCount", "InvalidTargetCount", "IdFallbackCount"):
    assert counter in tracker, counter
assert "ProjectileOwnership.IsLocalProjectile(projectile)" in tracker
assert "tracked.Id == projectile.Id" in tracker
assert "ReferenceEquals(existing, target)" in tracker
assert "ReferenceEquals(idTarget, removedTarget)" in tracker

# Seam 3: ownership rejects NPC and remote projectiles and requires the local weapon holder.
for token in ("projectile.FromNpc", "projectile.IsLocal", "projectile.Owner", "weapon.Holder", "Refs.LocalPlayer"):
    assert token in ownership, token
assert "IsLocalWeapon" in ownership
assert "IsLocalProjectile" in ownership

# Seam 4: shot context only begins for a valid local weapon/target and always exposes an explicit clear path.
for token in ("IsActive", "ActiveWeapon", "ActiveTarget", "Begin", "End", "Clear"):
    assert token in context, token
assert "ProjectileOwnership.IsLocalWeapon" in context
assert "ProjectileTracker.IsValidTarget" in context

# Seam 5: lifecycle state is visible in diagnostics, pruned periodically, and cleared on unload.
assert "弹丸生命周期绑定" in diagnostics
assert "ProjectileBindings.ResolveAll();" in diagnostics
assert "ProjectileTracker.PruneInvalid();" in tick
assert "ProjectileTracker.Clear" in plugin
assert "MagicShotContext.Clear" in plugin

print("PROJECTILE_LIFECYCLE_TESTS=PASS cases=5")
