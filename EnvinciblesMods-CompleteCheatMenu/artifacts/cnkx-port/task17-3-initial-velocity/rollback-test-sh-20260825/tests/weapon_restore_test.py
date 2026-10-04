import sys
from pathlib import Path

TESTS = Path(__file__).resolve().parent
PROJECT = TESTS.parent
sys.path.insert(0, str(TESTS))

try:
    from cnkx_weapon_store_impl import WeaponStateStoreModel, projectile_speed
except (ImportError, ModuleNotFoundError):
    print("WEAPON_RESTORE_TESTS=FAIL missing implementation")
    raise SystemExit(1)


weapon_a = {"_projSpeed": 80.0, "_spread": 1.5, "adjacent": 777}
weapon_b = {"_projSpeed": 120.0, "_spread": 2.0, "adjacent": 888}
store = WeaponStateStoreModel(allowed_fields={"_projSpeed", "_spread"})

# 每个武器实例独立保存首次原值。
assert store.set(weapon_a, "_projSpeed", 300.0)
assert store.set(weapon_a, "_projSpeed", 500.0)
assert store.set(weapon_b, "_projSpeed", 600.0)
assert store.original(weapon_a, "_projSpeed") == 80.0
assert store.original(weapon_b, "_projSpeed") == 120.0

# 切换武器时只恢复上一件武器，不把 A 的原值写入 B。
store.restore_weapon(weapon_a)
assert weapon_a["_projSpeed"] == 80.0
assert weapon_b["_projSpeed"] == 600.0
store.restore_weapon(weapon_b)
assert weapon_b["_projSpeed"] == 120.0

# 高速子弹与瞬击使用独立开关和值，瞬击优先但关闭后可回到高速子弹值。
assert projectile_speed(False, False, 300.0, 10000.0) is None
assert projectile_speed(True, False, 300.0, 10000.0) == 300.0
assert projectile_speed(False, True, 300.0, 10000.0) == 10000.0
assert projectile_speed(True, True, 300.0, 10000.0) == 10000.0

# 关闭字段功能后精确恢复该字段，其他字段和相邻值不变。
store.set(weapon_a, "_spread", 0.0)
assert weapon_a["adjacent"] == 777
assert store.restore_field_all("_spread") == 1
assert weapon_a["_spread"] == 1.5
assert weapon_a["adjacent"] == 777

# 未解析字段不会保存、写入或尝试相邻字段。
snapshot = dict(weapon_a)
assert not store.set(weapon_a, "_missing", 1)
assert weapon_a == snapshot

# 插件卸载时恢复全部仍被修改的武器。
store.set(weapon_a, "_projSpeed", 999.0)
store.set(weapon_b, "_spread", 0.0)
assert store.restore_all() == 2
assert weapon_a["_projSpeed"] == 80.0
assert weapon_b["_spread"] == 2.0

# 生产实现只允许解析后的 FieldInfo 精确写入，并在插件销毁时 RestoreAll。
store_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Cheats/WeaponStateStore.cs"
).read_text(encoding="utf-8")
weapon_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs"
).read_text(encoding="utf-8")
plugin_source = (
    PROJECT / "decompiled-src/CompleteCheatMenu/Plugin.cs"
).read_text(encoding="utf-8")
assert "Dictionary<object, Dictionary<FieldInfo, object>>" in store_source
assert "field.SetValue(weapon" in store_source
assert "Marshal" not in store_source
assert "IntPtr" not in store_source
assert "RestorePreviousWeapon" in weapon_source
assert "CheatState.InstantHit" in weapon_source
assert "WeaponCheats.RestoreAllWeapons" in plugin_source

print("WEAPON_RESTORE_TESTS=PASS cases=7")
