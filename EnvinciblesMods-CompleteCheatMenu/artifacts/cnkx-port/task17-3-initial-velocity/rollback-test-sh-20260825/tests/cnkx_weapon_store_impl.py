class WeaponStateStoreModel:
    def __init__(self, allowed_fields):
        self.allowed_fields = set(allowed_fields)
        self.entries = {}

    def _entry(self, weapon, create=False):
        key = id(weapon)
        entry = self.entries.get(key)
        if entry is None and create:
            entry = {"weapon": weapon, "originals": {}}
            self.entries[key] = entry
        return entry

    def set(self, weapon, field, value):
        if weapon is None or field not in self.allowed_fields or field not in weapon:
            return False
        entry = self._entry(weapon, create=True)
        if field not in entry["originals"]:
            entry["originals"][field] = weapon[field]
        weapon[field] = value
        return True

    def original(self, weapon, field):
        entry = self._entry(weapon)
        return None if entry is None else entry["originals"].get(field)

    def restore_weapon(self, weapon):
        entry = self.entries.pop(id(weapon), None)
        if entry is None:
            return False
        for field, value in entry["originals"].items():
            weapon[field] = value
        return True

    def restore_field_all(self, field):
        restored = 0
        empty = []
        for key, entry in self.entries.items():
            originals = entry["originals"]
            if field in originals:
                entry["weapon"][field] = originals.pop(field)
                restored += 1
            if not originals:
                empty.append(key)
        for key in empty:
            self.entries.pop(key, None)
        return restored

    def restore_all(self):
        count = len(self.entries)
        for entry in list(self.entries.values()):
            for field, value in entry["originals"].items():
                entry["weapon"][field] = value
        self.entries.clear()
        return count


def projectile_speed(fast_enabled, instant_enabled, fast_speed, instant_speed):
    if instant_enabled:
        return instant_speed
    if fast_enabled:
        return fast_speed
    return None
