using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 玩家核心组件 - 负责物理逻辑、碰撞处理和蓄力计算
/// 
/// 【核心职责】：
/// - 管理球体的物理运动和碰撞检测
/// - 处理蓄力系统和发射逻辑
/// - 管理血量系统和伤害处理
/// - 协调BallPhysics组件和UI显示
/// 
/// 【主要功能】：
/// - 物理控制：速度设置、移动检测、充能力计算
/// - 蓄力系统：蓄力进度、发射力度计算
/// - 战斗系统：血量管理、伤害处理、死亡逻辑
/// - 事件处理：球停止事件、攻击事件响应
/// 
/// 【设计原则】：
/// - 专注核心业务逻辑，不处理输入和状态管理
/// - 通过事件与其他组件通信
/// - 作为Player系统的业务逻辑中心
/// </summary>
public class PlayerCore : MonoBehaviour, MMEventListener<AttackEvent>
{
    [Header("数据设置")]
    public BallData ballData; // 物理数据（由Player设置）
    public BallCombatData combatData; // 战斗数据（由Player设置）
    
    [Header("蓄力设置")]
    public float maxChargingTime = 2f; // 最大蓄力时间
    public float chargingSpeed = 1f; // 蓄力速度
    
    // 核心组件
    private BallPhysics ballPhysics;
    private HealthBar healthBar;
    
    // 蓄力相关
    private float chargingStartTime = 0f;
    private float chargingPower = 0f;
    
    // 事件
    public System.Action OnBallStopped;
    public System.Action<float> OnHealthChanged;
    
    void Start()
    {
        InitializeCore();
    }
    
    void OnEnable()
    {
        this.MMEventStartListening<AttackEvent>();
    }
    
    void OnDisable()
    {
        this.MMEventStopListening<AttackEvent>();
    }
    
    #region 初始化
    
    /// <summary>
    /// 初始化核心组件
    /// </summary>
    void InitializeCore()
    {
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
            Debug.LogError("PlayerCore: 请设置 BallData 资源！");
        }
        
        // 订阅 BallPhysics 事件
        ballPhysics.OnBallStopped += OnBallStoppedHandler;
        
        // 初始化血量
        float currentHealth = combatData != null ? combatData.currentHealth : 100f;
        InitializeHealthBar(currentHealth);
        
        // 确保球体在初始化后完全停止
        if (ballPhysics != null)
        {
            ballPhysics.ResetBall();
            Debug.Log("PlayerCore: 初始化完成，球体已重置为停止状态");
        }
    }
    
    #endregion
    
    #region 物理控制
    
    /// <summary>
    /// 设置球体速度
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        if (ballPhysics != null)
        {
            ballPhysics.SetVelocity(velocity);
        }
    }
    
    /// <summary>
    /// 获取球体速度
    /// </summary>
    public Vector2 GetVelocity()
    {
        return ballPhysics != null ? ballPhysics.GetVelocity() : Vector2.zero;
    }
    
    /// <summary>
    /// 获取球体速度大小
    /// </summary>
    public float GetSpeed()
    {
        return ballPhysics != null ? ballPhysics.GetSpeed() : 0f;
    }
    
    /// <summary>
    /// 是否在物理移动
    /// </summary>
    public bool IsPhysicsMoving()
    {
        return ballPhysics != null && ballPhysics.IsMoving();
    }
    
    /// <summary>
    /// 是否在微调移动（由MovementController管理）
    /// </summary>
    public bool IsMicroMoving()
    {
        // 这个方法由PlayerMovementController实现
        PlayerMovementController movementController = GetComponent<PlayerMovementController>();
        return movementController != null && movementController.IsMicroMoving;
    }
    
    #endregion
    
    #region 蓄力系统
    
    /// <summary>
    /// 开始蓄力
    /// </summary>
    public void StartCharging()
    {
        chargingStartTime = Time.time;
        chargingPower = 0f;
        Debug.Log("PlayerCore: 开始蓄力");
    }
    
    /// <summary>
    /// 更新蓄力进度
    /// </summary>
    public void UpdateChargingProgress()
    {
        float chargingTime = Time.time - chargingStartTime;
        chargingPower = Mathf.Clamp01(chargingTime * chargingSpeed / maxChargingTime);
        
        // 蓄力完成
        if (chargingPower >= 1f)
        {
            chargingPower = 1f;
            Debug.Log("PlayerCore: 蓄力完成！");
        }
    }
    
    /// <summary>
    /// 获取蓄力进度
    /// </summary>
    public float GetChargingProgress()
    {
        UpdateChargingProgress();
        return chargingPower;
    }
    
    /// <summary>
    /// 重置蓄力
    /// </summary>
    public void ResetCharging()
    {
        chargingPower = 0f;
        chargingStartTime = 0f;
    }
    
    /// <summary>
    /// 发射蓄力攻击
    /// </summary>
    public void LaunchCharged()
    {
        Debug.Log($"PlayerCore: 开始发射 - 蓄力强度={chargingPower:F2}");
        
        // 消耗能量
        EnergySystem energySystem = FindFirstObjectByType<EnergySystem>();
        if (energySystem != null)
        {
            energySystem.ConsumeEnergy();
            Debug.Log("PlayerCore: 能量已消耗");
        }
        
        // 计算发射方向（朝向鼠标位置）
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        // 计算发射力度（基于蓄力强度）
        float maxForce = ballData != null ? ballData.maxSpeed * 2f : 20;
        float force = chargingPower * maxForce;
        
        Debug.Log($"PlayerCore: 发射参数 - 方向={direction}, 力度={force:F2}, 最大力度={maxForce}");
        
        // 发射
        Launch(direction, force);
        
        Debug.Log($"PlayerCore: 发射完成！力度: {force}, 方向: {direction}");
    }
    
    /// <summary>
    /// 发射球体
    /// </summary>
    public void Launch(Vector2 direction, float force)
    {
        if (ballPhysics == null) return;
        
        if (ballPhysics.IsMoving()) 
        {
            Debug.LogWarning($"PlayerCore: 球正在移动，无法发射");
            return;
        }
        
        // 检查方向向量是否有效
        if (direction.magnitude < 0.1f)
        {
            Debug.LogWarning("PlayerCore: 发射方向无效，球不会移动");
            return;
        }
        
        // 触发发射特效事件
        EventTrigger.Launch(transform.position, direction, gameObject);
        
        // 使用 BallPhysics 的发射方法
        float launchSpeed = force;
        Vector2 velocity = direction.normalized * launchSpeed;
        
        Debug.Log($"PlayerCore: 发射计算 - 方向={direction}, 力度={force}, 计算速度={velocity}, 速度大小={velocity.magnitude}");
        
        // 直接设置刚体速度
        if (ballPhysics.GetComponent<Rigidbody2D>() != null)
        {
            ballPhysics.GetComponent<Rigidbody2D>().linearVelocity = velocity;
            Debug.Log($"PlayerCore: 直接设置刚体速度: {velocity}");
        }
        else
        {
            ballPhysics.SetVelocity(velocity);
        }
        
        Debug.Log($"PlayerCore: 发射后实际速度={ballPhysics.GetVelocity()}, 速度大小={ballPhysics.GetSpeed()}");
    }
    
    #endregion
    
    #region 碰撞处理
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 撞击敌人时的处理
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 获取敌人的 BallPhysics 组件
            BallPhysics enemyBallPhysics = collision.gameObject.GetComponent<BallPhysics>();
            if (enemyBallPhysics != null)
            {
                // 检查是否还能获得充能力（基于速度）
                if (CanGetBoost())
                {
                    // 计算碰撞方向
                    Vector2 collisionDirection = (transform.position - collision.transform.position).normalized;
                    
                    // 给双方都添加充能力
                    Vector2 boostForce = collisionDirection * ballData.hitBoostForce * ballData.hitBoostMultiplier;
                    
                    // 给白球添加充能力
                    ballPhysics.ApplyForce(boostForce);
                    
                    // 给敌人添加充能力
                    enemyBallPhysics.ApplyForce(-boostForce);
                    
                    Debug.Log($"PlayerCore: 白球碰撞充能 - 白球获得力 {boostForce}，敌人获得力 {-boostForce} (速度:{ballPhysics.GetSpeed():F2})");
                }
                else
                {
                    Debug.Log($"PlayerCore: 白球碰撞敌人 - 速度过低({ballPhysics.GetSpeed():F2}<{ballData.boostSpeedThreshold})，无充能力");
                }
            }
            
            // 攻击事件和伤害处理由 Enemy.OnCollisionEnter2D 统一处理
        }
        
        // 撞击边界时的处理
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 检查是否还能获得充能力（基于速度）
            if (CanGetBoost())
            {
                // 计算撞墙方向（从墙壁指向白球）
                Vector2 wallDirection = ((Vector2)transform.position - collision.contacts[0].point).normalized;
                
                // 给白球添加撞墙充能力
                Vector2 wallBoostForce = wallDirection * ballData.hitBoostForce * ballData.hitBoostMultiplier;
                ballPhysics.ApplyForce(wallBoostForce);
                
                Debug.Log($"PlayerCore: 白球撞墙充能 - 获得力 {wallBoostForce} (速度:{ballPhysics.GetSpeed():F2})");
            }
            else
            {
                Debug.Log($"PlayerCore: 白球撞墙 - 速度过低({ballPhysics.GetSpeed():F2}<{ballData.boostSpeedThreshold})，无充能力");
            }
        }
    }
    
    /// <summary>
    /// 检查是否还能获得充能力（基于速度）
    /// </summary>
    bool CanGetBoost()
    {
        return ballPhysics != null && ballData != null && ballPhysics.GetSpeed() > ballData.boostSpeedThreshold;
    }
    
    #endregion
    
    #region 血量系统
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        float currentHealth = combatData != null ? combatData.currentHealth : 100f;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        float maxHealth = combatData != null ? combatData.maxHealth : 100f;
        Debug.Log($"PlayerCore: 受到伤害: {damage}, 当前血量: {currentHealth}/{maxHealth}");
        
        // 更新血条
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 死亡处理
    /// </summary>
    void Die()
    {
        Debug.Log("PlayerCore: 玩家死亡！");
        // 可以在这里添加死亡逻辑
    }
    
    /// <summary>
    /// 获取血量百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        float currentHealth = combatData != null ? combatData.currentHealth : 100f;
        float maxHealth = combatData != null ? combatData.maxHealth : 100f;
        return currentHealth / maxHealth;
    }
    
    /// <summary>
    /// 是否存活
    /// </summary>
    public bool IsAlive()
    {
        float currentHealth = combatData != null ? combatData.currentHealth : 100f;
        return currentHealth > 0;
    }
    
    #endregion
    
    #region 事件处理
    
    /// <summary>
    /// 球停止运动事件处理
    /// </summary>
    void OnBallStoppedHandler(BallPhysics ball)
    {
        OnBallStopped?.Invoke();
        
        // 通知状态机
        PlayerStateMachine stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.OnBallStopped();
        }
    }
    
    /// <summary>
    /// 处理攻击事件（MMEventListener接口实现）
    /// </summary>
    public void OnMMEvent(AttackEvent attackEvent)
    {
        // 检查自己是否是攻击目标
        if (attackEvent.Target == gameObject && attackEvent.Damage > 0f)
        {
            Debug.Log($"PlayerCore: 收到攻击事件，伤害: {attackEvent.Damage}");
            
            // 处理伤害
            TakeDamage(attackEvent.Damage);
        }
    }
    
    #endregion
    
    #region 血条系统
    
    /// <summary>
    /// 初始化血条
    /// </summary>
    void InitializeHealthBar(float currentHealth)
    {
        // 查找血条组件
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetTarget(transform);
            float maxHealth = combatData != null ? combatData.maxHealth : 100f;
            healthBar.UpdateHealth(currentHealth, maxHealth);
            Debug.Log("PlayerCore: 血条初始化完成");
        }
        else
        {
            Debug.LogWarning("PlayerCore: 未找到HealthBar组件，请确保血条预制体包含HealthBar脚本");
        }
    }
    
    #endregion
    
    #region 公共属性
    
    /// <summary>
    /// 蓄力强度 (0-1)
    /// </summary>
    public float ChargingPower => chargingPower;
    
    /// <summary>
    /// 蓄力进度百分比
    /// </summary>
    public float ChargingProgress => chargingPower * 100f;
    
    #endregion
    
    #region 重置和清理
    
    /// <summary>
    /// 重置为新回合
    /// </summary>
    public void ResetForNewTurn()
    {
        if (ballPhysics != null)
        {
            ballPhysics.ResetBallState();
        }
        
        PlayerMovementController movementController = GetComponent<PlayerMovementController>();
        if (movementController != null)
        {
            // 停止微调移动
            movementController.StopWASDMovement();
        }
        
        Debug.Log("PlayerCore: 重置为新回合");
    }
    
    #endregion
}
