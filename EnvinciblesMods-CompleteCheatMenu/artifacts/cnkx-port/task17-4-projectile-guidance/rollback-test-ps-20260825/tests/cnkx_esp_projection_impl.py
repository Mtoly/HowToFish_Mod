import math


def _project(point, screen, vertical_fov):
    x, y, z = point
    if z <= 0.01:
        return None
    width, height = screen
    focal = (height * 0.5) / math.tan(math.radians(vertical_fov) * 0.5)
    return (width * 0.5 + x * focal / z, height * 0.5 - y * focal / z)


def project_bounds(center, extents, screen, vertical_fov):
    corners = []
    for sx in (-1.0, 1.0):
        for sy in (-1.0, 1.0):
            for sz in (-1.0, 1.0):
                projected = _project(
                    (
                        center[0] + extents[0] * sx,
                        center[1] + extents[1] * sy,
                        center[2] + extents[2] * sz,
                    ),
                    screen,
                    vertical_fov,
                )
                if projected is None:
                    return None
                corners.append(projected)
    xs = [point[0] for point in corners]
    ys = [point[1] for point in corners]
    return (min(xs), min(ys), max(xs), max(ys))


def fallback_rect(root, height, screen, vertical_fov, root_up=(0.0, 1.0, 0.0)):
    bottom = _project(root, screen, vertical_fov)
    # 生产实现固定使用世界竖直方向；root_up 只用于验证旋转不会改变结果。
    top = _project((root[0], root[1] + height, root[2]), screen, vertical_fov)
    if bottom is None or top is None:
        return None
    projected_height = abs(bottom[1] - top[1])
    if projected_height <= 0.01:
        return None
    projected_width = projected_height * 0.5
    center_x = (bottom[0] + top[0]) * 0.5
    return (
        center_x - projected_width * 0.5,
        min(bottom[1], top[1]),
        center_x + projected_width * 0.5,
        max(bottom[1], top[1]),
    )
