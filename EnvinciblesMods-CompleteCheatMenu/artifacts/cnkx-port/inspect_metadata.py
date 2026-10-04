import hashlib
import json
import sys
from pathlib import Path

import dnfile

managed = Path(sys.argv[1])
output = Path(sys.argv[2])

def text(value):
    return str(value or "")

def type_rows(path):
    pe = dnfile.dnPE(str(path))
    if not pe.net or not pe.net.mdtables.TypeDef:
        return []
    result = []
    for row in pe.net.mdtables.TypeDef.rows:
        result.append({
            "namespace": text(row.TypeNamespace),
            "name": text(row.TypeName),
            "fields": sorted(text(index.row.Name) for index in row.FieldList),
            "methods": sorted(text(index.row.Name) for index in row.MethodList),
        })
    return result

assembly_path = managed / "Assembly-CSharp.dll"
rows = type_rows(assembly_path)
by_name = {}
for row in rows:
    by_name.setdefault(row["name"], []).append(row)

def exact_type(name, marker=None):
    matches = by_name.get(name, [])
    if marker:
        marked = [r for r in matches if marker in r["fields"] or ("get_" + marker) in r["methods"]]
        if marked:
            return marked[0]
    return matches[0] if matches else None

def has(row, kind, names):
    if not row:
        return None
    values = row["fields"] if kind == "field" else row["methods"]
    for name in names:
        if name in values:
            return name
    return None

checks = [
    ("Creature.type", "required", "Creature", None, "type", []),
    ("Creature.health", "required", "Creature", None, "method", ["get_Hp"]),
    ("Creature.max_health", "required", "Creature", None, "method", ["get_MaxHp"]),
    ("Creature.dead", "required", "Creature", None, "method", ["get_IsDead"]),
    ("Creature.head", "optional", "Creature", None, "method", ["get_HeadPos"]),
    ("Creature.head_field", "optional", "Creature", None, "field", ["_headPos"]),
    ("Fish.type", "required", "Fish", None, "type", []),
    ("Fish.attached_rod_inherited_from_Item", "optional", "Item", None, "method", ["get_AttachedRod"]),
    ("Player.type", "required", "Player", "LocalPlayer", "type", []),
    ("Player.transform", "required", "Player", "LocalPlayer", "method", ["get_Transform"]),
    ("Player.holding", "required", "Player", "LocalPlayer", "method", ["get_Holding"]),
    ("Player.vitals", "required", "Player", "LocalPlayer", "method", ["get_Vitals"]),
    ("Player.camera", "required", "Player", "LocalPlayer", "method", ["get_Camera"]),
    ("Player.skin", "optional", "Player", "LocalPlayer", "method", ["get_Skin"]),
    ("Weapon.type", "required", "Weapon", None, "type", []),
    ("Weapon.shoot", "required", "Weapon", None, "method", ["Shoot"]),
    ("Weapon.projectile_speed", "required", "Weapon", None, "field", ["_projSpeed"]),
    ("Weapon.spread", "required", "Weapon", None, "field", ["_spread"]),
    ("Weapon.recoil", "required", "Weapon", None, "field", ["_recoilKnockback"]),
    ("Weapon.attachments", "required", "Weapon", None, "method", ["get_Attachments"]),
    ("Projectile.type", "required", "Projectile", None, "type", []),
    ("Projectile.position", "required", "Projectile", None, "field", ["Position"]),
    ("Projectile.velocity", "required", "Projectile", None, "field", ["Velocity"]),
    ("Projectile.gravity", "optional", "Projectile", None, "field", ["GravityForce"]),
    ("Projectile.owner", "optional", "Projectile", None, "field", ["Owner"]),
    ("ProjectileManager.type", "required", "ProjectileManager", None, "type", []),
    ("ProjectileManager.add", "optional", "ProjectileManager", None, "method", ["AddProjectile", "AddProjectiles"]),
    ("ProjectileManager.update_position", "optional", "ProjectileManager", None, "method", ["UpdateProjectilePos"]),
    ("Item.type", "required", "Item", None, "type", []),
    ("Item.total_worth", "required", "Item", None, "method", ["get_TotalWorth"]),
    ("Item.model_height", "optional", "Item", None, "method", ["get_ModelHeight"]),
    ("Item.rigidbody", "optional", "Item", None, "method", ["get_Rig"]),
    ("ItemManager.type", "required", "ItemManager", None, "type", []),
    ("ItemManager.items", "required", "ItemManager", None, "method", ["get_Items"]),
]

results = []
for key, importance, type_name, marker, kind, candidates in checks:
    row = exact_type(type_name, marker)
    if kind == "type":
        match = (row["namespace"] + "." + row["name"]).strip(".") if row else None
    else:
        match = has(row, kind, candidates)
    if match:
        status = "resolved"
    elif importance == "optional":
        status = "optional"
    else:
        status = "missing"
    results.append({"key": key, "importance": importance, "status": status, "match": match or "-", "candidates": candidates})

external_specs = [
    ("UnityEngine.Animator", managed / "UnityEngine.AnimationModule.dll", "Animator"),
    ("UnityEngine.Renderer", managed / "UnityEngine.CoreModule.dll", "Renderer"),
    ("UnityEngine.SkinnedMeshRenderer", managed / "UnityEngine.CoreModule.dll", "SkinnedMeshRenderer"),
    ("UnityEngine.Collider", managed / "UnityEngine.PhysicsModule.dll", "Collider"),
]
for key, path, type_name in external_specs:
    found = False
    if path.exists():
        try:
            found = any(row["name"] == type_name for row in type_rows(path))
        except Exception:
            found = False
    results.append({"key": key, "importance": "required", "status": "resolved" if found else "missing", "match": type_name if found else "-", "candidates": [type_name]})

required_missing = [r for r in results if r["importance"] == "required" and r["status"] == "missing"]
optional_unresolved = [r for r in results if r["importance"] == "optional" and r["status"] != "resolved"]

lines = []
lines.append("CompleteCheatMenu cnKX port binding inventory")
lines.append("ASSEMBLY=" + str(assembly_path))
lines.append("ASSEMBLY_SHA256=" + hashlib.sha256(assembly_path.read_bytes()).hexdigest().upper())
lines.append("TYPE_COUNT=" + str(len(rows)))
lines.append("")
lines.append("[CANDIDATES]")
for result in results:
    candidates = ",".join(result["candidates"]) if result["candidates"] else "-"
    lines.append(f"{result['status'].upper():8} {result['importance'].upper():8} {result['key']} match={result['match']} candidates={candidates}")

lines.append("")
lines.append("[TYPE MEMBERS]")
for type_name, marker in [("Creature", None), ("Fish", None), ("Player", "LocalPlayer"), ("Weapon", None), ("Projectile", None), ("ProjectileManager", None), ("Item", None), ("ItemManager", None)]:
    row = exact_type(type_name, marker)
    lines.append(f"TYPE {type_name} resolved={'yes' if row else 'no'}")
    if row:
        lines.append("  fields=" + ",".join(row["fields"]))
        lines.append("  methods=" + ",".join(row["methods"]))

health_like = sorted(r["name"] for r in rows if "health" in r["name"].lower() or "vital" in r["name"].lower())
lines.append("")
lines.append("[HEALTH/VITAL TYPES]")
lines.append(",".join(health_like) if health_like else "-")
lines.append("")
lines.append("REQUIRED_MISSING=" + str(len(required_missing)))
lines.append("OPTIONAL_UNRESOLVED=" + str(len(optional_unresolved)))
lines.append("BINDING_INVENTORY=" + ("PASS" if not required_missing else "FAIL"))
output.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("BINDING_INVENTORY=" + ("PASS" if not required_missing else "FAIL"))
print("REQUIRED_MISSING=" + str(len(required_missing)))
print("OPTIONAL_UNRESOLVED=" + str(len(optional_unresolved)))
sys.exit(0 if not required_missing else 2)
