using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("数据设置")]
    public BallData ballData; // 物理数据
    public BallCombatData combatData; // 战斗数据
    
    [Header("移动设置")]
    [Tooltip("敌人移动速度")]
    public float moveSpeed = 2f;
    [Tooltip("是否启用AI移动")]
    public bool enableAI = true;
    
    [Header("攻击设置")]
    [Tooltip("攻击间隔时间（秒）")]
    public float attackInterval = 1f;
    
    private float currentHealth;
    private Player targetPlayer;
    
    // 防重复触发机制
    private float lastAttackTime = 0f;
    private const float ATTACK_COOLDOWN = 0.1f; // 0.1秒冷却时间
    private BallPhysics ballPhysics;
    private HealthBar healthBar; // 血条组件
    private Animator enemyAnimator; // 敌人动画组件
    
    // 攻击间隔控制
    private float lastEnemyAttackTime = 0f;
    
    
    // 事件
    public System.Action<float> OnHealthChanged;
    
    void Start()
    {
        // 初始化血量（currentHealth = maxHealth）
        float maxHealth = combatData != null ? combatData.maxHealth : 100f;
        currentHealth = maxHealth; // 初始化为满血
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
        
        // 初始化动画组件
        InitializeAnimator();
    }
    
    void Update()
    {
        // 获取缩放后的时间
        float scaledDeltaTime = TimeManager.Instance != null ? 
            TimeManager.Instance.GetEnemyDeltaTime() : Time.deltaTime;
        
        // 更新动画（根据设置决定是否应用时间缩放）
        UpdateAnimations(scaledDeltaTime);
        
        // 敌人AI移动逻辑（根据设置决定是否应用时间缩放）
        if (enableAI && targetPlayer != null && IsAlive())
        {
            MoveTowardsPlayer(scaledDeltaTime);
        }
    }
    
    void OnEnable()
    {
        // 订阅攻击事件
        EventTrigger.OnAttack += HandleAttack;
    }
    
    void OnDisable()
    {
        // 取消订阅攻击事件
        EventTrigger.OnAttack -= HandleAttack;
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
    
    /// <summary>
    /// 敌人朝向玩家移动
    /// </summary>
    void MoveTowardsPlayer(float deltaTime)
    {
        if (targetPlayer == null) return;
        
        // 计算朝向玩家的方向
        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;
        
        // 根据TimeManager设置决定是否使用缩放时间
        float actualDeltaTime = deltaTime;
        if (TimeManager.Instance != null && !TimeManager.Instance.ShouldAffectEnemyMovement())
        {
            actualDeltaTime = Time.deltaTime; // 使用正常时间
        }
        
        // 移动敌人
        transform.Translate(direction * moveSpeed * actualDeltaTime);
    }
    
    /// <summary>
    /// 敌人攻击玩家
    /// </summary>
    void AttackPlayer()
    {
        if (targetPlayer == null) 
        {
            Debug.LogWarning($"Enemy {name}: targetPlayer 为空，无法攻击");
            return;
        }
        
        // 检查攻击间隔（根据设置决定是否受时停影响）
        float actualAttackInterval = attackInterval;
        if (TimeManager.Instance != null && TimeManager.Instance.ShouldAffectEnemyAttackInterval())
        {
            // 如果攻击间隔受时停影响，需要调整间隔时间
            float timeScale = TimeManager.Instance.GetEnemyTimeScale();
            actualAttackInterval = attackInterval / timeScale; // 时停时攻击间隔变长
        }
        
        if (Time.time - lastEnemyAttackTime < actualAttackInterval)
        {
            Debug.Log($"Enemy {name}: 攻击冷却中，剩余时间: {actualAttackInterval - (Time.time - lastEnemyAttackTime):F2}秒");
            return;
        }
        
        // 更新攻击时间
        lastEnemyAttackTime = Time.time;
        
        // 计算攻击方向和位置
        Vector3 attackPosition = (transform.position + targetPlayer.transform.position) * 0.5f;
        Vector3 attackDirection = (targetPlayer.transform.position - transform.position).normalized;
        
        // 获取敌人伤害值
        float damage = combatData != null ? combatData.damage : 10f;
        
        Debug.Log($"Enemy {name} 准备攻击玩家 {targetPlayer.name}，伤害: {damage}，攻击位置: {attackPosition}");
        
        // 触发攻击事件
        EventTrigger.Attack("EnemyAttack", attackPosition, attackDirection, gameObject, targetPlayer.gameObject, damage);
        
        Debug.Log($"Enemy {name} 已触发攻击事件");
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 与玩家碰撞时的处理（只在Normal和Transition阶段）
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Enemy {name} 与玩家碰撞 - 碰撞体: {collision.collider.name}, 时间: {Time.time}");
            
            // 检查游戏状态，只在Normal和Transition阶段处理碰撞
            GameFlowController gameFlowController = GameFlowController.Instance;
            if (gameFlowController != null && 
                (gameFlowController.IsNormalState || gameFlowController.IsTransitionState))
            {
                // Normal和Transition状态：敌人攻击玩家
                AttackPlayer();
            }
            else
            {
                Debug.Log($"Enemy {name} 在Charging阶段，不处理碰撞（由PlayerCore处理）");
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
    
    /// <summary>
    /// 初始化动画组件
    /// </summary>
    void InitializeAnimator()
    {
        enemyAnimator = GetComponent<Animator>();
        if (enemyAnimator != null)
        {
            Debug.Log($"敌人 {name} 动画组件初始化完成");
        }
        else
        {
            Debug.LogWarning($"敌人 {name} 未找到Animator组件");
        }
    }
    
    /// <summary>
    /// 更新敌人动画（根据设置决定是否应用时间缩放）
    /// </summary>
    void UpdateAnimations(float deltaTime)
    {
        if (enemyAnimator != null)
        {
            // 根据TimeManager设置决定是否应用时间缩放
            float timeScale = 1f;
            if (TimeManager.Instance != null && TimeManager.Instance.ShouldAffectEnemyAnimation())
            {
                timeScale = TimeManager.Instance.GetEnemyTimeScale();
            }
            
            enemyAnimator.speed = timeScale;
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
    /// 处理攻击事件
    /// 当自己是攻击目标时处理伤害
    /// </summary>
    private void HandleAttack(AttackData attackData)
    {
        Debug.Log($"Enemy {name} 收到攻击事件: 目标={attackData.Target?.name}, 伤害={attackData.Damage}, 自己={gameObject.name}");
        
        // 检查自己是否是攻击目标
        if (attackData.Target == gameObject && attackData.Damage > 0f)
        {
            Debug.Log($"Enemy {name} 是攻击目标，处理伤害: {attackData.Damage}");
            
            // 处理伤害
            TakeDamage(attackData.Damage);
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
