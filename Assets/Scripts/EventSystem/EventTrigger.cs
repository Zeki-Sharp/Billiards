using UnityEngine;

/// <summary>
/// 统一的事件触发接口
/// 提供简化的静态方法，内部调用MMEventManager
/// </summary>
public static class EventTrigger
{
    #region 通用攻击方法
    
    /// <summary>
    /// 通用攻击事件 - 触发攻击相关的游戏逻辑
    /// </summary>
    /// <param name="attackType">攻击类型：Hit, Shoot, Skill, Magic 等</param>
    /// <param name="position">攻击位置</param>
    /// <param name="direction">攻击方向</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    /// <param name="damage">伤害值（可选，默认为0）</param>
    public static void Attack(string attackType, Vector3 position, Vector3 direction, GameObject attacker, GameObject target, float damage = 0f)
    {
        // 触发攻击事件，由各系统监听处理
        AttackEvent.Trigger(attackType, position, direction, attacker, target, damage);
        
        if (Debug.isDebugBuild)
        {
            Debug.Log($"EventTrigger.Attack: 触发攻击事件 {attackType} -> {attacker?.name} 攻击 {target?.name}, 伤害: {damage}");
        }
    }
    
    /// <summary>
    /// 通用攻击事件 - 带完整参数版本（用于墙壁撞击等复杂攻击）
    /// </summary>
    /// <param name="attackType">攻击类型：Hit, Shoot, Skill, Magic 等</param>
    /// <param name="position">攻击位置</param>
    /// <param name="direction">攻击方向</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    /// <param name="hitNormal">撞击法线</param>
    /// <param name="speed">撞击速度</param>
    /// <param name="rotationAngle">旋转角度</param>
    /// <param name="positionOffset">位置偏移</param>
    /// <param name="damage">伤害值（可选，默认为0）</param>
    public static void Attack(string attackType, Vector3 position, Vector3 direction, GameObject attacker, GameObject target, Vector3 hitNormal, float speed, float rotationAngle = 0f, Vector3 positionOffset = default, float damage = 0f)
    {
        // 触发攻击事件（带撞墙参数），由各系统监听处理
        AttackEvent.Trigger(attackType, position, direction, attacker, target, damage, hitNormal, speed, rotationAngle, positionOffset);
        
        if (Debug.isDebugBuild)
        {
            Debug.Log($"EventTrigger.Attack: 触发复杂攻击事件 {attackType} -> {attacker?.name} 攻击 {target?.name}, 伤害: {damage}, 速度: {speed}, 旋转角度: {rotationAngle}");
        }
    }
    
    #endregion
    
    #region 特殊特效方法（保留现有方法）
    
    /// <summary>
    /// 触发发射特效
    /// </summary>
    public static void Launch(Vector3 position, Vector3 direction, GameObject target)
    {
        EffectEvent.Trigger("Launch Effect", position, direction, target, "Player");
    }
    
    /// <summary>
    /// 触发进洞特效
    /// </summary>
    public static void HoleEnter(Vector3 position, GameObject target)
    {
        EffectEvent.Trigger("Hole Enter Effect", position, Vector3.zero, target, "Player");
    }
    
    /// <summary>
    /// 触发死亡事件
    /// </summary>
    public static void Dead(Vector3 position, Vector3 direction, GameObject target)
    {
        Debug.Log($"EventTrigger.Dead: 触发死亡事件，目标: {target?.name}");
        DeathEvent.Trigger("EnemyDeath", position, direction, target);
    }
    
    /// <summary>
    /// 触发蓄力开始特效
    /// </summary>
    public static void ChargeStart(Vector3 position, GameObject target)
    {
        EffectEvent.Trigger("Charge Effect", position, Vector3.zero, target, "Player");
    }
    
    #endregion
    
    #region 游戏状态事件
    
    /// <summary>
    /// 触发阶段转换事件
    /// </summary>
    public static void PhaseChanged(int phaseId, string phaseName)
    {
        GameStateEvent.Trigger("PhaseChanged", phaseId, 0f, phaseName);
    }
    
    /// <summary>
    /// 触发游戏开始事件
    /// </summary>
    public static void GameStart()
    {
        GameStateEvent.Trigger("GameStart");
    }
    
    /// <summary>
    /// 触发游戏结束事件
    /// </summary>
    public static void GameEnd(bool isVictory)
    {
        GameStateEvent.Trigger("GameEnd", 0, 0f, "", Vector3.zero, isVictory);
    }
    
    /// <summary>
    /// 触发分数变化事件
    /// </summary>
    public static void ScoreChanged(int newScore)
    {
        GameStateEvent.Trigger("ScoreChanged", newScore);
    }
    
    /// <summary>
    /// 触发生命值变化事件
    /// </summary>
    public static void HealthChanged(float currentHealth, float maxHealth)
    {
        GameStateEvent.Trigger("HealthChanged", 0, currentHealth, $"{currentHealth}/{maxHealth}");
    }
    
    #endregion
    
    #region 状态机通信事件
    
    /// <summary>
    /// 触发玩家状态变化事件
    /// </summary>
    public static void PlayerStateChanged(string fromState, string toState, string stateType, bool canMove = true, bool canCharge = true, bool isPhysicsMoving = false)
    {
        PlayerStateChangeEvent.Trigger(fromState, toState, stateType, canMove, canCharge, isPhysicsMoving);
        
        if (Debug.isDebugBuild)
        {
            Debug.Log($"EventTrigger.PlayerStateChanged: {fromState} -> {toState} (类型: {stateType})");
        }
    }
    
    /// <summary>
    /// 触发游戏流程状态变化事件
    /// </summary>
    public static void GameFlowStateChanged(string fromState, string toState, string flowType, bool isTimeStopped = false, bool isPartialTimeStop = false, bool canPlayerMove = true)
    {
        GameFlowStateChangeEvent.Trigger(fromState, toState, flowType, isTimeStopped, isPartialTimeStop, canPlayerMove);
        
        if (Debug.isDebugBuild)
        {
            Debug.Log($"EventTrigger.GameFlowStateChanged: {fromState} -> {toState} (类型: {flowType})");
        }
    }
    
    /// <summary>
    /// 请求进入蓄力状态（由PlayerInputHandler调用）
    /// </summary>
    public static void RequestChargingState()
    {
        GameStateEvent.Trigger("RequestCharging", 0, 0f, "PlayerInput");
        
        if (Debug.isDebugBuild)
        {
            Debug.Log("EventTrigger.RequestChargingState: 请求进入蓄力状态");
        }
    }
    
    /// <summary>
    /// 请求进入过渡状态（由PlayerStateMachine调用）
    /// </summary>
    public static void RequestTransitionState()
    {
        GameStateEvent.Trigger("RequestTransition", 0, 0f, "PlayerStateMachine");
        
        if (Debug.isDebugBuild)
        {
            Debug.Log("EventTrigger.RequestTransitionState: 请求进入过渡状态");
        }
    }
    
    /// <summary>
    /// 请求回到正常状态（由PlayerStateMachine调用）
    /// </summary>
    public static void RequestNormalState()
    {
        GameStateEvent.Trigger("RequestNormal", 0, 0f, "PlayerStateMachine");
        
        if (Debug.isDebugBuild)
        {
            Debug.Log("EventTrigger.RequestNormalState: 请求回到正常状态");
        }
    }
    
    #endregion
}
