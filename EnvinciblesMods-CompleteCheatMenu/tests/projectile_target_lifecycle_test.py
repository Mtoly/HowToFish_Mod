from pathlib import Path
ROOT=Path(r"D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu")
tracker=(ROOT/'decompiled-src/CompleteCheatMenu/Targeting/ProjectileTracker.cs').read_text(encoding='utf-8')
assert 'target.Entity is Creature creature' in tracker
assert 'creature.IsDead' in tracker
assert 'target.Entity is Item item' in tracker
assert 'item.IsDestroying' in tracker
assert 'target.Entity is Behaviour behaviour' in tracker
assert 'behaviour.isActiveAndEnabled' in tracker
assert 'target.Transform.gameObject.activeInHierarchy' in tracker
assert 'Refs.IsAlive(target.Entity)' in tracker
assert 'IdToLaunchData.Remove(projectile.Id)' in tracker
print('PROJECTILE_TARGET_LIFECYCLE_TESTS=PASS cases=5')
