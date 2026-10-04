import math
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT))

try:
    from cnkx_math_impl import predict_point, sanitize_velocity
except (ImportError, ModuleNotFoundError):
    print("BALLISTIC_PREDICTION_TESTS=FAIL missing implementation")
    raise SystemExit(1)


def vector_close(actual, expected, tolerance=1e-4):
    assert len(actual) == len(expected)
    for value, wanted in zip(actual, expected):
        assert math.isclose(value, wanted, rel_tol=tolerance, abs_tol=tolerance), (
            actual,
            expected,
        )


origin = (0.0, 0.0, 0.0)

# 静止目标：预测点保持当前点。
vector_close(
    predict_point(origin, (100.0, 0.0, 0.0), (0.0, 0.0, 0.0), 100.0),
    (100.0, 0.0, 0.0),
)

# 横向移动：100m 距离、100m/s 弹速、10m/s 横移，二次迭代后略大于 10m 提前量。
lateral = predict_point(origin, (100.0, 0.0, 0.0), (0.0, 10.0, 0.0), 100.0)
assert lateral[0] == 100.0
assert 10.0 < lateral[1] < 10.1

# 接近目标的提前点应比当前位置更靠近射手。
approaching = predict_point(origin, (100.0, 0.0, 0.0), (-10.0, 0.0, 0.0), 100.0)
assert 90.0 < approaching[0] < 91.0

# 远离目标的提前点应比当前位置更远。
receding = predict_point(origin, (100.0, 0.0, 0.0), (10.0, 0.0, 0.0), 100.0)
assert 111.0 < receding[0] < 112.0

# 零弹速：关闭预判并返回当前点。
vector_close(
    predict_point(origin, (100.0, 5.0, 2.0), (30.0, 0.0, 0.0), 0.0),
    (100.0, 5.0, 2.0),
)

# 异常速度：超过阈值视为传送样本并清零。
vector_close(sanitize_velocity((5000.0, 0.0, 0.0), 250.0), (0.0, 0.0, 0.0))
vector_close(sanitize_velocity((30.0, 40.0, 0.0), 250.0), (30.0, 40.0, 0.0))

print("BALLISTIC_PREDICTION_TESTS=PASS cases=7")
