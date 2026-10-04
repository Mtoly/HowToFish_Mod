from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "decompiled-src" / "CompleteCheatMenu"

gate = (SRC / "Cheats" / "CheatGate.cs").read_text(encoding="utf-8")
widgets = (SRC / "UI" / "Widgets.cs").read_text(encoding="utf-8")
menu = (SRC / "UI" / "MenuWindow.cs").read_text(encoding="utf-8")
plugin = (SRC / "Plugin.cs").read_text(encoding="utf-8")
diagnostics = (SRC / "UI" / "Tabs" / "DiagnosticsTab.cs").read_text(encoding="utf-8")
tabs = list((SRC / "UI" / "Tabs").glob("*Tab.cs"))

# Seam 1: page availability is based on having a local player, not host status.
assert "internal static string PageBlockReason()" in gate
page_body = gate.split("internal static string PageBlockReason()", 1)[1].split("internal static string", 1)[0]
assert "!InGame" in page_body
assert "!IsHost" not in page_body

# Seam 2: server authority remains separately observable for individual actions.
assert "internal static string ServerBlockReason()" in gate
assert "internal static bool LocalFeaturesAvailable => InGame;" in gate
assert "internal static bool ServerFeaturesAvailable => InGame && IsHost;" in gate

# Seam 3: no tab is blanked wholesale merely because this peer is a client.
joined_tabs = "\n".join(path.read_text(encoding="utf-8") for path in tabs)
assert "CheatGate.BlockReason()" not in joined_tabs
assert "Widgets.RequireHost(" not in joined_tabs
for required in ("PlayerTab.cs", "WeaponsTab.cs", "BoatTab.cs", "TeleportTab.cs"):
    text = (SRC / "UI" / "Tabs" / required).read_text(encoding="utf-8")
    assert "Widgets.RequireAvailable(CheatGate.PageBlockReason())" in text, required

# Seam 4: local developer settings are enabled for clients after joining a game.
assert "Refs.InGame && Refs.IsHost" not in plugin
assert "if (Refs.InGame)" in plugin

# Seam 5: menu and diagnostics describe capability rather than disabling tabs.
assert "ServerSide && !Refs.IsHost" not in menu
assert "本地功能可用" in menu
assert "部分操作由房主同步" in menu
assert "本地功能" in diagnostics
assert "服务器写入" in diagnostics
assert "internal static bool RequireAvailable" in widgets

print("CLIENT_MODE_CAPABILITY_TESTS=PASS cases=5")
