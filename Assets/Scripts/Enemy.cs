using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("数据设置")]
    public BallData ballData; // 物理数据
    public BallCombatData combatData; // 战斗数据
    
    // 注意：撞墙特效现在由WallManager统一管理
    
    private float currentHealth;
    private WhiteBall targetBall;
    private BallPhysics ballPhysics;
    private HealthBar healthBar; // 血条组件
    
    
    // 事件
    public System.Action<float> OnHealthChanged;
    
    void Start()
    {
        currentHealth = combatData != null ? combatData.currentHealth : 100f;
        targetBall = FindAnyObjectByType<WhiteBall>();
        
        // 获取或添加 BallPhysics 组件
        ballPhysics = GetComponent<BallPhysics>();
        if (ballPhysics == null)
        {
            ballPhysics = gameObject.AddComponent<BallPhysics>();
        }
        
        // 设置 BallData
        if (ballData != null)
        {
            ballPhysics.ballData = ballData;
        }
        else
        {
            Debug.LogError("Enemy: 请设置 BallData 资源！");
        }
        
        // 初始化攻击范围朝向
        InitializeAttackRange();
        
        // 初始化血条
        InitializeHealthBar();
    }
    
    public void InitializeAttackRange()
    {
        AttackRange attackRange = GetComponentInChildren<AttackRange>();
        if (attackRange != null && targetBall != null)
        {
            // 计算从敌人到白球的方向
            Vector2 direction = (targetBall.transform.position - transform.position).normalized;
            
            // 简化初始化：直接调用AttackRange的SetAttackDirection方法
            attackRange.SetAttackDirection(direction);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 被白球撞击时的处理
        if (collision.gameObject.CompareTag("Player"))
        {
            // 检查当前游戏阶段，只有在非敌人阶段时才受伤
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.GetCurrentPhase() == GameManager.GamePhase.EnemyPhase)
            {
                return;
            }
            
            // 先计算伤害，判断是否死亡
            float damage = combatData != null ? combatData.damage : 50f;
            bool willDie = (currentHealth - damage) <= 0;
            
            // 计算碰撞位置和方向
            Vector3 hitPosition = (transform.position + collision.transform.position) * 0.5f;
            Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
            
            if (willDie)
            {
                // 如果会死亡，只播放死亡特效
                EventTrigger.Dead(hitPosition, hitDirection, gameObject);
            }
            else
            {
                // 如果不会死亡，播放受击特效
                EventTrigger.Attack("Hit", hitPosition, hitDirection, collision.gameObject, gameObject);
            }
            
            // 获取白球的 BallPhysics 组件
            BallPhysics whiteBallPhysics = collision.gameObject.GetComponent<BallPhysics>();
            if (whiteBallPhysics != null)
            {
                // 检查是否还能获得充能力（基于速度）
                if (CanGetBoost())
                {
                    // 计算碰撞方向
                    Vector2 collisionDirection = (transform.position - collision.transform.position).normalized;
                    
                    // 给双方都添加充能力
                    Vector2 boostForce = collisionDirection * ballData.hitBoostForce * ballData.hitBoostMultiplier;
                    
                    // 给敌人添加充能力
                    ballPhysics.ApplyForce(boostForce);
                    
                    // 给白球添加充能力
                    whiteBallPhysics.ApplyForce(-boostForce);
                }
            }
            
            // 最后应用伤害
            TakeDamage(damage);
        }
        
        // 撞墙时的处理
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 检查是否还能获得充能力（基于速度）
            if (CanGetBoost())
            {
                // 计算撞墙方向（从墙壁指向敌人）
                Vector2 wallDirection = ((Vector2)transform.position - collision.contacts[0].point).normalized;
                
                // 给敌人添加撞墙充能力
                Vector2 wallBoostForce = wallDirection * ballData.hitBoostForce * ballData.hitBoostMultiplier;
                ballPhysics.ApplyForce(wallBoostForce);
            }
            
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"敌人受到伤害: {damage}, 当前血量: {currentHealth}");
        
        // 更新血条
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, combatData != null ? combatData.maxHealth : 100f);
        }
        
        OnHealthChanged?.Invoke(currentHealth / (combatData != null ? combatData.maxHealth : 100f));
        
        // 注意：死亡检查已移至OnCollisionEnter2D中，避免特效冲突
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void InitializeHealthBar()
    {
        // 查找血条组件
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetTarget(transform);
            healthBar.UpdateHealth(currentHealth, combatData != null ? combatData.maxHealth : 100f);
        }
        else
        {
            Debug.LogWarning($"敌人 {name} 未找到HealthBar组件，请确保血条预制体包含HealthBar脚本");
        }
    }
    
    void Die()
    {
        // 禁用碰撞器，停止与白球的物理交互
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 注意：死亡特效已在OnCollisionEnter2D中发布，避免重复发布
        // EventBus.PublishEffect("EnemyDead", transform.position, Vector3.zero, gameObject, "Enemy");
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / (combatData != null ? combatData.maxHealth : 100f);
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public float GetCurrentSpeed()
    {
        return ballPhysics != null ? ballPhysics.GetSpeed() : 0f;
    }
    
    public bool IsMoving()
    {
        return ballPhysics != null && ballPhysics.IsMoving();
    }
    
    // 检查是否还能获得充能力（基于速度）
    private bool CanGetBoost()
    {
        return ballPhysics != null && ballPhysics.GetSpeed() > ballData.boostSpeedThreshold;
    }
}
