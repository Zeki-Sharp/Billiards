using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 能量系统 - 管理玩家能量恢复和消耗
/// </summary>
public class EnergySystem : MonoBehaviour
{
    [Header("能量设置")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float energyRegenRate = 10f; // 每秒恢复能量
    [SerializeField] private float energyConsumeRate = 20f; // 每秒消耗能量（蓄力时）
    [SerializeField] private float energyThreshold = 80f; // 能量阈值（保留用于其他功能）
    
    [Header("状态控制")]
    [SerializeField] private bool recoveryEnabled = true; // 是否允许恢复能量
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 事件（使用MM架构）
    public System.Action<float> OnEnergyChanged; // 能量变化
    public System.Action OnEnergyReady; // 能量就绪
    public System.Action OnEnergyDepleted; // 能量耗尽
    
    void Update()
    {
        // 根据状态决定是否恢复能量
        if (recoveryEnabled && currentEnergy < maxEnergy)
        {
            RecoverEnergy();
        }
    }
    
    /// <summary>
    /// 恢复能量
    /// </summary>
    void RecoverEnergy()
    {
        currentEnergy += energyRegenRate * Time.deltaTime;
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy);
        OnEnergyChanged?.Invoke(currentEnergy);
        
        // 触发MM事件
        GameStateEvent.Trigger("EnergyChanged", 0, currentEnergy, $"{currentEnergy:F1}/{maxEnergy:F1}");
        
        // 检查是否达到阈值
        if (currentEnergy >= energyThreshold)
        {
            OnEnergyReady?.Invoke();
            GameStateEvent.Trigger("EnergyReady", 0, currentEnergy, $"{currentEnergy:F1}/{maxEnergy:F1}");
        }
    }
    
    public bool CanUseEnergy()
    {
        return currentEnergy >= energyThreshold;
    }
    
    /// <summary>
    /// 消耗能量（单次）
    /// </summary>
    public void ConsumeEnergy()
    {
        if (currentEnergy >= energyThreshold)
        {
            currentEnergy -= energyConsumeRate;
            currentEnergy = Mathf.Max(0, currentEnergy);
            OnEnergyChanged?.Invoke(currentEnergy);
            
            // 触发MM事件
            GameStateEvent.Trigger("EnergyChanged", 0, currentEnergy, $"{currentEnergy:F1}/{maxEnergy:F1}");
            
            if (currentEnergy < energyThreshold)
            {
                OnEnergyDepleted?.Invoke();
                GameStateEvent.Trigger("EnergyDepleted", 0, currentEnergy, $"{currentEnergy:F1}/{maxEnergy:F1}");
            }
        }
    }
    
    /// <summary>
    /// 持续消耗能量（蓄力时使用）
    /// </summary>
    public bool ConsumeEnergyOverTime(float rate)
    {
        currentEnergy -= rate * Time.deltaTime;
        currentEnergy = Mathf.Max(0, currentEnergy);
        OnEnergyChanged?.Invoke(currentEnergy);
        
        // 触发MM事件
        GameStateEvent.Trigger("EnergyChanged", 0, currentEnergy, $"{currentEnergy:F1}/{maxEnergy:F1}");
        
        if (currentEnergy <= 0)
        {
            OnEnergyDepleted?.Invoke();
            GameStateEvent.Trigger("EnergyDepleted", 0, currentEnergy, $"{currentEnergy:F1}/{maxEnergy:F1}");
            return false; // 能量耗尽
        }
        return true; // 还有能量
    }
    
    public float GetEnergyPercentage()
    {
        return currentEnergy / maxEnergy;
    }
    
    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }
    
    /// <summary>
    /// 设置能量恢复状态
    /// </summary>
    public void SetRecoveryEnabled(bool enabled)
    {
        recoveryEnabled = enabled;
        
        if (showDebugInfo)
        {
            Debug.Log($"EnergySystem: 能量恢复状态设置为 {enabled}");
        }
    }
    
    /// <summary>
    /// 获取能量恢复状态
    /// </summary>
    public bool IsRecoveryEnabled()
    {
        return recoveryEnabled;
    }
    
    /// <summary>
    /// 重置能量到满值
    /// </summary>
    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy);
        
        if (showDebugInfo)
        {
            Debug.Log("EnergySystem: 能量已重置");
        }
    }
}
