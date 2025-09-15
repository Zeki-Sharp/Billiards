using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 三阶段流程控制器 - 管理Normal、Charging、Transition三状态
/// 专注于游戏流程控制，不处理游戏数据管理
/// </summary>
public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }
    
    /// <summary>
    /// 游戏状态枚举 - 三状态设计
    /// </summary>
    public enum GameState
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
    private GameState currentState = GameState.Normal;
    
    // 组件引用（由GameInitializer设置）
    private EnergySystem energySystem;
    private TimeStopManager timeStopManager;
    private TransitionManager transitionManager;
    private EnemyManager enemyManager;
    
    // 状态管理
    private bool hasPlayerLaunched = false;
    
    // 事件（使用MM架构）
    public System.Action<GameState> OnStateChanged;
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
        // 三状态流程控制
        HandleCurrentState();
    }
    
    #region 状态管理
    
    void HandleCurrentState()
    {
        switch (currentState)
        {
            case GameState.Normal:
                HandleNormalState();
                break;
            case GameState.Charging:
                HandleChargingState();
                break;
            case GameState.Transition:
                HandleTransitionState();
                break;
        }
    }
    
    void HandleNormalState()
    {
        // 正常状态：玩家移动躲避，敌人移动+射击
        // 不再自动检测蓄力输入，等待Player通知切换状态
        // if (CanEnterChargingState())
        // {
        //     SwitchToChargingState();
        // }
    }
    
    void HandleChargingState()
    {
        // 蓄力状态：完全时停，玩家瞄准蓄力
        // 不再自动检测发射，等待Player通知发射
        // if (ShouldLaunch())
        // {
        //     LaunchPlayer();
        //     SwitchToTransitionState();
        // }
    }
    
    void HandleTransitionState()
    {
        // 过渡状态：玩家可移动，敌人和子弹仍时停，白球运动
        // 检查是否应该回到正常状态
        if (ShouldReturnToNormal())
        {
            SwitchToNormalState();
        }
    }
    
    #endregion
    
    #region 状态切换
    
    public void SwitchToNormalState()
    {
        if (currentState == GameState.Normal) return;
        
        currentState = GameState.Normal;
        
        // 释放时停
        if (timeStopManager != null)
        {
            timeStopManager.ReleaseTimeStop();
        }
        
        // 重置发射状态
        hasPlayerLaunched = false;
        
        OnStateChanged?.Invoke(currentState);
        
        // 触发MM事件
        GameStateEvent.Trigger("Normal", 0, 0f, GetPreviousStateName());
        
        if (showDebugInfo)
        {
            Debug.Log("GameFlowController: 切换到正常状态");
        }
    }
    
    public void SwitchToChargingState()
    {
        if (currentState == GameState.Charging) return;
        
        currentState = GameState.Charging;
        
        // 应用完全时停
        if (timeStopManager != null)
        {
            timeStopManager.ApplyTimeStop();
        }
        
        OnStateChanged?.Invoke(currentState);
        
        // 触发MM事件
        GameStateEvent.Trigger("Charging", 0, 0f, GetPreviousStateName());
        
        if (showDebugInfo)
        {
            Debug.Log("GameFlowController: 切换到蓄力状态");
        }
    }
    
    public void SwitchToTransitionState()
    {
        if (currentState == GameState.Transition) return;
        
        currentState = GameState.Transition;
        
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
        
        OnStateChanged?.Invoke(currentState);
        
        // 触发MM事件
        GameStateEvent.Trigger("Transition", 0, 0f, GetPreviousStateName());
        
        if (showDebugInfo)
        {
            Debug.Log("GameFlowController: 切换到过渡状态");
        }
    }
    
    #endregion
    
    #region 状态检查
    
    bool CanEnterChargingState()
    {
        // 检查能量是否足够
        if (energySystem != null && !energySystem.CanUseEnergy())
        {
            return false;
        }
        
        // 检查玩家输入（长按左键）
        return Input.GetMouseButton(0);
    }
    
    bool ShouldLaunch()
    {
        // 检查玩家是否释放鼠标
        return Input.GetMouseButtonUp(0);
    }
    
    bool ShouldReturnToNormal()
    {
        // 检查过渡是否完成
        if (transitionManager != null && !transitionManager.IsTransitioning())
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
    
    #endregion
    
    #region 事件处理
    
    public void OnPlayerStopped()
    {
        if (currentState == GameState.Transition)
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
            case GameState.Normal: return "Normal";
            case GameState.Charging: return "Charging";
            case GameState.Transition: return "Transition";
            default: return "Unknown";
        }
    }
    
    #endregion
    
    #region 公共属性
    
    public GameState CurrentState => currentState;
    public bool IsNormalState => currentState == GameState.Normal;
    public bool IsChargingState => currentState == GameState.Charging;
    public bool IsTransitionState => currentState == GameState.Transition;
    
    #endregion
}