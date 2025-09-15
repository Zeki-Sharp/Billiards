using UnityEngine;
using UnityEngine.UI;

public class GameUIPanel : MonoBehaviour
{
    [Header("UI元素")]
    public Image energyBar;
    public Image playerHealthBar;
    
    private EnergySystem energySystem;
    private PlayerStateMachine playerStateMachine;
    
    void Start()
    {
        // 查找能量系统
        energySystem = FindFirstObjectByType<EnergySystem>();
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged += UpdateEnergyBar;
        }
        
        // 查找玩家状态机（假设有生命值相关逻辑）
        playerStateMachine = FindFirstObjectByType<PlayerStateMachine>();
        
        // 初始化UI
        UpdateEnergyBar(energySystem != null ? energySystem.GetCurrentEnergy() : 100f);
        UpdateHealthBar(100f); // 假设满血
    }
    
    void UpdateEnergyBar(float currentEnergy)
    {
        if (energyBar != null && energySystem != null)
        {
            energyBar.fillAmount = energySystem.GetEnergyPercentage();
        }
    }
    
    void UpdateHealthBar(float currentHealth)
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.fillAmount = currentHealth / 100f; // 假设最大生命值为100
        }
    }
    
    void OnDestroy()
    {
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged -= UpdateEnergyBar;
        }
    }
}
