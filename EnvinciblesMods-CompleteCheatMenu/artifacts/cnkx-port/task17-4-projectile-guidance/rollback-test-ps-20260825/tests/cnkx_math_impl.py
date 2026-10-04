import math


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


def sanitize_velocity(velocity, max_speed):
    if max_speed <= 0 or not all(math.isfinite(value) for value in velocity):
        return (0.0, 0.0, 0.0)
    magnitude_squared = sum(value * value for value in velocity)
    if magnitude_squared > max_speed * max_speed:
        return (0.0, 0.0, 0.0)
    return tuple(float(value) for value in velocity)


def predict_point(origin, current_point, velocity, projectile_speed):
    if not math.isfinite(projectile_speed) or projectile_speed <= 0:
        return tuple(current_point)
    velocity = sanitize_velocity(velocity, 250.0)
    flight_time = math.dist(origin, current_point) / projectile_speed
    for _ in range(2):
        estimate = tuple(
            current_point[index] + velocity[index] * flight_time
            for index in range(3)
        )
        flight_time = math.dist(origin, estimate) / projectile_speed
    return tuple(
        current_point[index] + velocity[index] * flight_time
        for index in range(3)
    )


def should_track(percent, sample):
    probability = min(1.0, max(0.0, percent / 100.0))
    if probability <= 0.0:
        return False
    if probability >= 1.0:
        return True
    return sample < probability
