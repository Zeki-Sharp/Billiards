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
