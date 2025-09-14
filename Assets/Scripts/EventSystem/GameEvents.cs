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

public struct GameStateEvent
{
    public string StateName;         // 状态名称：PhaseChanged, HealthChanged等
    public int IntValue;            // 整数值
    public float FloatValue;        // 浮点值
    public string StringValue;      // 字符串值
    public Vector3 Vector3Value;    // 向量值
    public bool BoolValue;          // 布尔值
    
    /// <summary>
    /// 触发游戏状态事件
    /// </summary>
    public static void Trigger(string stateName, int intValue = 0, float floatValue = 0f, string stringValue = "", Vector3 vector3Value = default, bool boolValue = false)
    {
        var gameStateEvent = new GameStateEvent
        {
            StateName = stateName,
            IntValue = intValue,
            FloatValue = floatValue,
            StringValue = stringValue,
            Vector3Value = vector3Value,
            BoolValue = boolValue
        };
        
        MoreMountains.Tools.MMEventManager.TriggerEvent(gameStateEvent);
    }
}

