# CompleteCheatMenu 游戏内汉化设计

## 范围

仅处理 `CompleteCheatMenu.dll` 中游戏内菜单、标签、按钮、提示、诊断和错误文本；不修改 README、CHANGELOG、manifest 或游戏本体。

## 方案

保留原始 DLL，复制为汉化副本。通过 .NET 元数据读取用户字符串，按可见文本建立集中映射，并仅替换字符串内容，不改变方法体、类型、控制流或资源结构。

## 验证

记录原始 SHA-256、程序集版本、基线字符串统计；修改后验证程序集可加载、映射覆盖率、格式化占位符一致性；回滚脚本在另一副本上恢复原始字节并复算哈希。

## 产物

- `CompleteCheatMenu.zh-CN.dll`
- `CompleteCheatMenu.localization.diff.json`
- `VERIFICATION.txt`
- `ROLLBACK.sh`
