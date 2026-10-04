from pathlib import Path
root=Path(r"D:/Code/How2fish/EnvinciblesMods-CompleteCheatMenu")
g=(root/"decompiled-src/CompleteCheatMenu/Targeting/ProjectileGuidance.cs").read_text(encoding="utf-8")
c=(root/"decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs").read_text(encoding="utf-8")
p=(root/"decompiled-src/CompleteCheatMenu/Runtime/Presets.cs").read_text(encoding="utf-8")
u=(root/"decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs").read_text(encoding="utf-8")
assert "AimMagicBulletStyle" in g and "AimMagicBulletStyle" in c and "AimMagicBulletStyle" in p and "AimMagicBulletStyle" in u
branch=g.index("if (CheatState.AimMagicBulletStyle)")
steer=g.index("ProjectileGuidanceMath.TrySteer")
assert branch < steer
assert "projectile.Velocity" not in g[branch:steer]
assert "ProjectileTracker.TryGetTarget" in g
print("MAGIC_BULLET_FLIGHT_MODE_TESTS=PASS cases=5")
