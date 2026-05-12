using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

/// <summary>
/// 输入层适配器（对接 GameInputAction）。
/// 
/// 作用：
/// 1. 统一接收新输入系统回调。
/// 2. 维护可持续读取的输入状态（MoveInput / IsRunHeld）。
/// 3. 在关键输入发生时通过事件通知上层（Attack/Dodge/Interact等）。
/// </summary>
public class CharacterInputAdapter : MonoBehaviour, GameInputAction.IMoveActions
{
    // ===== 可在 Inspector 调整的输入参数 =====

    [Header("Input")]
    [SerializeField] private bool autoEnableOnEnable = true;   // 组件启用时自动 Enable 输入
    [SerializeField] private bool normalizeMoveInput = true;    // 是否把移动向量归一化（避免斜向更快）
    [SerializeField] private float moveDeadZone = 0.05f;        // 死区阈值（小于该值视为 0）

    // ===== 对外暴露的只读状态 =====

    /// 当前移动输入向量（持续状态，可随时读取）。
    public Vector2 MoveInput => _moveInput;

    /// 是否存在有效移动输入（MoveInput 非零）。
    public bool HasMoveInput => _moveInput.sqrMagnitude > 0f;

    /// 是否处于“长按跑步”状态。
    public bool IsRunHeld => _isRunHeld;
    //是否处于跳跃状态
    public bool IsJumpHeld => _isJumpHeld;
    
    // ===== 对外事件（瞬时信号）=====

    /// 移动向量变化时触发。
    public event Action<Vector2> MoveInputChanged;

    /// 攻击键触发时触发（performed）。
    public event Action AttackPressed;

    /// 闪避触发时触发（DodgeRun 的 Tap/performed）。
    public event Action DodgePressed;

    /// 跑步状态变化时触发（true=开始跑，false=停止跑）。
    public event Action<bool> RunStateChanged;

    /// 交互键触发时触发（performed）。
    public event Action InteractPressed;

    /// 跳跃键触发时触发（performed）。
    public event Action JumpPressed;


    // ===== 私有运行时字段 =====

    private GameInputAction _inputActions;  // 新输入系统自动生成的输入资产
    private Vector2 _moveInput;             // 缓存的当前移动输入
    private bool _isRunHeld;                // 缓存的当前跑步按住状态
    private bool _isJumpHeld;               // 缓存的当前跳跃按住状态

    // 通过平方比较避免频繁开方，提高一点点性能
    private float MoveDeadZoneSqr => moveDeadZone * moveDeadZone;

    /// <summary>
    /// 初始化输入资产并绑定回调。
    /// </summary>
    private void Awake()
    {
        _inputActions = new GameInputAction();
        _inputActions.Move.SetCallbacks(this);
    }

    /// <summary>
    /// 组件启用时按配置自动启用输入。
    /// </summary>
    private void OnEnable()
    {
        if (autoEnableOnEnable)
        {
            EnableInput();
        }
        //玩家按下交互键时触发事件 NPC自己监听
        _inputActions.Move.Interact.performed += OnInteract;
    }

    /// <summary>
    /// 组件禁用时按配置自动关闭输入并重置状态。
    /// </summary>
    private void OnDisable()
    {
        if (autoEnableOnEnable)
        {
            DisableInput();
            _inputActions.Move.Interact.performed -= OnInteract;
        }
    }

    /// <summary>
    /// 销毁前解绑回调并释放输入资产，避免泄漏。
    /// </summary>
    private void OnDestroy()
    {
        if (_inputActions == null)
        {
            return;
        }

        _inputActions.Move.SetCallbacks(null);
        _inputActions.Disable();
        _inputActions.Dispose();
        _inputActions = null;
    }

    /// <summary>
    /// 手动启用输入资产。
    /// </summary>
    public void EnableInput()
    {
        _inputActions?.Enable();
    }

    /// <summary>
    /// 手动关闭输入资产，并把状态重置，防止禁用后残留移动/跑步状态。
    /// </summary>
    public void DisableInput()
    {
        _inputActions?.Disable();
        SetMoveInput(Vector2.zero);
        SetRunHeld(false);
        SetJumpHeld(false);
    }

    /// <summary>
    /// 给上层“主动拉取”输入的接口。
    /// </summary>
    public Vector2 ReadMoveInput()
    {
        return _moveInput;
    }

    /// <summary>
    /// Move 回调：读取 Vector2 并写入缓存。
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        SetMoveInput(context.ReadValue<Vector2>());
    }

    /// <summary>
    /// Attack 回调：按下触发（performed）。
    /// </summary>
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttackPressed?.Invoke();
        }
    }

    /// <summary>
    /// DodgeRun 回调：
    /// - Tap/performed -> 闪避
    /// - Hold/started 或 performed -> 开始跑步
    /// - Hold/canceled -> 结束跑步
    /// </summary>
    public void OnDodgeRun(InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
        {
            if (context.performed)
            {
                DodgePressed?.Invoke();
            }

            return;
        }

        if (context.interaction is HoldInteraction)
        {
            if (context.started || context.performed)
            {
                SetRunHeld(true);
            }
            else if (context.canceled)
            {
                SetRunHeld(false);
            }
        }
    }

    /// <summary>
    /// Interact 回调：按下触发（performed）。
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractPressed?.Invoke();
        }
    }
/// <summary>
    /// Interact 回调：按下触发（performed）。
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetJumpHeld(true);
            JumpPressed?.Invoke();
        }
        else if (context.canceled)
        {
            SetJumpHeld(false);            
        }
    }
    /// <summary>
    /// 写入移动输入：
    /// 1) 先做死区过滤；
    /// 2) 可选归一化；
    /// 3) 值变化时才广播事件。
    /// </summary>
    private void SetMoveInput(Vector2 rawValue)
    {
        var value = rawValue;

        if (value.sqrMagnitude <= MoveDeadZoneSqr)
        {
            value = Vector2.zero;
        }
        else if (normalizeMoveInput && value.sqrMagnitude > 1f)
        {
            value = value.normalized;
        }

        if (_moveInput == value)
        {
            return;
        }

        _moveInput = value;
        MoveInputChanged?.Invoke(_moveInput);
    }

    /// <summary>
    /// 更新跑步状态并在变化时通知外部。
    /// </summary>
    private void SetRunHeld(bool value)
    {
        if (_isRunHeld == value)
        {
            return;
        }

        _isRunHeld = value;
        RunStateChanged?.Invoke(_isRunHeld);
    }
    private void SetJumpHeld(bool value)
    {
        if (_isJumpHeld == value)
        {
            return;
        }

        _isJumpHeld = value;
    }
    
}
