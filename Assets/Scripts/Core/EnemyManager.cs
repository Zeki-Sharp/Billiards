using UnityEngine;

/// <summary>
/// 敌人管理器 - 管理敌人的移动+射击循环
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("敌人设置")]
    [SerializeField] private float moveDistance = 1f; // 每次移动距离
    [SerializeField] private float moveDuration = 1f; // 移动持续时间
    [SerializeField] private float shootCooldown = 2f; // 射击冷却时间
    [SerializeField] private int maxEnemies = 10; // 最大敌人数量
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    
    private Enemy[] enemies;
    private float[] enemyTimers;
    private bool[] enemyCanShoot;
    
    // 事件
    public System.Action OnEnemyMoveStart; // 敌人移动开始
    public System.Action OnEnemyShootStart; // 敌人射击开始
    public System.Action OnEnemyBehaviorComplete; // 敌人行为完成
    
    void Start()
    {
        InitializeEnemies();
    }
    
    void Update()
    {
        UpdateEnemyBehavior();
    }
    
    void InitializeEnemies()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        enemyTimers = new float[enemies.Length];
        enemyCanShoot = new bool[enemies.Length];
        
        // 初始化敌人状态
        for (int i = 0; i < enemies.Length; i++)
        {
            enemyTimers[i] = 0f;
            enemyCanShoot[i] = true;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyManager: 初始化 {enemies.Length} 个敌人");
        }
    }
    
    void UpdateEnemyBehavior()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;
            
            enemyTimers[i] += Time.deltaTime;
            
            // 移动+射击循环
            if (enemyTimers[i] >= moveDuration)
            {
                // 移动一步
                MoveEnemyOneStep(i);
                OnEnemyMoveStart?.Invoke();
                
                // 射击
                if (enemyCanShoot[i])
                {
                    ShootEnemy(i);
                    OnEnemyShootStart?.Invoke();
                    enemyCanShoot[i] = false;
                }
                
                // 重置计时器
                enemyTimers[i] = 0f;
            }
        }
    }
    
    void MoveEnemyOneStep(int enemyIndex)
    {
        if (enemies[enemyIndex] == null) return;
        
        // 简单的随机移动
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 targetPosition = enemies[enemyIndex].transform.position + (Vector3)(randomDirection * moveDistance);
        
        // 这里应该调用敌人的移动方法
        // enemies[enemyIndex].MoveTo(targetPosition);
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyManager: 敌人 {enemyIndex} 移动到 {targetPosition}");
        }
    }
    
    void ShootEnemy(int enemyIndex)
    {
        if (enemies[enemyIndex] == null) return;
        
        // 这里应该调用敌人的射击方法
        // enemies[enemyIndex].Shoot();
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyManager: 敌人 {enemyIndex} 射击");
        }
    }
    
    public void SetEnemies(Enemy[] newEnemies)
    {
        enemies = newEnemies;
        enemyTimers = new float[enemies.Length];
        enemyCanShoot = new bool[enemies.Length];
        
        for (int i = 0; i < enemies.Length; i++)
        {
            enemyTimers[i] = 0f;
            enemyCanShoot[i] = true;
        }
    }
    
    public int GetEnemyCount()
    {
        return enemies != null ? enemies.Length : 0;
    }
}
