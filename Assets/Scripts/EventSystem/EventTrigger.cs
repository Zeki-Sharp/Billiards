using UnityEngine;

/// <summary>
/// 统一的事件触发接口
/// 提供简化的静态方法，内部调用MMEventManager
/// </summary>
public static class EventTrigger
{
    #region 通用攻击方法
    
    /// <summary>
    /// 通用攻击特效 - 一次调用，双方都触发
    /// </summary>
    /// <param name="attackType">攻击类型：Hit, Shoot, Skill, Magic 等</param>
    /// <param name="position">特效位置</param>
    /// <param name="direction">特效方向</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    public static void Attack(string attackType, Vector3 position, Vector3 direction, GameObject attacker, GameObject target)
    {
        // 攻击者特效 - 发送给攻击者
        string attackEffectType = $"{attackType} Attack Effect";
        Debug.Log($"EventTrigger.Attack: 触发攻击者特效 {attackEffectType} -> {attacker.name}");
        EffectEvent.Trigger(attackEffectType, position, direction, attacker, attacker.tag);
        
        // 受击者特效 - 发送给被攻击者
        string beHitEffectType = "Be Hit Effect";
        Debug.Log($"EventTrigger.Attack: 触发受击者特效 {beHitEffectType} -> {target.name}");
        EffectEvent.Trigger(beHitEffectType, position, direction, target, target.tag);
    }
    
    /// <summary>
    /// 通用攻击特效 - 带完整参数版本（用于墙壁撞击等复杂特效）
    /// </summary>
    /// <param name="attackType">攻击类型：Hit, Shoot, Skill, Magic 等</param>
    /// <param name="position">特效位置</param>
    /// <param name="direction">特效方向</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    /// <param name="hitNormal">撞击法线</param>
    /// <param name="speed">撞击速度</param>
    /// <param name="rotationAngle">旋转角度</param>
    /// <param name="positionOffset">位置偏移</param>
    public static void Attack(string attackType, Vector3 position, Vector3 direction, GameObject attacker, GameObject target, Vector3 hitNormal, float speed, float rotationAngle = 0f, Vector3 positionOffset = default)
    {
        // 攻击者特效 - 发送给攻击者
        string attackEffectType = $"{attackType} Attack Effect";
        Debug.Log($"EventTrigger.Attack: 触发攻击者特效 {attackEffectType} -> {attacker.name}");
        EffectEvent.Trigger(attackEffectType, position, direction, attacker, attacker.tag);
        
        // 受击者特效 - 发送给被攻击者（带完整参数）
        string beHitEffectType = "Be Hit Effect";
        Debug.Log($"EventTrigger.Attack: 触发受击者特效 {beHitEffectType} -> {target.name} (带参数)");
        EffectEvent.Trigger(beHitEffectType, position, direction, target, target.tag, hitNormal, speed, rotationAngle, positionOffset);
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
    /// 触发死亡特效
    /// </summary>
    public static void Dead(Vector3 position, Vector3 direction, GameObject target)
    {
        EffectEvent.Trigger("Dead Effect", position, direction, target, target.tag);
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
}
