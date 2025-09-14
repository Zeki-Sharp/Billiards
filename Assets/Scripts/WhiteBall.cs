using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;


/// <summary>
/// 白球脚本 - 负责白球的物理逻辑和碰撞逻辑
/// </summary>
public class WhiteBall : MonoBehaviour, MMEventListener<AttackEvent>
{
    [Header("数据设置")]
    public BallData ballData; // 物理数据
    public BallCombatData combatData; // 战斗数据
    
    private BallPhysics ballPhysics;
    private bool isMicroMoving = false; // 是否正在进行微调移动
    private HealthBar healthBar; // 血条组件
    private float currentHealth; // 当前血量
    
    // 事件
    public System.Action OnBallStopped;
    public System.Action<float> OnHealthChanged; // 血量变化事件
    
    void Start()
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
            Debug.LogError("WhiteBall: 请设置 BallData 资源！");
        }
        
        // 订阅 BallPhysics 事件
        ballPhysics.OnBallStopped += OnBallStoppedHandler;
        
        // 初始化血量
        currentHealth = combatData != null ? combatData.currentHealth : 100f;
        
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
    
    void Update()
    {
        // BallPhysics 会自动处理移动检测，这里不需要额外处理
    }
    
    // BallPhysics 事件处理
    void OnBallStoppedHandler(BallPhysics ball)
    {
        OnBallStopped?.Invoke();
    }
    
    // 物理设置现在由 BallPhysics 处理
    
    public void Launch(Vector2 direction, float force)
    {
        if (ballPhysics == null) return;
        
        if (ballPhysics.IsMoving()) 
        {
            Debug.LogWarning($"白球无法发射: 球正在移动");
            return;
        }
        
        // 检查方向向量是否有效
        if (direction.magnitude < 0.1f)
        {
            Debug.LogWarning("发射方向无效，白球不会移动");
            return;
        }
        
        // 触发发射特效事件
        EventTrigger.Launch(transform.position, direction, gameObject);
        
        // 使用 BallPhysics 的发射方法
        // force 直接作为速度值，ballData.maxSpeed 只作为上限检查
        float launchSpeed = force;
        Vector2 velocity = direction.normalized * launchSpeed;
        
        Debug.Log($"白球发射计算: 方向={direction}, 力度={force}, 计算速度={velocity}, 速度大小={velocity.magnitude}");
        
        // 直接设置刚体速度，绕过 BallPhysics 的复杂逻辑
        if (ballPhysics != null && ballPhysics.GetComponent<Rigidbody2D>() != null)
        {
            ballPhysics.GetComponent<Rigidbody2D>().linearVelocity = velocity;
            Debug.Log($"直接设置刚体速度: {velocity}");
        }
        else
        {
            ballPhysics.SetVelocity(velocity);
        }
        
        Debug.Log($"白球发射后: 实际速度={ballPhysics.GetVelocity()}, 速度大小={ballPhysics.GetSpeed()}");
    }
    
    
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
                    
                    Debug.Log($"白球碰撞充能：白球获得力 {boostForce}，敌人获得力 {-boostForce} (速度:{ballPhysics.GetSpeed():F2})");
                }
                else
                {
                    Debug.Log($"白球碰撞敌人：速度过低({ballPhysics.GetSpeed():F2}<{ballData.boostSpeedThreshold})，无充能力");
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
                
                Debug.Log($"白球撞墙充能：获得力 {wallBoostForce} (速度:{ballPhysics.GetSpeed():F2})");
            }
            else
            {
                Debug.Log($"白球撞墙：速度过低({ballPhysics.GetSpeed():F2}<{ballData.boostSpeedThreshold})，无充能力");
            }
            
        }
    }
    
    
    // 检查是否还能获得充能力（基于速度）
    private bool CanGetBoost()
    {
        return ballPhysics != null && ballData != null && ballPhysics.GetSpeed() > ballData.boostSpeedThreshold;
    }
    
    public float GetCurrentSpeed()
    {
        return ballPhysics != null ? ballPhysics.GetSpeed() : 0f;
    }
    
    public bool IsMoving()
    {
        return (ballPhysics != null && ballPhysics.IsMoving()) || isMicroMoving;
    }
    
    public void MicroMove(Vector2 direction, float distance)
    {
        // 检查方向向量是否有效
        if (direction.magnitude < 0.1f)
        {
            Debug.LogWarning("微调方向无效，白球不会移动");
            return;
        }
        
        // 如果正在进行微调移动，停止当前的移动
        if (isMicroMoving)
        {
            StopAllCoroutines();
            isMicroMoving = false;
        }
        
        // 如果白球正在物理移动，不能进行微调
        if (ballPhysics != null && ballPhysics.IsMoving())
        {
            Debug.LogWarning("白球正在物理移动中，无法进行微调");
            return;
        }
        
        // 获取当前位置
        Vector2 currentPosition = transform.position;
        
        // 计算目标位置
        Vector2 targetPosition = currentPosition + direction.normalized * distance;
        
        // 调试信息
        Debug.Log($"白球微调: 从{currentPosition}到{targetPosition}");
        
        // 开始平滑移动协程
        StartCoroutine(SmoothMicroMove(targetPosition));
    }
    
    private System.Collections.IEnumerator SmoothMicroMove(Vector2 targetPosition)
    {
        isMicroMoving = true;
        Vector2 startPosition = transform.position;
        float distance = Vector2.Distance(startPosition, targetPosition);
        float duration = distance / (combatData != null ? combatData.microMoveSpeed : 5f);
        float elapsedTime = 0f;
        
        while (elapsedTime < duration && isMicroMoving) // 添加isMicroMoving检查
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // 使用平滑插值
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            
            yield return null;
        }
        
        // 只有在没有被中断的情况下才设置最终位置
        if (isMicroMoving)
        {
            // 确保最终位置准确
            transform.position = targetPosition;
            isMicroMoving = false;
        }
    }
    
    public void TakeDamage(float damage)
    {
        // 白球受到伤害的处理
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        float maxHealth = combatData != null ? combatData.maxHealth : 100f;
        Debug.Log($"白球受到伤害: {damage}, 当前血量: {currentHealth}/{maxHealth}");
        
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
    
    void InitializeHealthBar()
    {
        // 查找血条组件
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetTarget(transform);
            float maxHealth = combatData != null ? combatData.maxHealth : 100f;
            healthBar.UpdateHealth(currentHealth, maxHealth);
            Debug.Log("白球血条初始化完成");
        }
        else
        {
            Debug.LogWarning("白球未找到HealthBar组件，请确保血条预制体包含HealthBar脚本");
        }
    }
    
    void Die()
    {
        Debug.Log("白球死亡！");
        
        // 显示失败界面
        UIController uiController = FindAnyObjectByType<UIController>();
        if (uiController != null)
        {
            uiController.ShowGameOverScreen();
        }
    }
    
    public float GetHealthPercentage()
    {
        float maxHealth = combatData != null ? combatData.maxHealth : 100f;
        return currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    /// <summary>
    /// 处理攻击事件（MMEventListener接口实现）
    /// 当自己是攻击目标时处理伤害
    /// </summary>
    public void OnMMEvent(AttackEvent attackEvent)
    {
        // 检查自己是否是攻击目标
        if (attackEvent.Target == gameObject && attackEvent.Damage > 0f)
        {
            Debug.Log($"WhiteBall 收到攻击事件，伤害: {attackEvent.Damage}");
            
            // 处理伤害
            TakeDamage(attackEvent.Damage);
        }
    }
    
    public void ResetForNewTurn()
    {
        isMicroMoving = false;
        if (ballPhysics != null)
        {
            ballPhysics.ResetBallState();
        }
        Debug.Log("白球重置为新回合");
    }
    
    
}
