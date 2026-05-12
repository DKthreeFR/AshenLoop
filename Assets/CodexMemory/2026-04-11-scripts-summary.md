# 今日脚本实现说明（简版）

## CharacterInputAdapter.cs

### 1) 输入资产生命周期管理
- 在 `Awake` 创建 `GameInputAction` 并挂接 `Move` Action Map 回调。
- 在启用/禁用时按配置自动启停输入。
- 在销毁时解绑回调、禁用并释放输入资产，避免资源泄漏。

### 2) 持续态输入缓存
- 维护 `MoveInput`、`IsRunHeld`、`IsJumpHeld` 三类可持续读取状态。
- 提供 `ReadMoveInput()` 给上层主动拉取输入。
- `DisableInput()` 时清理移动、跑步、跳跃状态，避免残留。

### 3) 瞬时事件分发
- 把攻击、闪避、交互、跳跃等瞬时操作通过事件对外广播。
- `Attack` 与 `Interact` 在 `performed` 时触发。
- `DodgeRun` 通过 `Tap/Hold` 区分“闪避触发”与“跑步按住状态”。
- `Jump` 在 `performed` 时触发 `JumpPressed`，在 `canceled` 时清除按住态。

### 4) 移动输入整形
- 对移动向量做死区过滤。
- 可选归一化，避免对角线速度异常。
- 仅在值变化时触发 `MoveInputChanged`，减少无效事件。

## Test.cs

### 1) 引用与自动获取
- 支持 Inspector 手动指定 `CharacterInputAdapter`。
- 若未指定，`Awake` 自动从同物体获取。

### 2) 事件日志监听
- 在 `OnEnable` 订阅输入事件，输出调试日志。
- 包含 Move、Attack、Dodge、RunState、Interact、Jump 的日志开关。

### 3) 周期状态观测
- 可按固定间隔输出持续态信息（如 Move/Run），用于排查状态异常。

### 4) 事件处理函数
- 为每个输入事件提供独立处理函数，按开关控制是否打印，便于逐项验证输入链路。
