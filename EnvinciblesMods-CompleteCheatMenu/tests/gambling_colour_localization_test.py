import json
import subprocess
import sys
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
LOCALIZER = ROOT / "tools" / "localize_dll.py"
RAW_DLL = ROOT / "artifacts" / "cnkx-port" / "task17-5-aim-overlay" / "CompleteCheatMenu.task17-5.dll"

source = LOCALIZER.read_text(encoding="utf-8")
assert "SEMANTIC_RUNTIME_STRINGS" in source
for colour in ("Red", "Black", "Green"):
    assert repr(colour) in source or f'"{colour}"' in source
assert "protected.update(SEMANTIC_RUNTIME_STRINGS)" in source

with tempfile.TemporaryDirectory() as directory:
    localized = Path(directory) / "CompleteCheatMenu.localized.dll"
    completed = subprocess.run(
        [sys.executable, str(LOCALIZER), str(RAW_DLL), str(localized)],
        check=True,
        capture_output=True,
        text=True,
    )
    changes = json.loads(localized.with_suffix(".localization.diff.json").read_text(encoding="utf-8"))["changed"]
    translated_runtime_colours = [change for change in changes if change["original"] in {"Red", "Black", "Green"}]
    assert not translated_runtime_colours, translated_runtime_colours
    assert localized.exists() and localized.stat().st_size == RAW_DLL.stat().st_size
    assert '"changed"' in completed.stdout

print("GAMBLING_COLOUR_LOCALIZATION_TEST=PASS cases=9")
