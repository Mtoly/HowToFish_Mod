def inside_screen_radius(dx, dy, radius):
    if radius < 0:
        return False
    return dx * dx + dy * dy <= radius * radius


def score_entity(
    distance,
    angle,
    screen_distance,
    is_switch,
    distance_weight,
    angle_weight,
    screen_weight,
    switch_penalty,
):
    score = (
        distance * max(0.0, distance_weight)
        + angle * max(0.0, angle_weight)
        + screen_distance * max(0.0, screen_weight)
    )
    if is_switch:
        score += max(0.0, switch_penalty)
    return score


def select_best_bone(bones, require_visible=True):
    best = None
    best_score = float("inf")
    for bone in bones:
        if require_visible and not bone.get("visible", False):
            continue
        score = float(bone.get("screen_distance", float("inf")))
        if score < best_score:
            best = bone
            best_score = score
    return best


def select_target(
    targets,
    current_name,
    distance_weight,
    angle_weight,
    screen_weight,
    switch_penalty,
):
    best = None
    best_score = float("inf")
    for target in targets:
        score = score_entity(
            float(target.get("distance", 0.0)),
            float(target.get("angle", 0.0)),
            float(target.get("screen_distance", 0.0)),
            current_name is not None and target.get("name") != current_name,
            distance_weight,
            angle_weight,
            screen_weight,
            switch_penalty,
        )
        if score < best_score:
            best = target
            best_score = score
    return best
