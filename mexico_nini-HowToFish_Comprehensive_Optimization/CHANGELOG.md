### v1.5.4
#### 中文
- 程序集版本号与发布版本号同步为 1.5.4.0
- 依赖格式改为 Thunderstore 标准（BepInEx-BepInExPack-5.4.2305）
- 配置文件更名为 HowToFish.ComprehensivePerformanceOptimization.cfg
#### English
- Assembly version synchronized with release version to 1.5.4.0
- Dependency format changed to Thunderstore standard (BepInEx-BepInExPack-5.4.2305)
- Config file renamed to HowToFish.ComprehensivePerformanceOptimization.cfg

### v1.5.1
#### 中文
- 新增岛屿早加载，减轻跨岛卡顿（默认开启，触发距离 ×2）
#### English
- Added island early load to reduce island-crossing stutter (on by default, trigger distance ×2)

### v1.0.0
#### 中文
- 首次发布，整合 JIT 预热、日志削减、温和垃圾清理
#### English
- Initial release combining JIT warmup, log trim, and gentle GC

### 更早版本（已废弃）
#### 中文
- 移除地图加载资源清理、帧率上限、着色器预热
- 保留核心功能：JIT 预热、日志削减、温和垃圾清理、岛屿早加载
#### English
- Removed map-load resource sweep, frame-rate cap, and shader warmup
- Kept core features: JIT warmup, log trim, gentle GC, and island early load