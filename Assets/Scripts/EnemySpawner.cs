using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("敌人生成设置")]
    public GameObject enemyPrefab; // 敌人预制体
    public Transform enemyParent; // 敌人父物体（可选）
    
    [Header("生成范围设置")]
    public float minX = -10f; // 生成范围左边界
    public float maxX = 10f;  // 生成范围右边界
    public float minY = -5f;  // 生成范围下边界
    public float maxY = 5f;   // 生成范围上边界
    
    [Header("生成点设置")]
    public GameObject spawnPointPrefab; // 生成点预制体
    
    [Header("波次配置")]
    public int[] enemiesPerWave = {3, 2, 4, 1, 5}; // 每波敌人数数组，可以单独配置每一波
    
    [Header("测试设置")]
    public KeyCode spawnKey = KeyCode.Space; // 按空格生成敌人
    
    private List<Enemy> spawnedEnemies = new List<Enemy>(); // 已生成的敌人列表
    private Player targetPlayer;
    private EnemyController enemyController;
    
    // 生成点状态
    private List<GameObject> currentSpawnPoints = new List<GameObject>(); // 当前生成点列表
    private bool hasSpawnPoints = false; // 是否有生成点
    private int currentWave = 0; // 当前波次
    
    void Start()
    {
        targetPlayer = FindAnyObjectByType<Player>();
        enemyController = FindAnyObjectByType<EnemyController>();
        
        if (enemyPrefab == null)
        {
            Debug.LogError("敌人预制体未设置！");
        }
        
        Debug.Log($"EnemySpawner初始化完成，生成范围: X({minX}~{maxX}), Y({minY}~{maxY})");
    }
    
    void Update()
    {
        // 按空格键：先生成波次生成点，再生成敌人
        if (Input.GetKeyDown(spawnKey))
        {
            if (!hasSpawnPoints)
            {
                // 检查是否还有更多波次
                if (currentWave >= enemiesPerWave.Length)
                {
                    Debug.Log("所有波次已完成！");
                    return;
                }
                
                // 第一步：生成波次生成点
                CreateWaveSpawnPoints();
            }
            else
            {
                // 第二步：销毁所有生成点，生成敌人
                SpawnEnemiesFromPoints();
            }
        }
    }
    
    // 创建波次生成点
    public void CreateWaveSpawnPoints()
    {
        if (spawnPointPrefab == null)
        {
            Debug.LogError("生成点预制体未设置！");
            return;
        }
        
        if (currentWave >= enemiesPerWave.Length)
        {
            Debug.Log("所有波次已完成！");
            return;
        }
        
        currentWave++;
        currentSpawnPoints.Clear();
        
        // 获取当前波次的敌人数
        int currentWaveEnemies = enemiesPerWave[currentWave - 1];
        
        // 创建指定数量的生成点
        for (int i = 0; i < currentWaveEnemies; i++)
        {
            Vector3 spawnPosition = GenerateRandomPosition();
            GameObject spawnPoint = Instantiate(spawnPointPrefab, spawnPosition, Quaternion.identity);
            currentSpawnPoints.Add(spawnPoint);
        }
        
        hasSpawnPoints = true;
        Debug.Log($"第{currentWave}波：创建了{currentWaveEnemies}个生成点，按空格键生成敌人");
    }
    
    // 从所有生成点生成敌人
    public void SpawnEnemiesFromPoints()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("敌人预制体未设置，无法生成敌人！");
            return;
        }
        
        if (currentSpawnPoints.Count == 0)
        {
            Debug.Log("没有生成点，无法生成敌人！");
            return;
        }
        
        // 从每个生成点生成敌人
        foreach (GameObject spawnPoint in currentSpawnPoints)
        {
            if (spawnPoint != null)
            {
                Vector3 spawnPosition = spawnPoint.transform.position;
                GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
                
                // 获取敌人组件
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // 添加到列表
                    spawnedEnemies.Add(enemy);
                    
                    // 初始化敌人的攻击范围朝向
                    enemy.InitializeAttackRange();
                    
                    // 新敌人生成完成，静态事件会自动处理死亡通知
                    
                    // 等待一帧确保初始化完成，然后显示攻击范围预览
                    StartCoroutine(ShowPreviewAfterDelay(enemy));
                }
                else
                {
                    Debug.LogError("敌人预制体上没有Enemy组件！");
                    Destroy(enemyObj);
                }
            }
        }
        
        // 销毁所有生成点
        foreach (GameObject spawnPoint in currentSpawnPoints)
        {
            if (spawnPoint != null)
            {
                Destroy(spawnPoint);
            }
        }
        currentSpawnPoints.Clear();
        hasSpawnPoints = false;
        
        // 通知EnemyController刷新敌人列表
        if (enemyController != null)
        {
            enemyController.RefreshEnemies();
        }
        
        int currentWaveEnemies = enemiesPerWave[currentWave - 1];
        Debug.Log($"第{currentWave}波：生成了{currentWaveEnemies}个敌人，当前敌人数量: {spawnedEnemies.Count}");
    }
    
    // 在固定范围内生成随机位置
    Vector3 GenerateRandomPosition()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        return new Vector3(randomX, randomY, 0f);
    }
    
    // 延迟显示攻击范围预览
    System.Collections.IEnumerator ShowPreviewAfterDelay(Enemy enemy)
    {
        // 等待一帧确保所有初始化完成
        yield return null;
        
        ShowEnemyPreview(enemy);
    }
    
    // 为敌人显示攻击范围预览
    void ShowEnemyPreview(Enemy enemy)
    {
        if (enemy == null) return;
        
        // 获取敌人的攻击范围组件
        AttackRange attackRange = enemy.GetComponentInChildren<AttackRange>();
        if (attackRange != null)
        {
            // 先重新初始化攻击范围，确保方向正确
            enemy.InitializeAttackRange();
            
            // 然后显示预览
            attackRange.ShowPreview();
            Debug.Log($"为敌人 {enemy.name} 显示了攻击范围预览");
        }
        else
        {
            Debug.LogWarning($"敌人 {enemy.name} 没有找到AttackRange组件！");
        }
    }
    
    // 清除所有生成的敌人
    public void ClearAllEnemies()
    {
        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        spawnedEnemies.Clear();
        Debug.Log("已清除所有生成的敌人");
    }
    
    // 获取当前敌人数量
    public int GetEnemyCount()
    {
        return spawnedEnemies.Count;
    }
    
    // 获取所有敌人
    public List<Enemy> GetAllEnemies()
    {
        return new List<Enemy>(spawnedEnemies);
    }
    
    void OnGUI()
    {
        // 检查是否所有波次已完成
        if (currentWave >= enemiesPerWave.Length)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "所有波次已完成！");
            GUI.Label(new Rect(10, 30, 300, 20), $"总敌人数: {spawnedEnemies.Count}");
            return;
        }
        
        // 在屏幕上显示操作提示
        if (!hasSpawnPoints)
        {
            int nextWave = currentWave + 1;
            int nextWaveEnemies = enemiesPerWave[currentWave];
            GUI.Label(new Rect(10, 10, 300, 20), $"按 {spawnKey} 键创建第{nextWave}波生成点 ({nextWaveEnemies}个)");
        }
        else
        {
            int currentWaveEnemies = enemiesPerWave[currentWave - 1];
            GUI.Label(new Rect(10, 10, 300, 20), $"按 {spawnKey} 键生成第{currentWave}波敌人 ({currentWaveEnemies}个)");
        }
        GUI.Label(new Rect(10, 30, 300, 20), $"当前敌人数量: {spawnedEnemies.Count}");
        GUI.Label(new Rect(10, 50, 300, 20), $"生成点状态: {(hasSpawnPoints ? $"已创建{currentSpawnPoints.Count}个" : "无")}");
        GUI.Label(new Rect(10, 70, 300, 20), $"波次进度: {currentWave}/{enemiesPerWave.Length}");
    }
    
    // 获取剩余敌人数（包括待生成的）
    public int GetRemainingEnemiesCount()
    {
        int count = 0;
        
        // 当前场景中的敌人
        count += spawnedEnemies.Count;
        
        // 待生成的敌人（只计算当前波次及以后的波次）
        if (currentWave < enemiesPerWave.Length)
        {
            // 当前波次剩余敌人（如果当前波次还没生成完）
            if (hasSpawnPoints)
            {
                count += currentSpawnPoints.Count; // 当前波次的生成点数量
            }
            
            // 后续波次的敌人
            for (int i = currentWave + 1; i < enemiesPerWave.Length; i++)
            {
                count += enemiesPerWave[i];
            }
        }
        
        Debug.Log($"EnemySpawner剩余敌人数计算: 当前敌人={spawnedEnemies.Count}, 当前波次={currentWave}, 有生成点={hasSpawnPoints}, 生成点数量={currentSpawnPoints.Count}, 总计={count}");
        
        return count;
    }
}
