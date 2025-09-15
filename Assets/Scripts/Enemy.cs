using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class Enemy : MonoBehaviour, MMEventListener<AttackEvent>
{
    [Header("数据设置")]
    public BallData ballData; // 物理数据
    public BallCombatData combatData; // 战斗数据
    
    
    private float currentHealth;
    private Player targetPlayer;
    
    // 防重复触发机制
    private float lastAttackTime = 0f;
    private const float ATTACK_COOLDOWN = 0.1f; // 0.1秒冷却时间
    private BallPhysics ballPhysics;
    private HealthBar healthBar; // 血条组件
    
    
    // 事件
    public System.Action<float> OnHealthChanged;
    
    void Start()
    {
        currentHealth = combatData != null ? combatData.currentHealth : 100f;
        targetPlayer = FindAnyObjectByType<Player>();
        
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
    
    void OnEnable()
    {
        this.MMEventStartListening<AttackEvent>();
    }
    
    void OnDisable()
    {
        this.MMEventStopListening<AttackEvent>();
    }
    
    public void InitializeAttackRange()
    {
        AttackRange attackRange = GetComponentInChildren<AttackRange>();
        if (attackRange != null && targetPlayer != null)
        {
            // 计算从敌人到白球的方向
            Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;
            
            // 简化初始化：直接调用AttackRange的SetAttackDirection方法
            attackRange.SetAttackDirection(direction);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 被白球撞击时的处理
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Enemy {name} 被白球撞击 - 碰撞体: {collision.collider.name}, 时间: {Time.time}");
            
            // 防重复触发检查
            if (Time.time - lastAttackTime < ATTACK_COOLDOWN)
            {
                Debug.Log($"Enemy {name} 攻击冷却中，忽略重复触发");
                return;
            }
            
            // 检查当前游戏状态，只有在非敌人阶段时才受伤
            GameFlowController gameFlowController = GameFlowController.Instance;
            if (gameFlowController != null && gameFlowController.IsNormalState)
            {
                Debug.Log($"Enemy {name} 在正常状态，不受伤");
                return;
            }
            
            // 更新最后攻击时间
            lastAttackTime = Time.time;
            
            // 计算伤害和碰撞信息
            // 白球攻击敌人，应该使用白球的伤害值
            Player player = collision.gameObject.GetComponent<Player>();
            float damage = player != null && player.combatData != null ? player.combatData.damage : 50f;
            Vector3 hitPosition = (transform.position + collision.transform.position) * 0.5f;
            Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
            
            Debug.Log($"Enemy {name} 被玩家攻击，玩家伤害: {damage}, 碰撞体: {collision.collider.name}");
            
            // 触发攻击事件，伤害处理由事件监听器处理
            EventTrigger.Attack("Hit", hitPosition, hitDirection, collision.gameObject, gameObject, damage);
            
            // 获取玩家的 BallPhysics 组件
            PlayerCore playerCore = player.GetPlayerCore();
            if (playerCore != null)
            {
                BallPhysics playerPhysics = playerCore.GetComponent<BallPhysics>();
                if (playerPhysics != null)
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
                        
                        // 给玩家添加充能力
                        playerPhysics.ApplyForce(-boostForce);
                    }
                }
            }
            
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
        float oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        float maxHealth = combatData != null ? combatData.maxHealth : 100f;
        // 更新血条
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
        
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        // 检查死亡
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
        Debug.Log($"敌人 {name} 开始死亡流程");
        
        // 禁用碰撞器，停止与白球的物理交互
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            Debug.Log($"敌人 {name} 碰撞器已禁用");
        }

        // 触发死亡特效
        Debug.Log($"敌人 {name} 触发死亡事件");
        EventTrigger.Dead(transform.position, Vector3.zero, gameObject);
        
        Debug.Log($"敌人 {name} 死亡流程完成");
    }
    
    /// <summary>
    /// 处理攻击事件（MMEventListener接口实现）
    /// 当自己是攻击目标时处理伤害
    /// </summary>
    public void OnMMEvent(AttackEvent attackEvent)
    {
        Debug.Log($"Enemy {name} 收到攻击事件: 目标={attackEvent.Target?.name}, 伤害={attackEvent.Damage}, 自己={gameObject.name}");
        
        // 检查自己是否是攻击目标
        if (attackEvent.Target == gameObject && attackEvent.Damage > 0f)
        {
            Debug.Log($"Enemy {name} 是攻击目标，处理伤害: {attackEvent.Damage}");
            
            // 处理伤害
            TakeDamage(attackEvent.Damage);
        }
        else
        {
            Debug.Log($"Enemy {name} 不是攻击目标或伤害为0，忽略");
        }
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
