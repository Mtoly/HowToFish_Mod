from pathlib import Path
root=Path(r"D:/Code/How2fish/EnvinciblesMods-CompleteCheatMenu")
wc=(root/"decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs").read_text(encoding="utf-8")
sr=(root/"decompiled-src/CompleteCheatMenu/Targeting/ShotRedirector.cs").read_text(encoding="utf-8")
ap=(root/"decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectile_Patch.cs").read_text(encoding="utf-8")
aps=(root/"decompiled-src/CompleteCheatMenu/Patches/WeaponAddProjectiles_Patch.cs").read_text(encoding="utf-8")
assert "AlignFirePointToView(currentCamera, firePoint)" not in wc
assert "firePoint.rotation =" not in sr
assert "InitialVelocityRedirector.TryRedirect" in ap
assert "InitialVelocityRedirector.TryRedirectAll" in aps
assert "LaunchVelocity = redirected" in ap
assert "LaunchVelocities = redirected" in aps
print("MAGIC_BULLET_STYLE_TESTS=PASS cases=6")
