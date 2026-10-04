from dataclasses import dataclass

@dataclass
class Target:
    name: str
    distance: float
    angle: float
    dead: bool = False
    active: bool = True
    seagull: bool = False
    hooked: bool = False
    albatross: bool = False

def choose(targets, current=None, lock=False):
    candidates = [t for t in targets if t.active and not t.dead and not t.seagull and not t.hooked and .5 <= t.distance <= 200 and t.angle <= 35]
    if lock and current in candidates:
        return current
    def score(t):
        base = t.angle * .2 if t.albatross else t.distance + t.angle * .5
        return base + (8 if current is not None and t is not current else 0)
    return min(candidates, key=score) if candidates else None

near = Target('near', 20, 10)
center = Target('center', 40, 1)
assert choose([near, center]) is near
assert choose([Target('dead', 1, 0, dead=True), center]) is center
assert choose([Target('seagull', 1, 0, seagull=True), center]) is center
assert choose([Target('hooked', 1, 0, hooked=True), center]) is center
assert choose([Target('far', 201, 0), center]) is center
assert choose([Target('outside', 1, 36), center]) is center
albatross = Target('albatross', 500, 5, albatross=True)
assert choose([near, albatross]) is near  # range filtering still wins
albatross.distance = 150
assert choose([near, albatross]) is albatross
assert choose([near, center], current=center, lock=True) is center
print('AIM_SCORING_TESTS=PASS cases=9')
