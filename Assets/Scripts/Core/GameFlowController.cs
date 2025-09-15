using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 游戏流程控制器 - 管理Normal、Charging、Transition三状态
/// 
/// 【核心职责】：
/// - 管理游戏全局流程状态（Normal/Charging/Transition）
/// - 协调时停系统、过渡系统、敌人系统等
/// - 通过事件与Player系统通信，避免直接耦合
/// 
/// 【设计原则】：
/// - 不直接检测玩家输入（由PlayerInputHandler处理）
/// - 不直接访问Player内部组件（通过事件通信）
/// - 专注于游戏流程逻辑，不处理具体的玩家行为
/// </summary>
public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }
    
    /// <summary>
    /// 游戏流程状态枚举
    /// </summary>
    public enum GameFlowState
    {
        Normal,         // 正常游戏状态：玩家移动躲避，敌人移动+射击
        Charging,       // 蓄力时停状态：完全时停，玩家瞄准蓄力
        Transition      // 过渡状态：玩家可移动，敌人和子弹仍时停，白球运动
    }
    
    [Header("流程设置")]
    [SerializeField] private bool enableAutoTransition = true;
    [SerializeField] private float transitionTimeout = 5f; // 过渡状态超时时间
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 当前状态
    private GameFlowState currentState = GameFlowState.Normal;
    
    // 组件引用（由GameInitializer设置）
    private EnergySystem energySystem;
    private TimeStopManager timeStopManager;
    private TransitionManager transitionManager;
    private EnemyManager enemyManager;
    private PlayerStateMachine playerStateMachine;
    private PlayerCore playerCore;
    
    // 状态管理
    private bool hasPlayerLaunched = false;
    
    // 事件（使用MM架构）
    public System.Action<GameFlowState> OnStateChanged;
    public System.Action OnGameStart;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("发现多个GameFlowController实例，销毁重复的实例");
            Destroy(gameObject);
            return;
        }
    }
    
    void Update()
    {
        // 检查状态切换条件
        CheckStateTransitions();
    }
    
    #region 状态管理
    
    /// <summary>
    /// 检查状态切换条件
    /// </summary>
    void CheckStateTransitions()
    {
        switch (currentState)
        {
            case GameFlowState.Normal:
                // 正常状态：不再自动检查进入蓄力状态
                // 蓄力状态切换现在由PlayerInputHandler主动触发
                break;
            case GameFlowState.Charging:
                // 蓄力状态：检查是否可以进入过渡状态
                if (CanEnterTransitionState())
                {
                    SwitchToTransitionState();
                }
                break;
            case GameFlowState.Transition:
                // 过渡状态：检查是否可以回到正常状态
                if (CanReturnToNormalState())
                {
                    SwitchToNormalState();
                }
                break;
        }
    }
    
    #endregion
    
    #region 状态切换
    
    public void SwitchToNormalState()
    {
        if (currentState == GameFlowState.Normal) return;
        
        GameFlowState oldState = currentState;
        currentState = GameFlowState.Normal;
        
        // 释放时停
        if (timeStopManager != null)
        {
            timeStopManager.ReleaseTimeStop();
        }
        
        // 重置发射状态
        hasPlayerLaunched = false;
        
        // 触发状态变化事件
        OnStateChanged?.Invoke(currentState);
        
        // 触发MM事件
        GameStateEvent.Trigger("Normal", 0, 0f, oldState.ToString());
        
        if (showDebugInfo)
        {
            Debug.Log($"GameFlowController: 切换到正常状态 ({oldState} -> {currentState})");
        }
    }
    
    public void SwitchToChargingState()
    {
        if (currentState == GameFlowState.Charging) return;
        
        GameFlowState oldState = currentState;
        currentState = GameFlowState.Charging;
        
        // 应用完全时停
        if (timeStopManager != null)
        {
            timeStopManager.ApplyTimeStop();
        }
        
        // 触发状态变化事件
        OnStateChanged?.Invoke(currentState);
        
        // 触发MM事件
        GameStateEvent.Trigger("Charging", 0, 0f, oldState.ToString());
        
        if (showDebugInfo)
        {
            Debug.Log($"GameFlowController: 切换到蓄力状态 ({oldState} -> {currentState})");
        }
    }
    
    public void SwitchToTransitionState()
    {
        if (currentState == GameFlowState.Transition) return;
        
        GameFlowState oldState = currentState;
        currentState = GameFlowState.Transition;
        
        // 应用部分时停
        if (timeStopManager != null)
        {
            timeStopManager.ApplyPartialTimeStop();
        }
        
        // 开始过渡
        if (transitionManager != null)
        {
            transitionManager.StartTransition();
        }
        
        // 触发状态变化事件
        OnStateChanged?.Invoke(currentState);
        
        // 触发MM事件
        GameStateEvent.Trigger("Transition", 0, 0f, oldState.ToString());
        
        if (showDebugInfo)
        {
            Debug.Log($"GameFlowController: 切换到过渡状态 ({oldState} -> {currentState})");
        }
    }
    
    #endregion
    
    #region 状态验证
    
    /// <summary>
    /// 检查是否可以进入蓄力状态
    /// 注意：不再直接检测输入，由PlayerInputHandler通过事件通知
    /// </summary>
    bool CanEnterChargingState()
    {
        // 检查能量是否足够
        if (energySystem != null && !energySystem.CanUseEnergy())
        {
            return false;
        }
        
        // 检查玩家状态
        if (playerStateMachine != null && !playerStateMachine.IsIdle)
        {
            return false;
        }
        
        // 检查玩家是否在物理移动
        if (playerCore != null && playerCore.IsPhysicsMoving())
        {
            return false;
        }
        
        // 现在由PlayerInputHandler主动调用RequestChargingState()来触发状态切换
        // 这里只做基本的状态检查，不检测输入
        return true;
    }
    
    /// <summary>
    /// 检查是否可以进入过渡状态
    /// </summary>
    bool CanEnterTransitionState()
    {
        // 检查玩家是否在移动状态
        if (playerStateMachine != null && !playerStateMachine.IsMoving)
        {
            return false;
        }
        
        // 检查玩家是否在物理移动
        if (playerCore != null && !playerCore.IsPhysicsMoving())
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查是否可以回到正常状态
    /// </summary>
    bool CanReturnToNormalState()
    {
        // 检查过渡是否完成
        if (transitionManager != null && !transitionManager.IsTransitioning())
        {
            return true;
        }
        
        // 检查玩家是否停止物理移动
        if (playerCore != null && !playerCore.IsPhysicsMoving())
        {
            return true;
        }
        
        // 检查超时
        if (enableAutoTransition && transitionTimeout > 0)
        {
            // 这里需要实现超时逻辑
            return false;
        }
        
        return false;
    }
    
    #endregion
    
    #region 游戏逻辑
    
    void LaunchPlayer()
    {
        // 消耗能量
        if (energySystem != null)
        {
            energySystem.ConsumeEnergy();
        }
        
        hasPlayerLaunched = true;
        
        if (showDebugInfo)
        {
            Debug.Log("GameFlowController: 发射白球");
        }
    }
    
    public void StartNormalState()
    {
        SwitchToNormalState();
        OnGameStart?.Invoke();
    }
    
    /// <summary>
    /// 由PlayerInputHandler调用，当检测到蓄力输入时进入蓄力状态
    /// </summary>
    public void RequestChargingState()
    {
        if (CanEnterChargingState())
        {
            SwitchToChargingState();
        }
    }
    
    #endregion
    
    #region 事件处理
    
    public void OnPlayerStopped()
    {
        if (currentState == GameFlowState.Transition)
        {
            // 白球停止，结束过渡状态
            if (transitionManager != null)
            {
                transitionManager.EndTransition();
            }
        }
    }
    
    public void OnEnemyPhaseComplete()
    {
        // 敌人阶段完成，回到正常状态
        SwitchToNormalState();
    }
    
    public void OnPlayerInHole(Player player)
    {
        // 白球进洞，切换到过渡状态
        SwitchToTransitionState();
    }
    
    #endregion
    
    #region 组件引用设置
    
    public void SetEnergySystem(EnergySystem system)
    {
        energySystem = system;
    }
    
    public void SetTimeStopManager(TimeStopManager manager)
    {
        timeStopManager = manager;
    }
    
    public void SetTransitionManager(TransitionManager manager)
    {
        transitionManager = manager;
    }
    
    public void SetEnemyManager(EnemyManager manager)
    {
        enemyManager = manager;
    }
    
    #endregion
    
    #region 辅助方法
    
    string GetPreviousStateName()
    {
        switch (currentState)
        {
            case GameFlowState.Normal: return "Normal";
            case GameFlowState.Charging: return "Charging";
            case GameFlowState.Transition: return "Transition";
            default: return "Unknown";
        }
    }
    
    #endregion
    
    #region 公共属性
    
    public GameFlowState CurrentState => currentState;
    public bool IsNormalState => currentState == GameFlowState.Normal;
    public bool IsChargingState => currentState == GameFlowState.Charging;
    public bool IsTransitionState => currentState == GameFlowState.Transition;
    
    #endregion
    
    #region 组件引用设置
    
    public void SetPlayerStateMachine(PlayerStateMachine stateMachine)
    {
        playerStateMachine = stateMachine;
    }
    
    public void SetPlayerCore(PlayerCore core)
    {
        playerCore = core;
    }
    
    #endregion
}