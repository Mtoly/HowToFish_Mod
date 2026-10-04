from pathlib import Path
root=Path(r"D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu")
g=(root/'decompiled-src/CompleteCheatMenu/Targeting/ProjectileGuidance.cs').read_text(encoding='utf-8')
c=(root/'decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs').read_text(encoding='utf-8')
p=(root/'decompiled-src/CompleteCheatMenu/Runtime/Presets.cs').read_text(encoding='utf-8')
u=(root/'decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs').read_text(encoding='utf-8')
for token in ('GuidanceMode','Tracking','Returning','NaturalFlight','RawVelocity','LaunchVelocity','Invalidate','StepReturn','ProjectileTracker.TryGetLaunchData','ProjectileTracker.Unregister','Vector3.RotateTowards'):
    assert token in g, token
for token in ('AimReturnToLaunchVelocity','AimReturnDuration','AimReturnTurnSpeed'):
    assert token in c and token in p and token in u, token
assert 'Acquire(' not in g
print('PROJECTILE_GUIDANCE_STATE_TESTS=PASS cases=6')
