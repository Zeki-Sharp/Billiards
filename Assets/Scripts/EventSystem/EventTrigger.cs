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
    

}
