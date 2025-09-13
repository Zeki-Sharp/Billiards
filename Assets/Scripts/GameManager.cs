using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GamePhase
    {
        PlayerPhase,    // 玩家阶段
        MicroMovePhase, // 微调阶段
        EnemyPhase      // 敌人阶段
    }
    
    [Header("阶段设置")]
    public float microMoveDuration = 3f; // 微调阶段持续时间
    public float microMoveDistance = 1f; // 微调移动距离
    
    [Header("白球重生设置")]
    public float whiteBallRespawnDamage = 20f; // 白球进洞扣血量
    public bool waitingForRespawn = false; // 是否等待重生
    public bool ballJustRespawned = false; // 白球刚重生，需要等待确认
    
    private GamePhase currentPhase = GamePhase.PlayerPhase;
    private float microMoveTimer = 0f;
    private bool hasPlayerLaunched = false; // 玩家是否已发射
    private bool isFirstRound = true; // 是否为第一回合
    
    // 游戏对象引用
    private WhiteBall whiteBall;
    private Enemy[] enemies;
    private EnemyController enemyController;
    private HoleManager holeManager;
    private EnemySpawner enemySpawner;
    
    // 事件
    public System.Action<GamePhase> OnPhaseChanged;
    
    void Awake()
    {
        // 单例模式：确保只有一个GameManager实例
        if (Instance == null)
        {
            Instance = this;
            // 移除DontDestroyOnLoad，让GameManager在场景重新加载时被销毁
            
            // 设置全局物理参数
            Physics2D.gravity = Vector2.zero; // 禁用重力，台球不受重力影响
            Debug.Log("GameManager: 已禁用全局重力");
        }
        else
        {
            Debug.LogWarning("发现多个GameManager实例，销毁重复的实例");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        HandleCurrentPhase();
    }
    
    void InitializeGame()
    {
        // 查找白球
        whiteBall = FindAnyObjectByType<WhiteBall>();
        if (whiteBall != null)
        {
            whiteBall.OnBallStopped += OnWhiteBallStopped;
        }
        
        // 查找所有敌人
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        // 查找敌人控制器
        enemyController = FindAnyObjectByType<EnemyController>();
        if (enemyController != null)
        {
            enemyController.OnEnemyPhaseComplete += OnEnemyPhaseComplete;
        }
        
        // 查找HoleManager并订阅事件
        holeManager = FindAnyObjectByType<HoleManager>();
        if (holeManager != null)
        {
            holeManager.OnWhiteBallInHole += HandleWhiteBallInHole;
            Debug.Log("找到HoleManager并订阅事件");
        }
        else
        {
            Debug.LogError("未找到HoleManager！请确保HoleManager在场景中");
        }
        
        // 查找EnemySpawner
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (enemySpawner != null)
        {
            Debug.Log("找到EnemySpawner");
        }
        else
        {
            Debug.LogWarning("未找到EnemySpawner！将使用静态敌人");
        }
        
        // 第一回合：生成第一波生成点，然后显示敌人预览
        if (enemySpawner != null)
        {
            enemySpawner.CreateWaveSpawnPoints(); // 生成第一波生成点
            Debug.Log("游戏开始：生成第一波生成点");
        }
        
        if (enemyController != null)
        {
            Debug.Log("调用ShowEnemyPreview()显示敌人攻击范围预览");
            enemyController.ShowEnemyPreview(); // 只显示预览，不开始敌人阶段
        }
        else
        {
            Debug.LogWarning("enemyController为null，无法显示敌人预览");
        }
        SetPhase(GamePhase.PlayerPhase);
    }
    
    void HandleCurrentPhase()
    {
        switch (currentPhase)
        {
            case GamePhase.PlayerPhase:
                HandlePlayerPhase();
                break;
            case GamePhase.MicroMovePhase:
                HandleMicroMovePhase();
                break;
            case GamePhase.EnemyPhase:
                HandleEnemyPhase();
                break;
        }
    }
    
    void HandlePlayerPhase()
    {
        // 如果等待重生，检测鼠标点击
        if (waitingForRespawn)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("GameManager: 摆球状态点击，重生白球");
                RespawnWhiteBall();
            }
            return;
        }
        
        // 玩家阶段：只有发射后才检查是否所有球都停止了
        if (hasPlayerLaunched && AllBallsStopped())
        {
            Debug.Log("玩家发射后，所有球都停止了，进入微调阶段");
            SetPhase(GamePhase.MicroMovePhase);
        }
    }
    
    void HandleMicroMovePhase()
    {
        // 微调阶段：倒计时
        microMoveTimer -= Time.deltaTime;
        
        // 每0.5秒输出一次倒计时信息
        if (Mathf.FloorToInt(microMoveTimer * 2) != Mathf.FloorToInt((microMoveTimer + Time.deltaTime) * 2))
        {
            Debug.Log($"微调阶段倒计时: {microMoveTimer:F1}秒");
        }
        
        // 处理微调阶段的鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            HandleMicroMoveClick();
        }
        
        if (microMoveTimer <= 0)
        {
            Debug.Log("微调阶段结束，进入敌人阶段");
            SetPhase(GamePhase.EnemyPhase);
        }
    }
    
    void HandleMicroMoveClick()
    {
        if (whiteBall == null) return;
        
        // 获取鼠标屏幕坐标
        Vector3 mouseScreenPos = Input.mousePosition;
        
        // 修复相机坐标转换问题
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("找不到主相机！");
            return;
        }
        
        // 获取白球当前位置（确保获取最新位置）
        Vector3 whiteBallPos = whiteBall.transform.position;
        
        // 使用相机的深度来正确转换坐标
        // 对于2D游戏，我们需要使用相机到白球的距离
        float distanceFromCamera = Mathf.Abs(mainCamera.transform.position.z - whiteBallPos.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, distanceFromCamera));
        mouseWorldPos.z = 0; // 确保z坐标为0
        
        // 计算从白球到鼠标点击位置的向量（未标准化）
        Vector3 rawDirection = mouseWorldPos - whiteBallPos;
        Vector2 direction = rawDirection.normalized;
        
        // 调试信息
        Debug.Log($"微调点击: 方向={direction}, 距离={rawDirection.magnitude:F2}");
        
        // 检查方向是否有效
        if (rawDirection.magnitude < 0.1f)
        {
            Debug.LogWarning("点击位置太接近白球，无法计算方向");
            return;
        }
        
        // 调用白球的微调移动方法
        whiteBall.MicroMove(direction, microMoveDistance);
    }
    
    void HandleEnemyPhase()
    {
        // 敌人阶段：由EnemyController管理
        // 可以添加一些UI更新或其他逻辑
    }
    
    void OnEnemyPhaseComplete()
    {
        Debug.Log("敌人阶段完成，回到玩家阶段");
        // 重置发射标志，准备下一轮
        hasPlayerLaunched = false;
        
        // 如果白球消失了，等待重生
        if (whiteBall != null && !whiteBall.gameObject.activeInHierarchy)
        {
            waitingForRespawn = true;
            Debug.Log("白球消失，等待玩家点击重生位置");
        }
        
        // 处理波次生成：只在非第一回合时处理
        if (!isFirstRound && enemySpawner != null)
        {
            // 将当前生成点变为敌人
            enemySpawner.SpawnEnemiesFromPoints();
            Debug.Log("将生成点变为敌人");
            
            // 生成下一波生成点
            enemySpawner.CreateWaveSpawnPoints();
            Debug.Log("生成下一波生成点");
        }
        
        // 第一回合完成后，标记为非第一回合
        if (isFirstRound)
        {
            isFirstRound = false;
            Debug.Log("第一回合完成，后续回合将处理波次生成");
        }
        
        // 为新生成的敌人显示攻击范围预览
        if (enemyController != null)
        {
            enemyController.ShowEnemyPreview();
        }
        
        SetPhase(GamePhase.PlayerPhase);
    }
    
    void HandleWhiteBallInHole(WhiteBall whiteBall)
    {
        Debug.Log("HandleWhiteBallInHole被调用！");
        
        // 白球消失
        whiteBall.gameObject.SetActive(false);
        Debug.Log("白球已设置为不可见");
        
        // 白球扣血
        whiteBall.TakeDamage(whiteBallRespawnDamage);
        Debug.Log($"白球扣血 {whiteBallRespawnDamage}");
        
        // 设置等待重生状态
        waitingForRespawn = true;
        Debug.Log("设置等待重生状态");
        
        // 切换到敌人阶段
        Debug.Log("准备切换到敌人阶段");
        SetPhase(GamePhase.EnemyPhase);
        
        Debug.Log("白球进洞，切换到敌人阶段");
    }
    
    public void RespawnWhiteBall()
    {
        if (whiteBall == null) return;
        
        // 使用射线检测获取鼠标点击位置
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        
        // 在2D游戏中，射线与Z=0平面相交
        float distance = -ray.origin.z / ray.direction.z;
        Vector3 worldPos = ray.origin + ray.direction * distance;
        
        // 重生白球
        whiteBall.gameObject.SetActive(true);
        whiteBall.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
        
        // 重置白球状态
        whiteBall.ResetForNewTurn();
        
        // 重置状态
        waitingForRespawn = false;
        ballJustRespawned = true; // 标记白球刚重生
        hasPlayerLaunched = false;
        
        Debug.Log($"白球重生完成: 位置={whiteBall.transform.position}, waitingForRespawn={waitingForRespawn}, ballJustRespawned={ballJustRespawned}");
        
        Debug.Log($"鼠标位置: {mousePos}, 射线起点: {ray.origin}, 射线方向: {ray.direction}, 世界坐标: {worldPos}, 白球重生位置: {whiteBall.transform.position}");
    }
    
    void OnWhiteBallStopped()
    {
        Debug.Log("白球停止事件触发");
        // 白球停止后，检查是否所有球都停止了（只有在玩家发射后才切换）
        if (currentPhase == GamePhase.PlayerPhase && hasPlayerLaunched && AllBallsStopped())
        {
            Debug.Log("玩家发射后，所有球都停止了，进入微调阶段");
            SetPhase(GamePhase.MicroMovePhase);
        }
    }
    
    bool AllBallsStopped()
    {
        // 检查白球是否停止
        if (whiteBall != null && whiteBall.IsMoving())
        {
            return false;
        }
        
        // 检查所有敌人是否停止
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.IsMoving())
            {
                return false;
            }
        }
        
        return true;
    }
    
    void SetPhase(GamePhase newPhase)
    {
        if (currentPhase != newPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log($"阶段切换到: {currentPhase}");
            
            // 阶段特定的初始化
            switch (currentPhase)
            {
                case GamePhase.MicroMovePhase:
                    microMoveTimer = microMoveDuration;
                    Debug.Log($"开始微调阶段，倒计时{microMoveDuration}秒");
                    break;
                case GamePhase.EnemyPhase:
                    Debug.Log("开始敌人阶段");
                    if (enemyController != null)
                    {
                        enemyController.StartEnemyPhase();
                    }
                    break;
                case GamePhase.PlayerPhase:
                    Debug.Log("开始玩家阶段");
                    break;
            }
        }
    }
    
    // 公共方法
    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    public bool CanPlayerLaunch()
    {
        return currentPhase == GamePhase.PlayerPhase && !hasPlayerLaunched && !ballJustRespawned;
    }
    
    public bool IsWaitingForRespawn()
    {
        return waitingForRespawn;
    }
    
    public void ConfirmBallPlacement()
    {
        if (ballJustRespawned)
        {
            ballJustRespawned = false;
            Debug.Log("白球位置确认，可以发射");
        }
    }
    
    public void OnPlayerLaunch()
    {
        hasPlayerLaunched = true;
    }
    
    public float GetMicroMoveTimer()
    {
        return currentPhase == GamePhase.MicroMovePhase ? microMoveTimer : 0f;
    }
}
