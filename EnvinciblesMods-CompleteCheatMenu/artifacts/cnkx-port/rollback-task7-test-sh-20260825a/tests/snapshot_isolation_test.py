from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


@dataclass(frozen=True)
class Record:
    instance_id: int
    kind: str


class Registry:
    def __init__(self):
        self.creatures = [Record(1, "creature"), Record(2, "creature")]
        self.players = [Record(3, "player"), Record(4, "player")]
        self.items = [Record(5, "item"), Record(6, "item")]
        self.containers = [Record(7, "container")]


class EspSnapshotModel:
    def __init__(self, registry):
        self.registry = registry
        self.creatures = []
        self.players = []
        self.items = []
        self.containers = []

    def refresh(self):
        self.creatures[:] = self.registry.creatures
        self.players[:] = self.registry.players
        self.items[:] = self.registry.items
        self.containers[:] = self.registry.containers

    def counts(self):
        return (len(self.creatures), len(self.players), len(self.items), len(self.containers))


class TargetingSnapshotModel:
    def __init__(self, registry):
        self.registry = registry
        self.creatures = ()
        self.players = ()

    def refresh(self, creatures, players):
        self.creatures = self.registry.creatures if creatures else ()
        self.players = self.registry.players if players else ()


registry = Registry()
esp = EspSnapshotModel(registry)
targeting = TargetingSnapshotModel(registry)
esp.refresh()
before = esp.counts()

targeting.refresh(creatures=False, players=True)
assert len(targeting.players) == 2
assert len(targeting.creatures) == 0
assert esp.counts() == before == (2, 2, 2, 1)

targeting.refresh(creatures=True, players=False)
assert len(targeting.creatures) == 2
assert len(targeting.players) == 0
assert esp.counts() == before
assert (len(registry.creatures), len(registry.players), len(registry.items), len(registry.containers)) == before

visual = (ROOT / "decompiled-src/CompleteCheatMenu/Cheats/VisualCheats.cs").read_text(encoding="utf-8")
weapon = (ROOT / "decompiled-src/CompleteCheatMenu/Cheats/WeaponCheats.cs").read_text(encoding="utf-8")
esp_builder = (ROOT / "decompiled-src/CompleteCheatMenu/Runtime/EspSnapshotBuilder.cs").read_text(encoding="utf-8")
target_snapshot = (ROOT / "decompiled-src/CompleteCheatMenu/Targeting/TargetingSnapshot.cs").read_text(encoding="utf-8")

assert "EspSnapshotBuilder.Refresh" in visual
assert "internal static IReadOnlyList<EspTarget> Targets => _targets" in visual
assert "VisualCheats.Rescan(creatures: false" not in weapon
assert "VisualCheats.Targets" not in weapon
assert "_targetingSnapshot.Refresh" in weapon
assert "EntityRegistry.Creatures" in target_snapshot
assert "EntityRegistry.Players" in target_snapshot
assert "EntityRegistry.Items" in esp_builder
assert "EntityRegistry.Containers" in esp_builder

print("SNAPSHOT_ISOLATION_TESTS=PASS cases=7")
