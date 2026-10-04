from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REGISTRY_SOURCE = ROOT / "decompiled-src/CompleteCheatMenu/Runtime/EntityRegistry.cs"
RECORD_SOURCE = ROOT / "decompiled-src/CompleteCheatMenu/Runtime/EntityRecord.cs"
CREATURE_PATCH = ROOT / "decompiled-src/CompleteCheatMenu/Patches/CreatureRegistry_Patch.cs"
PLAYER_PATCH = ROOT / "decompiled-src/CompleteCheatMenu/Patches/PlayerRegistry_Patch.cs"
ADAPTER_SOURCE = ROOT / "decompiled-src/CompleteCheatMenu/Cheats/AimTargetRegistry.cs"


@dataclass
class Record:
    instance_id: int
    name: str
    generation: int
    alive: bool = True


class RegistryModel:
    def __init__(self):
        self.records = []
        self.generation = 0
        self.next_refresh = 0.0

    def register(self, instance_id, name):
        for record in self.records:
            if record.instance_id == instance_id:
                record.name = name
                record.generation = self.generation
                return record
        record = Record(instance_id, name, self.generation)
        self.records.append(record)
        return record

    def refresh(self, now, interval, entities):
        if now < self.next_refresh:
            return False
        self.next_refresh = now + interval
        self.generation += 1
        if entities is None:
            self.prune_destroyed()
            return True
        for instance_id, name in entities:
            self.register(instance_id, name)
        for index in range(len(self.records) - 1, -1, -1):
            record = self.records[index]
            if not record.alive or record.generation != self.generation:
                self.records.pop(index)
        return True

    def prune_destroyed(self):
        for index in range(len(self.records) - 1, -1, -1):
            if not self.records[index].alive:
                self.records.pop(index)


# 注册和去重：相同稳定 ID 不增加记录，并刷新名称。
model = RegistryModel()
first = model.register(100, "old")
second = model.register(100, "new")
assert first is second
assert len(model.records) == 1
assert model.records[0].name == "new"

# 倒序清理：连续死亡记录不会因为 RemoveAt 导致跳项。
model.register(101, "dead-a").alive = False
model.register(102, "dead-b").alive = False
model.register(103, "alive")
model.prune_destroyed()
assert [record.instance_id for record in model.records] == [100, 103]

# 代次刷新：本轮未出现但仍存活的旧记录也会被移除。
assert model.refresh(1.0, 0.1, [(103, "alive-renamed"), (104, "new")])
assert [(record.instance_id, record.name) for record in model.records] == [
    (103, "alive-renamed"),
    (104, "new"),
]

# 独立刷新时间：玩家刷新不会推进物品或容器的时间门。
players = RegistryModel()
items = RegistryModel()
containers = RegistryModel()
assert players.refresh(1.0, 0.1, [(1, "player")])
assert not players.refresh(1.05, 0.1, [(1, "player")])
assert items.refresh(1.05, 0.5, [(2, "item")])
assert containers.refresh(1.05, 0.5, [(3, "container")])

# 暂时取不到管理器集合时只清理已销毁对象，不清空仍存活的缓存。
items.records[0].alive = True
assert items.refresh(2.0, 0.5, None)
assert [record.instance_id for record in items.records] == [2]

# 源码结构检查：四个集合、三个独立时间门、倒序清理、无 LINQ。
registry = REGISTRY_SOURCE.read_text(encoding="utf-8")
record = RECORD_SOURCE.read_text(encoding="utf-8")
creature_patch = CREATURE_PATCH.read_text(encoding="utf-8")
player_patch = PLAYER_PATCH.read_text(encoding="utf-8")
adapter = ADAPTER_SOURCE.read_text(encoding="utf-8")

for field in ("_creatures", "_players", "_items", "_containers"):
    assert f"List<EntityRecord> {field}" in registry
for field in ("_nextPlayerRefresh", "_nextItemRefresh", "_nextContainerRefresh"):
    assert field in registry
assert "for (int i = records.Count - 1; i >= 0; i--)" in registry
assert "using System.Linq" not in registry
assert "PruneDestroyed(_players)" in registry and "PruneDestroyed(_items)" in registry
assert "InstanceId" in record and "TypeName" in record and "SeenGeneration" in record
assert "EntityRegistry.RegisterCreature(__instance)" in creature_patch
assert "EntityRegistry.RegisterPlayer(__instance)" in player_patch
assert "EntityRegistry.Creatures" in adapter

print("ENTITY_REGISTRY_TESTS=PASS cases=9")
