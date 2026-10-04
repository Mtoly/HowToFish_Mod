class VisibilityCacheModel:
    def __init__(self, ttl):
        self.ttl = ttl
        self.entries = {}

    def visible(self, entity, point, now, raycast):
        key = (entity, point)
        cached = self.entries.get(key)
        if cached is not None and now <= cached[0]:
            return cached[1]
        result = bool(raycast(entity, point))
        self.entries[key] = (now + self.ttl, result)
        return result

    def hit_matches(self, hit, target, parents):
        if hit == target:
            return True
        current = hit
        while current is not None:
            current = parents.get(current)
            if current == target:
                return True
        current = target
        while current is not None:
            current = parents.get(current)
            if current == hit:
                return True
        return False


class TargetingModel:
    def __init__(self, lock_duration, switch_penalty):
        self.lock_duration = lock_duration
        self.switch_penalty = switch_penalty
        self.current = None
        self.lock_until = 0.0

    def acquire(self, candidates, now):
        valid = [candidate for candidate in candidates if candidate.get("valid", False)]
        current_candidate = next(
            (candidate for candidate in valid if candidate["name"] == self.current),
            None,
        )
        if current_candidate is not None and now < self.lock_until:
            return current_candidate["name"], current_candidate["point"]
        if not valid:
            self.current = None
            self.lock_until = 0.0
            return None

        def weighted(candidate):
            penalty = 0.0
            if self.current is not None and candidate["name"] != self.current:
                penalty = self.switch_penalty
            return candidate["score"] + penalty

        best = min(valid, key=weighted)
        if best["name"] != self.current:
            self.current = best["name"]
            self.lock_until = now + self.lock_duration
        return best["name"], best["point"]
