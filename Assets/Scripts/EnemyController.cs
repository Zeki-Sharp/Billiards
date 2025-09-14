using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("敌人阶段设置")]
    public float attackPhaseDuration = 2f; // 攻击阶段持续时间
    public float telegraphPhaseDuration = 1f; // 预备阶段持续时间
    
    [Header("敌人行动设置")]
    public float attackForce = 8f;         // 攻击力度
    public float attackRange = 3f;         // 攻击范围
    public float moveDistance = 2f;        // 移动距离
    public float moveSpeed = 3f;           // 移动速度
    
    public enum EnemyPhase
    {
        AttackPhase,    // 攻击阶段
        MovePhase,      // 移动阶段
        TelegraphPhase  // 预备阶段
    }
    
    private EnemyPhase currentPhase = EnemyPhase.AttackPhase;
    private float phaseTimer = 0f;
    private Enemy[] enemies;
    private WhiteBall targetBall;
    private HoleManager holeManager;
    private bool isActive = false;
    private bool hasCompleted = false; // 防止重复完成
    
    // 移动相关
    private bool isMoving = false;
    private Coroutine moveCoroutine;
    
    // 事件
    public System.Action<EnemyPhase> OnPhaseChanged;
    public System.Action OnEnemyPhaseComplete;
    
    void Start()
    {
        // 查找所有敌人和白球
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        targetBall = FindAnyObjectByType<WhiteBall>();
        holeManager = FindAnyObjectByType<HoleManager>();
        
        Debug.Log($"EnemyController初始化: 找到{enemies.Length}个敌人");
    }
    
    public void StartEnemyPhase()
    {
        isActive = true;
        hasCompleted = false;
        SetPhase(EnemyPhase.AttackPhase);
        Debug.Log("开始敌人阶段 - 攻击阶段");
    }
    
    public void ShowEnemyPreview()
    {
        // 确保enemies数组已初始化
        if (enemies == null)
        {
            enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            Debug.Log($"ShowEnemyPreview: 重新初始化enemies数组，找到{enemies.Length}个敌人");
        }
        
        // 只显示攻击范围预览，不开始完整的敌人阶段
        ExecuteTelegraph();
        Debug.Log("显示敌人攻击范围预览");
    }
    
    public void StopEnemyPhase()
    {
        isActive = false;
        Debug.Log("结束敌人阶段");
    }
    
    // 更新敌人列表
    public void RefreshEnemies()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Debug.Log($"刷新敌人列表: 找到{enemies.Length}个敌人");
    }
    
    void Update()
    {
        if (!isActive) return;
        
        HandleCurrentPhase();
    }
    
    void HandleCurrentPhase()
    {
        phaseTimer -= Time.deltaTime;
        
        switch (currentPhase)
        {
            case EnemyPhase.AttackPhase:
                HandleAttackPhase();
                break;
            case EnemyPhase.MovePhase:
                HandleMovePhase();
                break;
            case EnemyPhase.TelegraphPhase:
                HandleTelegraphPhase();
                break;
        }
    }
    
    void HandleAttackPhase()
    {
        // 攻击阶段：所有敌人向白球发射
        if (phaseTimer <= 0)
        {
            ExecuteAttack();
            // 延迟一帧再转换，避免在同一帧内多次转换
            StartCoroutine(DelayedPhaseTransition(EnemyPhase.MovePhase));
        }
    }
    
    void HandleMovePhase()
    {
        // 移动阶段：所有敌人朝白球方向移动
        if (phaseTimer <= 0 && !isMoving)
        {
            ExecuteMove();
        }
    }
    
    void HandleTelegraphPhase()
    {
        // 预备阶段：敌人准备下一轮行动
        if (phaseTimer <= 0 && !hasCompleted)
        {
            ExecuteTelegraph();
            // 延迟一帧再完成，避免在同一帧内多次转换
            StartCoroutine(DelayedPhaseComplete());
        }
    }
    
    System.Collections.IEnumerator DelayedPhaseTransition(EnemyPhase nextPhase)
    {
        yield return null; // 等待一帧
        SetPhase(nextPhase);
    }
    
    System.Collections.IEnumerator DelayedPhaseComplete()
    {
        yield return null; // 等待一帧
        hasCompleted = true;
        OnEnemyPhaseComplete?.Invoke();
        Debug.Log("敌人阶段完成事件已触发");
    }
    
    void ExecuteAttack()
    {
        Debug.Log("执行攻击阶段");
        
        // 显示所有敌人的攻击范围并攻击
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive()) continue;
            
            AttackRange attackRange = enemy.GetComponentInChildren<AttackRange>();
            if (attackRange != null)
            {
                attackRange.ShowAttack();
            }
        }
        
        // 检测伤害
        CheckDamage();
    }
    
    void ExecuteMove()
    {
        if (isMoving) return; // 如果已经在移动，直接返回
        
        Debug.Log("执行移动阶段");
        
        // 隐藏所有敌人的攻击范围
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive()) continue;
            
            AttackRange attackRange = enemy.GetComponentInChildren<AttackRange>();
            if (attackRange != null)
            {
                attackRange.Hide();
            }
        }
        
        // 开始移动
        moveCoroutine = StartCoroutine(MoveEnemies());
    }
    
    IEnumerator MoveEnemies()
    {
        isMoving = true;
        
        // 为每个敌人计算移动目标位置
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive()) continue;
            
            // 计算朝向白球的方向
            Vector2 direction = (targetBall.transform.position - enemy.transform.position).normalized;
            
            // 计算目标位置（当前位置 + 方向 * 移动距离）
            Vector2 targetPosition = (Vector2)enemy.transform.position + direction * moveDistance;
            
            // 开始移动协程
            StartCoroutine(MoveEnemy(enemy, targetPosition));
        }
        
        // 等待移动完成（使用phaseTimer，由距离和速度自动计算）
        yield return new WaitForSeconds(phaseTimer);
        
        isMoving = false;
        moveCoroutine = null; // 清空协程引用
        Debug.Log("所有敌人移动完成");
        
        // 移动完成后直接切换到下一个阶段
        StartCoroutine(DelayedPhaseTransition(EnemyPhase.TelegraphPhase));
    }
    
    IEnumerator MoveEnemy(Enemy enemy, Vector2 targetPosition)
    {
        Vector2 startPosition = enemy.transform.position;
        float moveTime = moveDistance / moveSpeed;
        float elapsedTime = 0f;
        
        Debug.Log($"敌人 {enemy.name} 开始移动: 从 {startPosition} 到 {targetPosition}, 移动时间: {moveTime}秒");
        
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveTime;
            
            // 使用平滑插值移动
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, progress);
            enemy.transform.position = newPosition;
            
            yield return null;
        }
        
        // 确保到达目标位置
        enemy.transform.position = targetPosition;
        Debug.Log($"敌人 {enemy.name} 移动完成，最终位置: {enemy.transform.position}");
    }
    
    void ExecuteTelegraph()
    {
        Debug.Log("执行预备阶段");
        
        // 激活随机Hole
        if (holeManager != null)
        {
            holeManager.ActivateRandomHoles();
        }
        
        // 重新计算并显示所有敌人的攻击范围
        if (enemies != null)
        {
            Debug.Log($"ExecuteTelegraph: 找到{enemies.Length}个敌人");
            foreach (Enemy enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive()) 
                {
                    Debug.Log($"敌人为null或已死亡，跳过");
                    continue;
                }
                
                AttackRange attackRange = enemy.GetComponentInChildren<AttackRange>();
                if (attackRange != null)
                {
                    Debug.Log($"为敌人 {enemy.name} 显示攻击范围预览");
                    attackRange.ShowPreview();
                }
                else
                {
                    Debug.LogWarning($"敌人 {enemy.name} 没有找到AttackRange组件");
                }
            }
        }
        else
        {
            Debug.LogWarning("enemies数组为null，无法执行预备阶段");
        }
    }
    
    void CheckDamage()
    {
        if (targetBall == null) return;
        
        Vector2 whiteBallPos = targetBall.transform.position;
        int totalDamage = 0;
        int hitCount = 0;
        
        // 从白球位置垂直发射射线，检测所有攻击范围
        RaycastHit2D[] hits = Physics2D.RaycastAll(whiteBallPos, Vector2.up, 0.1f);
        
        Debug.Log($"从白球位置 {whiteBallPos} 发射射线，检测到 {hits.Length} 个碰撞体");
        
        // 检查每个碰撞体是否是敌人的攻击范围
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                AttackRange attackRange = hit.collider.GetComponent<AttackRange>();
                if (attackRange != null)
                {
                    // 找到对应的敌人
                    Enemy enemy = attackRange.GetComponentInParent<Enemy>();
                    if (enemy != null && enemy.IsAlive())
                    {
                        float enemyDamage = enemy.combatData != null ? enemy.combatData.damage : 10f;
                        totalDamage += Mathf.RoundToInt(enemyDamage);
                        hitCount++;
                        Debug.Log($"射线检测命中敌人 {enemy.name} 的攻击范围，伤害: {enemyDamage}");
                    }
                }
            }
        }
        
        if (hitCount > 0)
        {
            Debug.Log($"白球受到 {hitCount} 个敌人攻击，总伤害: {totalDamage}");
            
            // 注意：攻击事件已由物理碰撞系统处理，这里不再重复触发
            // 直接处理伤害，避免重复的攻击事件
            if (targetBall != null)
            {
                targetBall.TakeDamage(totalDamage);
            }
        }
        else
        {
            Debug.Log("白球未受到任何攻击");
        }
    }
    
    void SetPhase(EnemyPhase newPhase)
    {
        if (currentPhase != newPhase)
        {
            Debug.Log($"敌人阶段转换: {currentPhase} -> {newPhase}");
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);
            
            // 设置阶段计时器
            switch (currentPhase)
            {
                case EnemyPhase.AttackPhase:
                    phaseTimer = attackPhaseDuration;
                    break;
                case EnemyPhase.MovePhase:
                    phaseTimer = moveDistance / moveSpeed; // 根据距离和速度计算时间
                    break;
                case EnemyPhase.TelegraphPhase:
                    phaseTimer = telegraphPhaseDuration;
                    break;
            }
            
            Debug.Log($"敌人阶段切换到: {currentPhase}, 持续时间: {phaseTimer}秒");
        }
    }
    
    // 公共方法
    public EnemyPhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    public float GetPhaseTimer()
    {
        return phaseTimer;
    }
    
    public bool IsActive()
    {
        return isActive;
    }
}
