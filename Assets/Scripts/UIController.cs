using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("UI面板")]
    public GameUIPanel gameUIPanel;
    
    // [Header("UI元素")]
    // public TextMeshProUGUI statusText;
    // public TextMeshProUGUI microMoveTimerText;
    // public TextMeshProUGUI instructionsText;
    // public TextMeshProUGUI enemyPhaseText;
    // public TextMeshProUGUI remainingEnemiesText; // 剩余敌人数显示
    // public TextMeshProUGUI shotCountText; // 出杆数显示
    
    // [Header("胜利界面")]
    // public GameObject victoryPanel;
    // public TextMeshProUGUI victoryTitleText;
    // public TextMeshProUGUI victoryStatsText;
    // public Button victoryRestartButton;
    
    // [Header("失败界面")]
    // public GameObject gameOverPanel;
    // public TextMeshProUGUI gameOverTitleText;
    // public TextMeshProUGUI gameOverStatsText;
    // public Button gameOverRestartButton;
    
    // private GameManager gameManager;
    // private EnemyController enemyController;
    // private EnemySpawner enemySpawner;
    // private int shotCount = 0; // 出杆数
    // private bool gameEnded = false; // 游戏是否已结束
    // private int totalEnemies = 0; // 敌人总数
    // private int lastEnemyCount = 0; // 上一次的场上敌人数量
    
    void Start()
    {
        // 查找GameUIPanel
        if (gameUIPanel == null)
        {
            gameUIPanel = FindFirstObjectByType<GameUIPanel>();
        }
        
        Debug.Log("UIController: 初始化完成");
    }
}
