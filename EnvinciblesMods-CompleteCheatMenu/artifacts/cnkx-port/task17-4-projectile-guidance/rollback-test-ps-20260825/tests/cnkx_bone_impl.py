import math


def _classify(name):
    value = name.lower()
    if any(token in value for token in ("head", "skull", "neck")):
        return "head"
    if any(token in value for token in ("chest", "spine", "body", "torso")):
        return "chest"
    if any(token in value for token in ("pelvis", "hips", "hip")):
        return "pelvis"
    return None


def _append_unique(points, label, position, source):
    if position is None or any(point["position"] == position for point in points):
        return
    points.append({"label": label, "position": position, "source": source})


def resolve_points(kind, humanoid, named, skinned, bounds, root, fixed_height):
    points = []
    if kind == "player":
        for label in ("head", "chest", "pelvis"):
            _append_unique(points, label, humanoid.get(label), "animator")
    else:
        classified = {}
        for name, position in named.items():
            label = _classify(name)
            if label is not None and label not in classified:
                classified[label] = position
        for label in ("head", "chest", "pelvis"):
            _append_unique(points, label, classified.get(label), "named-bone")
        classified = {}
        for name, position in skinned:
            label = _classify(name)
            if label is not None and label not in classified:
                classified[label] = position
        for label in ("head", "chest", "pelvis"):
            if not any(point["label"] == label for point in points):
                _append_unique(points, label, classified.get(label), "skinned-bone")
    if bounds is not None:
        center, size = bounds
        upper = (center[0], center[1] + size[1] * 0.25, center[2])
        _append_unique(points, "bounds-upper", upper, "renderer-bounds")
        _append_unique(points, "body-center", center, "renderer-bounds")
    fixed = (root[0], root[1] + fixed_height, root[2])
    _append_unique(points, "fixed-height", fixed, "root-offset")
    return points


def select_closest_projected(points, center):
    best = None
    best_distance = float("inf")
    for point in points:
        screen = point.get("screen")
        if screen is None or len(screen) != 3 or screen[2] <= 0:
            continue
        if not all(math.isfinite(value) for value in screen):
            continue
        distance = math.hypot(screen[0] - center[0], screen[1] - center[1])
        if distance < best_distance:
            best = dict(point)
            best["screen_distance"] = distance
            best_distance = distance
    return best
