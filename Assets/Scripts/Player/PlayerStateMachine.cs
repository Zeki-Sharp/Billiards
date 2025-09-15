using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 玩家状态机 - 管理玩家的状态转换和逻辑
/// 
/// 【核心职责】：
/// - 管理玩家的三种状态：Idle（空闲）、Charging（蓄力）、Moving（运动）
/// - 处理状态间的转换逻辑和条件判断
/// - 协调PlayerCore和AimController的UI显示
/// - 通过事件与其他系统通信，避免直接耦合
/// 
/// 【状态定义】：
/// - Idle: 可以移动和开始蓄力
/// - Charging: 不能移动，显示瞄准线，更新蓄力进度
/// - Moving: 物理发射移动中，不能进行任何操作
/// 
/// 【设计原则】：
/// - 单一职责：只管理玩家状态，不处理具体业务逻辑
/// - 状态驱动：根据当前状态决定允许的操作
/// - 事件通信：通过MM事件系统与其他状态机通信
/// - 状态独立：不直接访问其他状态机的内部状态
/// </summary>
public class PlayerStateMachine : MonoBehaviour, MMEventListener<GameStateEvent>
{
    /// <summary>
    /// 玩家状态枚举
    /// </summary>
    public enum PlayerState
    {
        Idle,        // 空闲状态：可以移动、可以开始蓄力
        Charging,    // 蓄力状态：不能移动、显示瞄准线、更新蓄力进度
        Moving       // 运动状态：物理发射移动中，不能进行任何操作
    }
    
    [Header("状态设置")]
    [SerializeField] private PlayerState initialState = PlayerState.Idle;
    [SerializeField] private bool showDebugInfo = true;
    
    // 当前状态
    private PlayerState currentState;
    
    // 组件引用
    private PlayerCore playerCore;
    private AimController aimController;
    
    // 事件
    public System.Action<PlayerState, PlayerState> OnStateChanged;
    
    void Start()
    {
        // 获取组件引用
        playerCore = GetComponent<PlayerCore>();
        aimController = FindFirstObjectByType<AimController>();
        
        // 初始化状态
        currentState = initialState;
        EnterState(currentState);
        
        if (showDebugInfo)
        {
            Debug.Log($"PlayerStateMachine: 初始化完成，初始状态: {currentState}");
        }
    }
    
    void OnEnable()
    {
        // 注册事件监听
        this.MMEventStartListening<GameStateEvent>();
    }
    
    void OnDisable()
    {
        // 注销事件监听
        this.MMEventStopListening<GameStateEvent>();
    }
    
    void Update()
    {
        UpdateCurrentState();
    }
    
    #region 状态管理
    
    /// <summary>
    /// 切换到指定状态
    /// </summary>
    public void SwitchToState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        PlayerState oldState = currentState;
        ExitState(oldState);
        currentState = newState;
        EnterState(newState);
        
        // 触发状态变化事件
        TriggerStateChangeEvent(oldState, newState);
        
        OnStateChanged?.Invoke(newState, oldState);
        
        if (showDebugInfo)
        {
            Debug.Log($"PlayerStateMachine: 状态切换 {oldState} -> {newState}");
        }
    }
    
    /// <summary>
    /// 触发状态变化事件
    /// </summary>
    void TriggerStateChangeEvent(PlayerState fromState, PlayerState toState)
    {
        string fromStateName = fromState.ToString();
        string toStateName = toState.ToString();
        string stateType = toStateName;
        
        // 根据目标状态设置能力标志
        bool canMove = toState == PlayerState.Idle;
        bool canCharge = toState == PlayerState.Idle;
        bool isPhysicsMoving = toState == PlayerState.Moving;
        
        // 触发MM事件
        EventTrigger.PlayerStateChanged(fromStateName, toStateName, stateType, canMove, canCharge, isPhysicsMoving);
        
        // 根据状态变化触发相应的游戏流程事件
        if (toState == PlayerState.Charging && fromState == PlayerState.Idle)
        {
            // 从空闲到蓄力：请求进入蓄力状态
            EventTrigger.RequestChargingState();
        }
        else if (toState == PlayerState.Moving && fromState == PlayerState.Charging)
        {
            // 从蓄力到移动：GameFlow不变，不需要触发事件
            // GameFlow保持Charging状态
        }
        else if (toState == PlayerState.Idle && fromState == PlayerState.Moving)
        {
            // 从移动到空闲：请求进入过渡状态
            EventTrigger.RequestTransitionState();
        }
    }
    
    /// <summary>
    /// 更新当前状态逻辑
    /// </summary>
    void UpdateCurrentState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                UpdateIdleState();
                break;
            case PlayerState.Charging:
                UpdateChargingState();
                break;
            case PlayerState.Moving:
                UpdateMovingState();
                break;
        }
    }
    
    /// <summary>
    /// 退出状态
    /// </summary>
    void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                // 清理空闲状态
                break;
            case PlayerState.Charging:
                // 清理蓄力状态
                if (playerCore != null)
                {
                    playerCore.ResetCharging();
                }
                break;
            case PlayerState.Moving:
                // 清理运动状态
                break;
        }
    }
    
    /// <summary>
    /// 进入状态
    /// </summary>
    void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                // 进入空闲状态
                break;
            case PlayerState.Charging:
                // 进入蓄力状态
                if (playerCore != null)
                {
                    playerCore.StartCharging();
                }
                
                // 显示蓄力UI
                if (aimController != null)
                {
                    aimController.ShowChargingUI();
                }
                break;
            case PlayerState.Moving:
                // 进入运动状态
                // 状态变化会通过事件通知GameFlowController
                break;
        }
    }
    
    #endregion
    
    #region 状态逻辑
    
    /// <summary>
    /// 更新空闲状态
    /// </summary>
    void UpdateIdleState()
    {
        // 检查是否在物理移动（排除WASD微调移动）
        if (playerCore != null && playerCore.IsPhysicsMoving() && !playerCore.IsMicroMoving())
        {
            SwitchToState(PlayerState.Moving);
        }
    }
    
    /// <summary>
    /// 更新蓄力状态
    /// </summary>
    void UpdateChargingState()
    {
        if (playerCore != null)
        {
            // 更新蓄力进度
            float chargingProgress = playerCore.GetChargingProgress();
            
            // 通知AimController更新UI
            if (aimController != null)
            {
                aimController.UpdateChargingUI(chargingProgress);
            }
        }
    }
    
    /// <summary>
    /// 更新运动状态
    /// </summary>
    void UpdateMovingState()
    {
        // 运动状态由物理系统处理，这里不需要额外逻辑
        // 状态转换由 PlayerCore 的 OnBallStopped 事件处理
    }
    
    #endregion
    
    #region 外部接口
    
    /// <summary>
    /// 开始蓄力（由输入系统调用）
    /// </summary>
    public void StartCharging()
    {
        if (currentState == PlayerState.Idle)
        {
            SwitchToState(PlayerState.Charging);
        }
    }
    
    /// <summary>
    /// 发射蓄力（由输入系统调用）
    /// </summary>
    public void LaunchCharged()
    {
        if (currentState == PlayerState.Charging)
        {
            // 隐藏蓄力UI
            if (aimController != null)
            {
                aimController.HideChargingUI();
            }
            
            // 发射
            if (playerCore != null)
            {
                playerCore.LaunchCharged();
            }
            
            // 切换到运动状态
            SwitchToState(PlayerState.Moving);
        }
    }
    
    /// <summary>
    /// 球停止运动（由PlayerCore调用）
    /// </summary>
    public void OnBallStopped()
    {
        if (currentState == PlayerState.Moving)
        {
            SwitchToState(PlayerState.Idle);
        }
    }
    
    #endregion
    
    #region 事件处理
    
    /// <summary>
    /// 处理MM事件
    /// </summary>
    public void OnMMEvent(GameStateEvent gameEvent)
    {
        switch (gameEvent.StateName)
        {
            case "RequestCharging":
                // 处理蓄力请求（由PlayerInputHandler触发）
                if (currentState == PlayerState.Idle)
                {
                    StartCharging();
                }
                break;
                
            case "ForceIdle":
                // 强制切换到空闲状态（由GameFlowController触发）
                if (currentState != PlayerState.Idle)
                {
                    SwitchToState(PlayerState.Idle);
                }
                break;
        }
    }
    
    #endregion
    
    #region 公共属性
    
    /// <summary>
    /// 当前玩家状态
    /// </summary>
    public PlayerState CurrentState => currentState;
    
    /// <summary>
    /// 是否正在蓄力
    /// </summary>
    public bool IsCharging => currentState == PlayerState.Charging;
    
    /// <summary>
    /// 是否正在移动
    /// </summary>
    public bool IsMoving => currentState == PlayerState.Moving;
    
    /// <summary>
    /// 是否空闲
    /// </summary>
    public bool IsIdle => currentState == PlayerState.Idle;
    
    #endregion
}
