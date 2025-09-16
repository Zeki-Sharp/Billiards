using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveConfig
{
    [Header("波次信息")]
    public string waveName = "Wave"; // 波次名称（可选）
    
    [Header("敌人配置")]
    public List<EnemySpawnInfo> enemySpawns = new List<EnemySpawnInfo>(); // 这一波要生成的敌人列表
    
    [Header("波次设置")]
    public float waveDelay = 0f; // 这一波开始前的延迟（秒）
    public bool waitForPreviousWave = true; // 是否等待上一波敌人全部死亡后再开始这一波
}

[System.Serializable]
public class EnemySpawnInfo
{
    [Header("敌人数据")]
    public EnemyData enemyData; // 敌人配置数据
    
    [Header("生成设置")]
    public int count = 1; // 生成数量
    public float spawnDelay = 0f; // 相对于波次开始的延迟（秒）
    public float spawnInterval = 0.5f; // 多个敌人之间的生成间隔（秒）
    
    [Header("位置设置")]
    public bool useRandomPosition = true; // 是否使用随机位置
    public Vector2 customPosition = Vector2.zero; // 自定义位置（当useRandomPosition为false时使用）
}
