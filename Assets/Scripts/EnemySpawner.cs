using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("敌人生成设置")]
    public EnemyData[] enemyDataList; // 敌人配置列表
    public Transform enemyParent; // 敌人父物体（可选）
    
    [Header("生成范围设置")]
    public float minX = -10f; // 生成范围左边界
    public float maxX = 10f;  // 生成范围右边界
    public float minY = -5f;  // 生成范围下边界
    public float maxY = 5f;   // 生成范围上边界
    
    [Header("波次配置")]
    public float waveInterval = 10f; // 波次生成间隔（秒）
    public int[] enemiesPerWave = {3, 2, 4, 1, 5}; // 每波敌人数数组
    public int maxWaves = 10; // 最大波次数（0表示无限）
    
    [Header("生成控制")]
    public bool autoStart = true; // 是否自动开始生成
    public bool loopWaves = true; // 是否循环波次
    
    [Header("测试设置")]
    public KeyCode spawnKey = KeyCode.Space; // 手动触发下一波
    public KeyCode toggleKey = KeyCode.T; // 切换生成开关
    
    private List<Enemy> spawnedEnemies = new List<Enemy>(); // 已生成的敌人列表
    private Player targetPlayer;
    
    // 生成状态
    private int currentWave = 0; // 当前波次索引
    private float lastWaveTime = 0f; // 上次生成波次的时间
    private bool isSpawning = false; // 是否正在生成
    private int totalWavesSpawned = 0; // 已生成的波次总数
    
    void Start()
    {
        targetPlayer = FindAnyObjectByType<Player>();
        
        if (enemyDataList == null || enemyDataList.Length == 0)
        {
            Debug.LogError("敌人配置列表未设置或为空！");
        }
        
        if (autoStart)
        {
            StartSpawning();
        }
        
        Debug.Log($"EnemySpawner初始化完成，生成范围: X({minX}~{maxX}), Y({minY}~{maxY}), 波次间隔: {waveInterval}秒");
    }
    
    void Update()
    {
        // 自动生成波次
        if (isSpawning)
        {
            if (Time.time - lastWaveTime >= waveInterval)
            {
                SpawnNextWave();
            }
        }
        
        // 手动控制
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnNextWave();
        }
        
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleSpawning();
        }
    }
    
    /// <summary>
    /// 开始生成
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
        lastWaveTime = Time.time;
        Debug.Log("敌人生成已开始");
    }
    
    /// <summary>
    /// 停止生成
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("敌人生成已停止");
    }
    
    /// <summary>
    /// 切换生成状态
    /// </summary>
    public void ToggleSpawning()
    {
        if (isSpawning)
        {
            StopSpawning();
        }
        else
        {
            StartSpawning();
        }
    }
    
    /// <summary>
    /// 生成下一波敌人
    /// </summary>
    public void SpawnNextWave()
    {
        // 检查是否达到最大波次限制
        if (maxWaves > 0 && totalWavesSpawned >= maxWaves)
        {
            Debug.Log("已达到最大波次限制，停止生成");
            StopSpawning();
            return;
        }
        
        // 获取当前波次的敌人数
        int enemyCount = enemiesPerWave[currentWave];
        
        // 生成敌人
        SpawnEnemies(enemyCount);
        
        // 更新状态
        lastWaveTime = Time.time;
        totalWavesSpawned++;
        
        Debug.Log($"第{totalWavesSpawned}波生成完成，敌人数量: {enemyCount}，当前敌人总数: {spawnedEnemies.Count}");
        
        // 移动到下一个波次配置
        currentWave++;
        if (currentWave >= enemiesPerWave.Length)
        {
            if (loopWaves)
            {
                currentWave = 0; // 循环到第一个波次
            }
            else
            {
                Debug.Log("所有波次配置已完成");
                StopSpawning();
            }
        }
    }
    
    /// <summary>
    /// 生成指定数量的敌人
    /// </summary>
    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            EnemyData selectedEnemyData = SelectEnemyData();
            SpawnEnemyFromData(selectedEnemyData);
        }
    }
    
    /// <summary>
    /// 根据权重选择敌人配置
    /// </summary>
    EnemyData SelectEnemyData()
    {
        if (enemyDataList == null || enemyDataList.Length == 0)
        {
            Debug.LogError("敌人配置列表为空！");
            return null;
        }
        
        // 计算总权重
        int totalWeight = 0;
        foreach (var enemyData in enemyDataList)
        {
            totalWeight += enemyData.spawnWeight;
        }
        
        // 随机选择
        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (var enemyData in enemyDataList)
        {
            currentWeight += enemyData.spawnWeight;
            if (randomWeight < currentWeight)
            {
                return enemyData;
            }
        }
        
        // 默认返回第一个
        return enemyDataList[0];
    }
    
    /// <summary>
    /// 根据配置数据生成敌人
    /// </summary>
    void SpawnEnemyFromData(EnemyData enemyData)
    {
        if (enemyData == null || enemyData.enemyPrefab == null)
        {
            Debug.LogError("敌人配置数据无效！");
            return;
        }
        
        Vector3 spawnPosition = GenerateRandomPosition();
        GameObject enemyObj = Instantiate(enemyData.enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
        
        // 获取敌人组件并设置配置数据
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.enemyData = enemyData;
            spawnedEnemies.Add(enemy);
            enemy.InitializeAttackRange();
        }
        else
        {
            Debug.LogError("敌人预制体上没有Enemy组件！");
            Destroy(enemyObj);
        }
    }
    
    
    // 在固定范围内生成随机位置
    Vector3 GenerateRandomPosition()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        return new Vector3(randomX, randomY, 0f);
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
        GUI.Label(new Rect(10, 10, 300, 20), $"生成状态: {(isSpawning ? "运行中" : "已停止")}");
        GUI.Label(new Rect(10, 30, 300, 20), $"当前敌人数量: {spawnedEnemies.Count}");
        GUI.Label(new Rect(10, 50, 300, 20), $"已生成波次: {totalWavesSpawned}/{(maxWaves > 0 ? maxWaves.ToString() : "∞")}");
        
        if (isSpawning)
        {
            float timeUntilNext = waveInterval - (Time.time - lastWaveTime);
            GUI.Label(new Rect(10, 70, 300, 20), $"下次生成倒计时: {timeUntilNext:F1}秒");
        }
        
        GUI.Label(new Rect(10, 90, 300, 20), $"按 {spawnKey} 手动生成下一波");
        GUI.Label(new Rect(10, 110, 300, 20), $"按 {toggleKey} 切换生成开关");
        
        if (maxWaves > 0 && totalWavesSpawned >= maxWaves)
        {
            GUI.Label(new Rect(10, 130, 300, 20), "已达到最大波次限制！");
        }
    }
    
    // 获取剩余敌人数（包括待生成的）
    public int GetRemainingEnemiesCount()
    {
        int count = spawnedEnemies.Count; // 当前场景中的敌人
        
        // 如果生成已停止，只返回当前敌人数量
        if (!isSpawning)
        {
            return count;
        }
        
        // 计算待生成的敌人数量
        if (maxWaves > 0)
        {
            // 有最大波次限制的情况
            int remainingWaves = maxWaves - totalWavesSpawned;
            if (remainingWaves > 0)
            {
                // 计算剩余波次的敌人数量
                int currentWaveIndex = currentWave;
                for (int i = 0; i < remainingWaves; i++)
                {
                    count += enemiesPerWave[currentWaveIndex];
                    currentWaveIndex++;
                    if (currentWaveIndex >= enemiesPerWave.Length)
                    {
                        if (loopWaves)
                        {
                            currentWaveIndex = 0; // 循环到第一个波次
                        }
                        else
                        {
                            break; // 不循环，停止计算
                        }
                    }
                }
            }
        }
        else
        {
            // 无限生成的情况，返回当前敌人数量（因为无法预知未来会生成多少）
            // 或者可以返回一个估算值
        }
        
        return count;
    }
}
