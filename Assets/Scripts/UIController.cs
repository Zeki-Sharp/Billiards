using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("UI元素")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI microMoveTimerText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI enemyPhaseText;
    public TextMeshProUGUI remainingEnemiesText; // 剩余敌人数显示
    public TextMeshProUGUI shotCountText; // 出杆数显示
    
    [Header("胜利界面")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryTitleText;
    public TextMeshProUGUI victoryStatsText;
    public Button victoryRestartButton;
    
    [Header("失败界面")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverStatsText;
    public Button gameOverRestartButton;
    
    private GameManager gameManager;
    private EnemyController enemyController;
    private EnemySpawner enemySpawner;
    private int shotCount = 0; // 出杆数
    private bool gameEnded = false; // 游戏是否已结束
    private int totalEnemies = 0; // 敌人总数
    private int lastEnemyCount = 0; // 上一次的场上敌人数量
    
    // void Start()
    // {
    //     // 重置游戏状态
    //     ResetGameState();
        
    //     gameManager = GameManager.Instance;
    //     enemyController = FindAnyObjectByType<EnemyController>();
    //     enemySpawner = FindAnyObjectByType<EnemySpawner>();
        
    //     if (gameManager != null)
    //     {
    //         gameManager.OnPhaseChanged += OnPhaseChanged;
    //     }
        
    //     if (enemyController != null)
    //     {
    //         enemyController.OnPhaseChanged += OnEnemyPhaseChanged;
    //     }
        
        
    //     // 监听白球发射（通过监听白球移动状态）
    //     StartCoroutine(MonitorWhiteBallShots());
        
    //     // 设置按钮监听
    //     if (victoryRestartButton != null)
    //     {
    //         victoryRestartButton.onClick.AddListener(RestartGame);
    //         Debug.Log("胜利界面重启按钮监听已设置");
    //     }
        
    //     if (gameOverRestartButton != null)
    //     {
    //         gameOverRestartButton.onClick.AddListener(RestartGame);
    //         Debug.Log("失败界面重启按钮监听已设置");
    //     }
        
    //     // 初始化UI
    //     InitializeUI();
        
    //     // 计算敌人总数
    //     CalculateTotalEnemies();
    // }
    
    
    // // 重置游戏状态
    // void ResetGameState()
    // {
    //     shotCount = 0;
    //     gameEnded = false;
    //     totalEnemies = 0;
    //     lastEnemyCount = 0;
    //     Time.timeScale = 1f; // 确保时间正常
    //     Debug.Log("游戏状态已重置");
    // }
    
    // void Update()
    // {
    //     UpdateUI();
    //     UpdateEnemyCount();
    // }
    
    // void UpdateUI()
    // {
    //     if (gameManager == null || gameEnded) return;
        
    //     // 更新状态文本
    //     if (statusText != null)
    //     {
    //         switch (gameManager.GetCurrentPhase())
    //         {
    //             case GameManager.GamePhase.PlayerPhase:
    //                 statusText.text = "玩家阶段";
    //                 break;
    //             case GameManager.GamePhase.MicroMovePhase:
    //                 statusText.text = "微调阶段";
    //                 break;
    //             case GameManager.GamePhase.EnemyPhase:
    //                 statusText.text = "敌人阶段";
    //                 break;
    //         }
    //     }
        
    //     // 更新微调阶段倒计时
    //     if (microMoveTimerText != null)
    //     {
    //         float timer = gameManager.GetMicroMoveTimer();
    //         if (timer > 0)
    //         {
    //             microMoveTimerText.text = $"微调倒计时: {timer:F1}秒";
    //         }
    //         else
    //         {
    //             microMoveTimerText.text = "";
    //         }
    //     }
        
    //     // 更新敌人阶段信息
    //     if (enemyPhaseText != null && gameManager.GetCurrentPhase() == GameManager.GamePhase.EnemyPhase)
    //     {
    //         if (enemyController != null)
    //         {
    //             string phaseName = GetEnemyPhaseName(enemyController.GetCurrentPhase());
    //             float timer = enemyController.GetPhaseTimer();
    //             enemyPhaseText.text = $"{phaseName}: {timer:F1}秒";
    //         }
    //         else
    //         {
    //             enemyPhaseText.text = "敌人阶段";
    //         }
    //     }
    //     else if (enemyPhaseText != null)
    //     {
    //         enemyPhaseText.text = "";
    //     }
        
    //     // 更新指令文本
    //     if (instructionsText != null)
    //     {
    //         switch (gameManager.GetCurrentPhase())
    //         {
    //             case GameManager.GamePhase.PlayerPhase:
    //                 instructionsText.text = "按住鼠标左键蓄力，松开发射";
    //                 break;
    //             case GameManager.GamePhase.MicroMovePhase:
    //                 instructionsText.text = "微调阶段：点击鼠标左键微调白球位置";
    //                 break;
    //             case GameManager.GamePhase.EnemyPhase:
    //                 instructionsText.text = "敌人阶段：等待敌人行动完成";
    //                 break;
    //         }
    //     }
        
    //     // 更新剩余敌人数显示
    //     UpdateRemainingEnemies();
        
    //     // 更新出杆数显示
    //     UpdateShotCount();
    // }
    
    // void OnPhaseChanged(GameManager.GamePhase newPhase)
    // {
    //     Debug.Log($"UI: 阶段切换到 {newPhase}");
    // }
    
    // void OnEnemyPhaseChanged(EnemyController.EnemyPhase newPhase)
    // {
    //     Debug.Log($"UI: 敌人阶段切换到 {newPhase}");
    // }
    
    // string GetEnemyPhaseName(EnemyController.EnemyPhase phase)
    // {
    //     switch (phase)
    //     {
    //         case EnemyController.EnemyPhase.AttackPhase:
    //             return "攻击阶段";
    //         case EnemyController.EnemyPhase.MovePhase:
    //             return "移动阶段";
    //         case EnemyController.EnemyPhase.TelegraphPhase:
    //             return "预备阶段";
    //         default:
    //             return "未知阶段";
    //     }
    // }
    
    // // 更新剩余敌人数显示
    // void UpdateRemainingEnemies()
    // {
    //     if (remainingEnemiesText != null)
    //     {
    //         // 显示敌人总数，与场上敌人数同步变化
    //         remainingEnemiesText.text = $"剩余敌人: {totalEnemies}";
    //     }
    // }
    
    
    
    // // 更新出杆数显示
    // void UpdateShotCount()
    // {
    //     if (shotCountText != null)
    //     {
    //         shotCountText.text = $"出杆数: {shotCount}";
    //     }
    // }
    
    
    // // 初始化UI
    // void InitializeUI()
    // {
    //     // 隐藏胜利和失败界面
    //     if (victoryPanel != null)
    //         victoryPanel.SetActive(false);
    //     if (gameOverPanel != null)
    //         gameOverPanel.SetActive(false);
    // }
    
    // // 计算敌人总数
    // void CalculateTotalEnemies()
    // {
    //     totalEnemies = 0;
        
    //     // 当前场上的敌人
    //     Enemy[] currentEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
    //     int currentEnemiesCount = currentEnemies.Length;
    //     totalEnemies += currentEnemiesCount;
        
    //     // 波次里的总敌人数
    //     int waveEnemiesCount = 0;
    //     if (enemySpawner != null)
    //     {
    //         for (int i = 0; i < enemySpawner.enemiesPerWave.Length; i++)
    //         {
    //             waveEnemiesCount += enemySpawner.enemiesPerWave[i];
    //         }
    //     }
    //     totalEnemies += waveEnemiesCount;
        
    //     // 初始化lastEnemyCount
    //     lastEnemyCount = currentEnemiesCount;
        
    //     Debug.Log($"敌人总数统计: 当前场上={currentEnemiesCount}, 波次总数={waveEnemiesCount}, 总计={totalEnemies}");
    // }
    
    // // 更新敌人计数，只在场上敌人数减少时同步变化
    // void UpdateEnemyCount()
    // {
    //     if (gameEnded) return;
        
    //     // 获取当前场上的敌人数量
    //     int currentEnemiesOnField = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        
    //     // 如果场上敌人数量比上次少，说明有敌人死亡
    //     if (currentEnemiesOnField < lastEnemyCount)
    //     {
    //         // 计算死了几个敌人
    //         int deadCount = lastEnemyCount - currentEnemiesOnField;
    //         totalEnemies -= deadCount;
    //         Debug.Log($"敌人死亡{deadCount}个，总数减少: {totalEnemies}");
            
    //         // 检查胜利条件
    //         CheckVictory();
    //     }
        
    //     // 更新记录
    //     lastEnemyCount = currentEnemiesOnField;
    // }
    
    // // 监听白球发射
    // System.Collections.IEnumerator MonitorWhiteBallShots()
    // {
    //     WhiteBall whiteBall = FindAnyObjectByType<WhiteBall>();
    //     if (whiteBall == null) yield break;
        
    //     bool wasMoving = false;
        
    //     while (whiteBall != null && !gameEnded)
    //     {
    //         bool isMoving = whiteBall.IsMoving();
            
    //         // 检测从静止到移动的转换（发射）
    //         // 只有在玩家阶段才算出杆
    //         if (!wasMoving && isMoving && gameManager != null && gameManager.GetCurrentPhase() == GameManager.GamePhase.PlayerPhase)
    //         {
    //             shotCount++;
    //             Debug.Log($"玩家出杆，当前出杆数: {shotCount}");
    //         }
            
    //         wasMoving = isMoving;
    //         yield return new WaitForSeconds(0.1f); // 每0.1秒检查一次
    //     }
    // }
    
    
    
    // // 检查胜利条件
    // void CheckVictory()
    // {
    //     // 检查敌人总数是否为0
    //     Debug.Log($"胜利检查: 敌人总数={totalEnemies}, 游戏已结束={gameEnded}");
        
    //     if (totalEnemies <= 0 && !gameEnded)
    //     {
    //         Debug.Log("触发胜利条件！");
    //         ShowVictoryScreen();
    //     }
    // }
    
    // // 显示胜利界面
    // void ShowVictoryScreen()
    // {
    //     if (victoryPanel != null)
    //     {
    //         gameEnded = true; // 标记游戏结束
    //         Time.timeScale = 0f; // 停止游戏时间
            
    //         victoryPanel.SetActive(true);
            
    //         if (victoryTitleText != null)
    //             victoryTitleText.text = "胜利！";
            
    //         if (victoryStatsText != null)
    //             victoryStatsText.text = $"出杆数: {shotCount}";
            
    //         Debug.Log("显示胜利界面，游戏时间已停止");
    //     }
    // }
    
    // // 显示失败界面
    // public void ShowGameOverScreen()
    // {
    //     if (gameOverPanel != null)
    //     {
    //         gameEnded = true; // 标记游戏结束
    //         Time.timeScale = 0f; // 停止游戏时间
            
    //         gameOverPanel.SetActive(true);
            
    //         if (gameOverTitleText != null)
    //             gameOverTitleText.text = "游戏失败！";
            
    //         if (gameOverStatsText != null)
    //             gameOverStatsText.text = $"出杆数: {shotCount}";
            
    //         Debug.Log("显示失败界面，游戏时间已停止");
    //     }
    // }
    
    // // 重新开始游戏
    // void RestartGame()
    // {
    //     Debug.Log("重新开始游戏 - 按钮被点击");
    //     Time.timeScale = 1f; // 恢复游戏时间
    //     Debug.Log($"当前场景名称: {SceneManager.GetActiveScene().name}");
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //     Debug.Log("场景重新加载命令已执行");
    // }
}
