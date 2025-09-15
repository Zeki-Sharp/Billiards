using UnityEngine;
using UnityEngine.UI;

public class GameUIPanel : MonoBehaviour
{
    [Header("UI元素")]
    public Image energyBar;
    public Image playerHealthBar;
    
    private EnergySystem energySystem;
    private PlayerCore playerCore;
    
    void Start()
    {
        // 查找能量系统
        energySystem = FindFirstObjectByType<EnergySystem>();
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged += UpdateEnergyBar;
        }
        
        // 查找玩家核心组件
        playerCore = FindFirstObjectByType<PlayerCore>();
        if (playerCore != null)
        {
            playerCore.OnHealthChanged += UpdateHealthBar;
        }
        
        // 初始化UI
        UpdateEnergyBar(energySystem != null ? energySystem.GetCurrentEnergy() : 100f);
        UpdateHealthBar(playerCore != null ? playerCore.GetHealthPercentage() : 1f);
    }
    
    void UpdateEnergyBar(float currentEnergy)
    {
        if (energyBar != null && energySystem != null)
        {
            energyBar.fillAmount = energySystem.GetEnergyPercentage();
        }
    }
    
    void UpdateHealthBar(float healthPercentage)
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.fillAmount = healthPercentage;
        }
    }
    
    void OnDestroy()
    {
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged -= UpdateEnergyBar;
        }
        
        if (playerCore != null)
        {
            playerCore.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
