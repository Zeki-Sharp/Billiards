using UnityEngine;

[CreateAssetMenu(fileName = "BallCombatData", menuName = "Combat/Ball Combat Data")]
public class BallCombatData : ScriptableObject
{
    [Header("血量设置")]
    [Tooltip("最大血量")]
    public float maxHealth = 100f;
    
    [Header("移动设置")]
    [Tooltip("移动速度")]
    public float moveSpeed = 2f;
    
    [Header("攻击设置")]
    [Tooltip("攻击伤害")]
    public float damage = 10f;
    [Tooltip("攻击冷却时间（秒）")]
    public float attackCooldown = 2f;
    
    [Header("微调设置")]
    [Tooltip("微调移动速度")]
    public float microMoveSpeed = 5f;
    
    [Header("AI设置")]
    [Tooltip("是否启用AI")]
    public bool enableAI = true;
    [Tooltip("AI更新间隔（秒）")]
    public float aiUpdateInterval = 0.1f;
    [Tooltip("攻击范围")]
    public float attackRange = 3f;
}
