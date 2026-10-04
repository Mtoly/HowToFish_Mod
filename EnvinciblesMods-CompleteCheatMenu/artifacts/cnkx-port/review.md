# Task 14 性能与回归审查

日期：2026-08-25  
审查对象：`Complete Cheat Menu 0.7.0` 的 cnKX 行为移植（Task 3–13）  
结论：**Gate A/B/C 静态与确定性回归通过，可以进入 Task 15 构建部署验证。**

## 门槛结论

```text
GATE_A=PASS
GATE_B=PASS
GATE_C=PASS
P0=0
P1=0
P2=2
READY_FOR_TASK15=True
```

Task 15 仍需完成实际 BepInEx/Harmony 启动日志验证后，Gate D 才能通过。

## 性能结果

性能原始记录：`artifacts/cnkx-port/performance.txt`。

- 固定种子 `935`，300 个候选，每个候选 3–8 个骨骼点，预热 1,000 次，正式执行 10,000 轮。
- 旧语义模型：距离/FOV/可见性过滤后，以最小角度选择固定瞄准点。
- 新语义模型：距离、角度、屏幕距离、切换惩罚与受上限约束的优先奖励组合评分。
- 当新增权重、优先奖励和切换惩罚均不参与时，旧/新模型都选择目标 `17`：`SEMANTIC_BASELINE_MATCH=True`。
- 启用组合评分与优先目标后选择目标 `211`：`ENHANCED_WEIGHTING_CHANGES_RESULT=True`。
- 重复输入得到相同目标，预测值全部有限：`DETERMINISTIC_RESULT=True`、`PREDICTION_FINITE=True`。
- 四个热点在基准循环中仅记录 40 字节测量框架开销；没有随候选数量增长的循环内分配。

注意：该 Harness 隔离测量评分、骨骼点选择和预测数学，不包含 Unity 的 Transform、Animator、Renderer、WorldToScreenPoint 与 Physics.Raycast 成本。Unity 端通过缓存、Repaint 限制和固定刷新预算控制这些成本。

## 正确性与生命周期审查

| 审查项 | 结果 | 证据 |
|---|---|---|
| 空引用 | 通过 | `TargetingSystem.Acquire`、`ShotRedirector.TryRedirect`、`VisibleAimController.Step` 均在入口拒绝空相机、武器、FirePoint、目标或 Transform。 |
| Unity 销毁对象语义 | 通过 | `Game/Refs.cs` 对 `UnityEngine.Object` 使用 Unity 重载的 `obj != null`；`EntityRecord.IsAlive` 同时检查实体与 Transform；注册表每秒清理销毁对象。 |
| 武器原值保存 | 通过 | `WeaponStateStore` 使用引用比较器，首次写字段时保存原值，避免 Unity 对象相等重载造成键冲突。 |
| 关闭开关恢复 | 通过 | `WeaponCheats.RestoreDisabledFields` 对散布、后坐、射速、多发和弹速逐字段恢复。 |
| 武器切换恢复 | 通过 | `WeaponCheats.RestorePreviousWeapon` 在持有武器引用变化时恢复上一把武器。 |
| 插件卸载恢复 | 通过 | `Plugin.OnDestroy` 调用 `WeaponCheats.RestoreAllWeapons`、释放 GUI 纹理并 `UnpatchSelf`。 |
| 共享集合 | 通过 | ESP 使用自己的 `EspSnapshot` 列表；目标系统只借用注册表只读视图，所有读取与刷新均在 Unity 主线程顺序执行，不会由 ESP 清空。 |
| 可见性缓存 | 通过 | 目标缓存 TTL 最低 0.01 秒、默认 0.05 秒，并每秒清理过期键；ESP 可见性 TTL 为 0.05 秒。 |
| 几何缓存 | 通过 | Renderer/Animator 几何缓存 1 秒，过期后重建；缓存清理周期 2 秒，使用 Unity 空对象语义跳过已销毁 Renderer。 |
| 日志速率 | 通过 | `ShotRedirector.LogLimited` 仅在诊断开关开启时输出，下一次日志至少延后 2 秒。 |
| Animator/Avatar | 通过 | 完整人体骨架仅在 Animator 非空、`isHuman`、Avatar 非空且 `isValid` 时使用，否则走 BoneResolver 回退。 |
| GUI 状态 | 通过 | `GuiPrimitives` 的方框、线、圆、血条和标签均在 `finally` 中恢复 `GUI.color` 与 `GUI.matrix`。 |
| Repaint 限制 | 通过 | `EspRendererV2.Draw` 在非 `EventType.Repaint` 事件立即返回。 |

## UI 消费点审查

Task 13 新增或变更的每个设置均存在生产消费点：

| 设置组 | 实际消费点 |
|---|---|
| `AimTargets` | 由 `CheatState.AimTargetsCreatures/AimTargetsPlayers` 派生，`TargetingSystem` 与 `TargetingSnapshot` 消费。 |
| 距离、角度、屏幕半径及三类权重 | `TargetingSettings.FromCheatState` → `TargetScorer`。 |
| 优先奖励、奖励上限、切换惩罚 | `TargetingSettings` → `TargetScorer.ScoreEntity`。 |
| 锁定时长 | `TargetingSystem._lockUntil`。 |
| 死亡、海鸥、上钩鱼、信天翁优先 | `TargetingSystem.CollectCreatures`。 |
| 速度采样间隔、传送距离、最大速度 | `VelocityTracker`；最大速度同时由 `BallisticPredictor` 校验。 |
| 预测、重力补偿、重力倍率 | `ShotRedirector` → `BallisticPredictor`。 |
| 追踪概率、诊断日志 | `ShotRedirector.ShouldTrack/LogLimited`。 |
| 可见拉枪、响应、距离下调、后坐补偿 | `WeaponCheats.VisibleAimTick` → `VisibleAimController`。 |
| 瞬击与弹速 | `WeaponCheats.Tick/ResolveProjectileSpeed`。 |
| ESP 类型选择、距离和锁定圆 | `EspRendererV2.Draw` 与 `EspSnapshotBuilder.Refresh`。 |
| 方框、射线、骨架、血条 | `EspRendererV2.DrawRecord`。 |
| 名称、距离、类型、可见状态 | `EspRendererV2.BuildLabel`。 |
| 手持物、价值、容器信息 | `EspRendererV2.BuildDetails`，绑定或数据缺失时返回空字符串。 |

`Fov` 是界面即时调用 `VisualCheats.SetFov` 的值，不依赖后台轮询；`AimTargets` 是互斥枚举的源字段，因此两者不属于漏接。

## 非阻断发现

### P2-1：BoneResolver 热路径存在短命集合分配

`BoneResolver.Resolve` 每个候选创建 `List<AimPoint>` 和 `HashSet<int>`。当前目标预览被限制为 0.05 秒刷新一次，可见性也有 TTL，因此不会逐渲染帧无限放大；但实体非常多时仍会形成 GC 压力。后续可采用调用方复用缓冲区或对象池降低分配。

### P2-2：VisibilityCache 每次未命中会拼接字符串键

缓存键使用 `point.Bone + ":" + point.Source`，会产生短命字符串。后续可把 `Bone`、`Source` 分别纳入结构键，或在 `AimPoint` 构造时缓存稳定键。

以上两项不改变选择语义、不造成状态泄漏，也未达到 P0/P1 阻断级别。

## 回归结论

- Task 2–13 的确定性测试全部通过。
- 当前源码可由 `decompiled-src/build.ps1` 无错误构建。
- Task 14 不改变插件运行时源码；构建验证生成了新的 PE/MVID，因此开发 DLL 字节哈希与 Task 14 基线不同，但全量语义回归一致。
- 游戏目录 DLL 在本任务中保持原哈希，部署动作继续留给 Task 15。
- Git 状态：`SKIPPED_NOT_A_GIT_REPOSITORY`。
