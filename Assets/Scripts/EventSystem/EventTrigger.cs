using UnityEngine;

/// <summary>
/// 简化的事件触发接口
/// 实现游戏逻辑和表现的桥接机制
/// </summary>
public static class EventTrigger
{
    // 游戏逻辑事件 - C# Action
    public static System.Action<AttackData> OnAttack; // 攻击逻辑事件
    public static System.Action<DeathData> OnDeath;   // 死亡逻辑事件
    
    #region 攻击方法
    
    /// <summary>
    /// 基础攻击事件 - 触发攻击相关的游戏逻辑和表现
    /// </summary>
    /// <param name="attackType">攻击类型：Hit, Shoot, Skill, Magic 等</param>
    /// <param name="position">攻击位置</param>
    /// <param name="direction">攻击方向</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    /// <param name="damage">伤害值（可选，默认为0）</param>
    public static void Attack(string attackType, Vector3 position, Vector3 direction, GameObject attacker, GameObject target, float damage = 0f)
    {
        // 创建攻击数据
        var attackData = new AttackData
        {
            AttackType = attackType,
            Position = position,
            Direction = direction,
            Attacker = attacker,
            Target = target,
            Damage = damage,
            AttackTime = Time.time,
            AttackerTag = attacker?.tag ?? "",
            TargetTag = target?.tag ?? "",
            HitNormal = Vector3.zero,
            HitSpeed = 0f,
            WallHitRotationAngle = 0f,
            WallHitPositionOffset = Vector3.zero
        };
        
        // 触发游戏逻辑事件
        OnAttack?.Invoke(attackData);
        
        // 触发表现事件
        AttackEffectEvent.Trigger(attackData);
        
    }
    
    /// <summary>
    /// 复杂攻击事件 - 带完整参数版本（用于墙壁撞击等复杂攻击）
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
        // 创建攻击数据
        var attackData = new AttackData
        {
            AttackType = attackType,
            Position = position,
            Direction = direction,
            Attacker = attacker,
            Target = target,
            Damage = damage,
            AttackTime = Time.time,
            AttackerTag = attacker?.tag ?? "",
            TargetTag = target?.tag ?? "",
            HitNormal = hitNormal,
            HitSpeed = speed,
            WallHitRotationAngle = rotationAngle,
            WallHitPositionOffset = positionOffset
        };
        
        // 触发游戏逻辑事件
        OnAttack?.Invoke(attackData);
        
        // 触发表现事件
        AttackEffectEvent.Trigger(attackData);
        
    }
    
    #endregion
    
    #region 死亡方法
    
    /// <summary>
    /// 触发死亡事件 - 触发死亡相关的游戏逻辑和表现
    /// </summary>
    public static void Dead(Vector3 position, Vector3 direction, GameObject target)
    {
        // 创建死亡数据
        var deathData = new DeathData
        {
            DeathType = "EnemyDeath",
            Position = position,
            Direction = direction,
            DeadObject = target,
            DeadObjectTag = target?.tag ?? "",
            DeathTime = Time.time
        };
        
        // 触发游戏逻辑事件
        OnDeath?.Invoke(deathData);
        
        // 触发表现事件
        DeathEffectEvent.Trigger(deathData);
        
    }
    
    #endregion
    
    #region 特效方法
    
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
    /// 触发蓄力开始特效
    /// </summary>
    public static void ChargeStart(Vector3 position, GameObject target)
    {
        EffectEvent.Trigger("Charge Effect", position, Vector3.zero, target, "Player");
    }
    
    #endregion

}
