using UnityEngine;

/// <summary>
/// 统一的事件触发接口
/// 提供简化的静态方法，内部调用MMEventManager
/// </summary>
public static class EventTrigger
{
    #region 特效事件
    
    /// <summary>
    /// 触发撞击特效
    /// </summary>
    public static void Hit(Vector3 position, Vector3 direction, GameObject target)
    {
        EffectEvent.Trigger("Hit", position, direction, target, "Enemy");
    }
    
    /// <summary>
    /// 触发撞击特效（带完整参数，用于墙壁受击等复杂特效）
    /// </summary>
    public static void Hit(Vector3 position, Vector3 direction, GameObject target, Vector3 hitNormal, float speed, float rotationAngle = 0f, Vector3 positionOffset = default)
    {
        EffectEvent.Trigger("Hit", position, direction, target, "Wall", hitNormal, speed, rotationAngle, positionOffset);
    }
    
    /// <summary>
    /// 触发撞墙特效
    /// </summary>
    public static void WallHit(Vector3 position, Vector3 direction, GameObject target, Vector3 hitNormal, float speed, float rotationAngle = 0f, Vector3 positionOffset = default)
    {
        EffectEvent.Trigger("WallHit", position, direction, target, "Wall", hitNormal, speed, rotationAngle, positionOffset);
    }
    
    /// <summary>
    /// 触发发射特效
    /// </summary>
    public static void Launch(Vector3 position, Vector3 direction, GameObject target)
    {
        EffectEvent.Trigger("Launch", position, direction, target, "Player");
    }
    
    /// <summary>
    /// 触发进洞特效
    /// </summary>
    public static void HoleEnter(Vector3 position, GameObject target)
    {
        EffectEvent.Trigger("HoleEnter", position, Vector3.zero, target, "Player");
    }
    
    /// <summary>
    /// 触发攻击特效
    /// </summary>
    public static void Attack(Vector3 position, Vector3 direction, GameObject target)
    {
        EffectEvent.Trigger("Attack", position, direction, target, "Enemy");
    }
    
    /// <summary>
    /// 触发死亡特效
    /// </summary>
    public static void Dead(Vector3 position, Vector3 direction, GameObject target)
    {
        EffectEvent.Trigger("Dead", position, direction, target, "Enemy");
    }
    
    /// <summary>
    /// 触发蓄力开始特效
    /// </summary>
    public static void ChargeStart(Vector3 position, GameObject target)
    {
        EffectEvent.Trigger("ChargeStart", position, Vector3.zero, target, "Player");
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
