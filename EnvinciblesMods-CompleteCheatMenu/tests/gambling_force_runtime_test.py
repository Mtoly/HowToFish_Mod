from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
cheats = (ROOT / "decompiled-src" / "CompleteCheatMenu" / "Cheats" / "GamblingCheats.cs").read_text(encoding="utf-8")
patches = "\n".join(p.read_text(encoding="utf-8") for p in (ROOT / "decompiled-src" / "CompleteCheatMenu" / "Patches").glob("*.cs"))
assert "OverrideRouletteResult" in cheats, "force colour must override authoritative result"
assert "ServerRouletteResult" in patches and "OverrideRouletteResult" in patches, "roulette result patch missing"
assert "RigMode != Mode.ForceColour" in cheats, "override must be gated by force-colour mode"
print("GAMBLING_FORCE_RUNTIME_TEST=PASS")
