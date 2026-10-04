import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

weapons = (ROOT / "decompiled-src/CompleteCheatMenu/UI/Tabs/WeaponsTab.cs").read_text(encoding="utf-8")
visuals = (ROOT / "decompiled-src/CompleteCheatMenu/UI/Tabs/VisualsTab.cs").read_text(encoding="utf-8")
diagnostics = (ROOT / "decompiled-src/CompleteCheatMenu/UI/Tabs/DiagnosticsTab.cs").read_text(encoding="utf-8")
state = (ROOT / "decompiled-src/CompleteCheatMenu/Runtime/CheatState.cs").read_text(encoding="utf-8")
presets = (ROOT / "decompiled-src/CompleteCheatMenu/Runtime/Presets.cs").read_text(encoding="utf-8")
targeting = (ROOT / "decompiled-src/CompleteCheatMenu/Targeting/TargetingSystem.cs").read_text(encoding="utf-8")
menu = (ROOT / "decompiled-src/CompleteCheatMenu/UI/MenuWindow.cs").read_text(encoding="utf-8")
localizer = (ROOT / "tools/localize_dll.py").read_text(encoding="utf-8")

# 1. 目标类型必须由单个互斥枚举表示。
assert "internal enum AimTargetMode" in state
assert "internal static AimTargetMode AimTargets" in state
assert "internal static bool AimAtCreatures;" not in state
assert "internal static bool AimAtPlayers;" not in state
assert "AimTargetsCreatures" in targeting and "AimTargetsPlayers" in targeting

# 2. 武器页必须暴露瞬击、预测、追踪概率、可见拉枪和独立补偿。
for token in (
    "瞬击",
    "弹道预测",
    "追踪概率",
    "可见拉枪",
    "距离下调",
    "后坐补偿",
    "目标类型",
    "速度采样最小间隔",
    "传送距离阈值",
    "目标最大速度",
):
    assert token in weapons, token

# 3. 视觉页必须暴露 V2 的所有主要绘制能力。
for token in (
    "方框",
    "骨架",
    "血条",
    "可见状态",
    "手持物",
    "物品价值",
    "容器信息",
    "自瞄有效范围圆",
    "当前目标连线",
):
    assert token in visuals, token

# 4. 诊断页显示注册数量、弹丸绑定、骨骼能力和开火追踪入口。
for token in ("目标注册", "弹速字段", "人体骨骼接口", "包围盒回退", "开火追踪入口"):
    assert token in diagnostics, token
assert 'GameBinder.Field("Weapon", "_projSpeed")' in diagnostics
assert 'GameBinder.Method("Weapon", "Shoot", 0)' in diagnostics

# 5. 所有 Task 5-12 新设置都必须写入预设并有读取分支。
keys = (
    "aimTargetMode", "aimbot", "aimRequireLineOfSight", "aimExcludeDead",
    "aimExcludeSeagulls", "aimExcludeHookedFish", "aimPrioritizeAlbatross",
    "aimFov", "aimRange", "aimHeightOffset", "aimDistanceWeight", "aimAngleWeight",
    "aimScreenRadius", "aimScreenDistanceWeight",
    "aimPriorityBonus", "aimPriorityBonusLimit", "aimPrediction",
    "aimLockDuration", "aimSwitchPenalty", "aimVelocityMinInterval",
    "aimTeleportDistance", "aimMaxTargetSpeed",
    "aimGravityCompensation", "aimGravityScale", "aimTrackingChance",
    "aimTrackingDiagnostics",
    "aimVisibleAssist", "aimVisibleResponse", "aimDistanceDropPerUnit",
    "aimRecoilCompensation", "aimRecoilCompensationStrength",
    "instantHit", "instantHitProjectileSpeed", "espBoxes", "espSkeletons",
    "espHealthBars", "espTypeInfo", "espVisibilityInfo", "espHeldItemInfo",
    "espItemValueInfo", "espContainerInfo", "espAimCircle", "espAimTargetTracer",
)
for key in keys:
    assert presets.count(f'"{key}"') >= 2, key

# 6. 旧预设的两个布尔目标字段仍可迁移到新枚举。
assert 'case "aimAtCreatures"' in presets
assert 'case "aimAtPlayers"' in presets
assert "ApplyLegacyTargetMode" in presets
assert "if (!loadedTargetMode) ApplyLegacyTargetMode" in presets

# 7. 三个页面的直接显示字符串不得残留常见英文界面文案。
diagnostics_ui = diagnostics.split("internal static void RunChecks", 1)[0]
quoted = re.findall(r'"([^"\\]*(?:\\.[^"\\]*)*)"', weapons + visuals + diagnostics_ui)
allowed = {"OK", "MISSING", "ESP", "FOV", "Projectile", "Weapon", "Shoot", "PlayerCamera", "Player", "Creature", "Animator", "Attachments", "FirePoint", "Renderer", "Bounds", "GetBoneTransform", "bounds", "WASD", "Ctrl", "Shift", "_projSpeed"}
residual = []
for text in quoted:
    if "{" in text or "}" in text:
        continue
    visible = re.sub(r"\{[^{}]*\}", "", text)
    if not re.search(r"[A-Za-z]{3,}", visible):
        continue
    if any(item in visible for item in allowed):
        continue
    if text.startswith(("type ", "field ", "method ")):
        continue
    residual.append(text)
assert not residual, residual[:20]

# 8. DLL 汉化器包含本次新增控件及格式化文本。
for phrase in ("Instant hit", "Ballistic prediction", "Tracking chance", "Visible aim assist", "Aim target mode", "Lock radius circle"):
    assert repr(phrase) in localizer or f"'{phrase}'" in localizer
assert "ROOT / 'artifacts' / 'CompleteCheatMenu.dll'" in localizer
assert "missing type marker" in localizer
assert "DST.with_suffix('.localization.diff.json')" in localizer
assert "PROTECTED_RUNTIME = protected_runtime_strings()" in localizer
assert "if s in PROTECTED_RUNTIME" in localizer
for runtime_identifier in ("Heal", "Value", "Boss"):
    assert runtime_identifier in localizer or "protected_runtime_strings" in localizer

for title in ("玩家", "武器", "移动", "船只", "传送", "生成", "实体", "物品", "外观", "金钱", "世界", "击杀分数", "游戏进度", "赌博", "钓鱼", "视觉", "按键", "设置", "诊断"):
    assert f'Title = "{title}"' in menu

print("LOCALIZED_UI_PRESET_TESTS=PASS cases=9")
