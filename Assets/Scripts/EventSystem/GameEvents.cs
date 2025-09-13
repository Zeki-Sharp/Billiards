using UnityEngine;

/// <summary>
/// 游戏事件定义
/// 使用简单的结构体，便于MMEventManager处理
/// </summary>
public struct EffectEvent
{
    public string EffectType;        // 特效类型：Hit, Launch, WallHit, HoleEnter等
    public Vector3 Position;         // 特效位置
    public Vector3 Direction;        // 特效方向（可选）
    public float Intensity;          // 特效强度（可选）
    public string TargetTag;         // 目标标签（Player, Enemy等）
    public GameObject TargetObject;  // 目标对象
    public Vector3 HitNormal;        // 撞击法线（用于墙面撞击旋转计算）
    public float HitSpeed;           // 撞击速度（用于摇晃强度计算）
    public float WallHitRotationAngle;    // 墙面撞击旋转角度（可选）
    public Vector3 WallHitPositionOffset; // 墙面撞击位置偏移（可选）
    
    /// <summary>
    /// 触发特效事件（简化版本）
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
    
    /// <summary>
    /// 触发特效事件（完整版本，包含墙面撞击参数）
    /// </summary>
    public static void Trigger(string effectType, Vector3 position, Vector3 direction, GameObject targetObject, string targetTag, Vector3 hitNormal, float hitSpeed, float wallHitRotationAngle = 0f, Vector3 wallHitPositionOffset = default)
    {
        var effectEvent = new EffectEvent
        {
            EffectType = effectType,
            Position = position,
            Direction = direction,
            TargetObject = targetObject,
            TargetTag = targetTag,
            Intensity = 1f,
            HitNormal = hitNormal,
            HitSpeed = hitSpeed,
            WallHitRotationAngle = wallHitRotationAngle,
            WallHitPositionOffset = wallHitPositionOffset
        };
        
        MoreMountains.Tools.MMEventManager.TriggerEvent(effectEvent);
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

