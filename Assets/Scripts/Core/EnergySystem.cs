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
    [SerializeField] private float energyConsumeRate = 50f; // 每次消耗能量
    [SerializeField] private float energyThreshold = 80f; // 能量阈值
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 事件（使用MM架构）
    public System.Action<float> OnEnergyChanged; // 能量变化
    public System.Action OnEnergyReady; // 能量就绪
    public System.Action OnEnergyDepleted; // 能量耗尽
    
    void Update()
    {
        // 自动恢复能量
        if (currentEnergy < maxEnergy)
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
    }
    
    public bool CanUseEnergy()
    {
        return currentEnergy >= energyThreshold;
    }
    
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
    
    public float GetEnergyPercentage()
    {
        return currentEnergy / maxEnergy;
    }
    
    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }
}
