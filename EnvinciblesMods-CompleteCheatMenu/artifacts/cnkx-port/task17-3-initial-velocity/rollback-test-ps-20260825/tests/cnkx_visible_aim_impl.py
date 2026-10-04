import math


def simulate_error(initial_error, response, fps, seconds, firing):
    if not firing:
        return initial_error
    error = float(initial_error)
    delta_time = 1.0 / fps
    alpha = 1.0 - math.exp(-max(0.0, response) * delta_time)
    for _ in range(round(fps * seconds)):
        error *= 1.0 - alpha
    return error


def apply_offsets(
    point,
    distance,
    distance_drop,
    recoil,
    recoil_strength,
    zero_recoil,
):
    adjusted = (
        point[0],
        point[1] - max(0.0, distance) * max(0.0, distance_drop),
        point[2],
    )
    if zero_recoil or recoil_strength <= 0.0:
        return {"point": adjusted, "pitch_comp": 0.0, "yaw_comp": 0.0}
    return {
        "point": adjusted,
        "pitch_comp": recoil[1] * recoil_strength,
        "yaw_comp": -recoil[0] * recoil_strength,
    }
