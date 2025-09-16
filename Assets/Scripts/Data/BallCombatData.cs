using UnityEngine;

/// <summary>
/// 攻击方式枚举
/// </summary>
public enum AttackType
{
    Contact,    // 接触攻击（现有实现）
    Ranged      // 远程攻击（待实现）
}

/// <summary>
/// 移动方式枚举
/// </summary>
public enum MovementType
{
    FollowPlayer,   // 追随玩家（现有实现）
    Patrol          // 固定路径巡逻（待实现）
}

[CreateAssetMenu(fileName = "BallCombatData", menuName = "Combat/Ball Combat Data")]
public class BallCombatData : ScriptableObject
{
    [Header("基础战斗属性")]
    [Tooltip("最大血量")]
    public float maxHealth = 100f;
    [Tooltip("攻击伤害")]
    public float damage = 10f;
    [Tooltip("攻击冷却时间（秒）")]
    public float attackCooldown = 2f;
    [Tooltip("移动速度")]
    public float moveSpeed = 2f;
    
    [Header("目标类型")]
    [Tooltip("是否为敌人（影响AI配置显示）")]
    public bool isEnemy = false;
    
    [Header("微调设置")]
    [Tooltip("微调移动速度")]
    public float microMoveSpeed = 5f;
    
    [Header("AI基础设置（仅敌人使用）")]
    [Tooltip("是否启用AI")]
    public bool enableAI = true;
    [Tooltip("AI更新间隔（秒）")]
    public float aiUpdateInterval = 0.1f;
    [Tooltip("攻击范围")]
    public float attackRange = 3f;
    
    [Header("敌人AI配置")]
    [Tooltip("攻击方式类型")]
    public AttackType attackType = AttackType.Contact;
    [Tooltip("移动方式类型")]
    public MovementType movementType = MovementType.FollowPlayer;
    
    [Header("接触攻击设置")]
    [Tooltip("接触攻击的碰撞检测范围")]
    public float contactRadius = 1f;
    
    [Header("远程攻击设置（占位符）")]
    [Tooltip("子弹预制体")]
    public GameObject bulletPrefab;
    [Tooltip("子弹速度")]
    public float bulletSpeed = 10f;
    [Tooltip("子弹伤害")]
    public float bulletDamage = 8f;
    [Tooltip("子弹存活时间")]
    public float bulletLifetime = 3f;
    [Tooltip("子弹发射偏移")]
    public Vector2 bulletOffset = Vector2.zero;
    
    [Header("追随移动设置")]
    [Tooltip("追随时的最小距离")]
    public float followMinDistance = 1f;
    [Tooltip("追随时的最大距离")]
    public float followMaxDistance = 5f;
    [Tooltip("是否保持一定距离")]
    public bool maintainDistance = false;
    
    [Header("巡逻移动设置")]
    [Tooltip("巡逻速度倍数")]
    public float patrolSpeedMultiplier = 0.7f;
    [Tooltip("防卡住检测时间（秒）")]
    public float stuckDetectionTime = 2f;
    [Tooltip("反弹角度随机偏移范围（度）")]
    public float bounceRandomOffset = 5f;
    [Tooltip("最小移动距离（防卡住检测）")]
    public float minMoveDistance = 0.5f;
}
