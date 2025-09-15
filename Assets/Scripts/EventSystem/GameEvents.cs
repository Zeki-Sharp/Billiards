using UnityEngine;

/// <summary>
/// 游戏事件定义
/// 使用简单的结构体，便于MMEventManager处理
/// </summary>
/// <summary>
/// 特效事件定义（简化版）
/// 用于非攻击相关的特效，如环境特效、UI特效等
/// 攻击相关特效通过 AttackEvent 处理
/// </summary>
public struct EffectEvent
{
    public string EffectType;        // 特效类型：Launch, HoleEnter, UI等
    public Vector3 Position;         // 特效位置
    public Vector3 Direction;        // 特效方向（可选）
    public float Intensity;          // 特效强度（可选）
    public string TargetTag;         // 目标标签（Player, Enemy等）
    public GameObject TargetObject;  // 目标对象
    
    /// <summary>
    /// 触发特效事件
    /// </summary>
    public static void Trigger(string effectType, Vector3 position, Vector3 direction = default, GameObject targetObject = null, string targetTag = "")
    {
        var effectEvent = new EffectEvent
        {
            EffectType = effectType,
            Position = position,
            Direction = direction,
            TargetObject = targetObject,
            TargetTag = targetTag,
            Intensity = 1f
        };
        
        MoreMountains.Tools.MMEventManager.TriggerEvent(effectEvent);
    }
}

/// <summary>
/// 攻击事件定义
/// 负责攻击相关的游戏逻辑，包括伤害计算、状态变化等
/// </summary>
public struct AttackEvent
{
    public string AttackType;        // 攻击类型：Hit, Shoot, Skill, Magic等
    public Vector3 Position;         // 攻击位置
    public Vector3 Direction;        // 攻击方向
    public GameObject Attacker;      // 攻击者
    public GameObject Target;        // 目标对象
    public float Damage;             // 伤害值
    public float AttackTime;         // 攻击时间戳
    public string AttackerTag;       // 攻击者标签
    public string TargetTag;         // 目标标签
    
    // 撞墙相关参数（可选）
    public Vector3 HitNormal;        // 撞击法线
    public float HitSpeed;           // 撞击速度
    public float WallHitRotationAngle;    // 墙面撞击旋转角度
    public Vector3 WallHitPositionOffset; // 墙面撞击位置偏移
    
    /// <summary>
    /// 触发攻击事件（简化版本）
    /// </summary>
    public static void Trigger(string attackType, Vector3 position, Vector3 direction, GameObject attacker, GameObject target, float damage = 0f)
    {
        var attackEvent = new AttackEvent
        {
            AttackType = attackType,
            Position = position,
            Direction = direction,
            Attacker = attacker,
            Target = target,
            Damage = damage,
            AttackTime = Time.time,
            AttackerTag = attacker != null ? attacker.tag : "",
            TargetTag = target != null ? target.tag : "",
            HitNormal = Vector3.zero,
            HitSpeed = 0f,
            WallHitRotationAngle = 0f,
            WallHitPositionOffset = Vector3.zero
        };
        
        MoreMountains.Tools.MMEventManager.TriggerEvent(attackEvent);
    }
    
    /// <summary>
    /// 触发攻击事件（带撞墙参数版本）
    /// </summary>
    public static void Trigger(string attackType, Vector3 position, Vector3 direction, GameObject attacker, GameObject target, float damage, Vector3 hitNormal, float hitSpeed, float wallHitRotationAngle = 0f, Vector3 wallHitPositionOffset = default)
    {
        var attackEvent = new AttackEvent
        {
            AttackType = attackType,
            Position = position,
            Direction = direction,
            Attacker = attacker,
            Target = target,
            Damage = damage,
            AttackTime = Time.time,
            AttackerTag = attacker != null ? attacker.tag : "",
            TargetTag = target != null ? target.tag : "",
            HitNormal = hitNormal,
            HitSpeed = hitSpeed,
            WallHitRotationAngle = wallHitRotationAngle,
            WallHitPositionOffset = wallHitPositionOffset
        };
        
        MoreMountains.Tools.MMEventManager.TriggerEvent(attackEvent);
    }
}

/// <summary>
/// 死亡事件定义
/// 负责死亡相关的游戏逻辑，包括特效播放、对象销毁、状态更新等
/// </summary>
public struct DeathEvent
{
    public string DeathType;        // 死亡类型：EnemyDeath, PlayerDeath等
    public Vector3 Position;        // 死亡位置
    public Vector3 Direction;       // 死亡方向（可选）
    public GameObject DeadObject;   // 死亡对象
    public string DeadObjectTag;    // 死亡对象标签
    public float DeathTime;         // 死亡时间戳
    
    /// <summary>
    /// 触发死亡事件
    /// </summary>
    public static void Trigger(string deathType, Vector3 position, Vector3 direction, GameObject deadObject)
    {
        Debug.Log($"DeathEvent.Trigger: 创建死亡事件，类型: {deathType}, 对象: {deadObject?.name}");
        
        var deathEvent = new DeathEvent
        {
            DeathType = deathType,
            Position = position,
            Direction = direction,
            DeadObject = deadObject,
            DeadObjectTag = deadObject != null ? deadObject.tag : "",
            DeathTime = Time.time
        };
        
        Debug.Log($"DeathEvent.Trigger: 通过 MMEventManager 触发死亡事件");
        MoreMountains.Tools.MMEventManager.TriggerEvent(deathEvent);
    }
}

// GameStateEvent 已移除，改用各系统的 C# Action 事件
// 例如：energySystem.OnEnergyChanged, transitionManager.OnTransitionStart 等

// 复杂的状态机通信事件已移除，改为直接引用通信

