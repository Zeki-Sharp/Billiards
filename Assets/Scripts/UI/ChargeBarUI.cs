using UnityEngine;
using UnityEngine.UI;

public class ChargeBarUI : MonoBehaviour
{
    [Header("UI组件")]
    public Image fillImage;
    
    private float maxWidth;
    private float lastUpdateTime;
    private float updateInterval = 0.016f; // 约60FPS更新频率
    
    void Start()
    {
        // 如果没有指定Fill Image，尝试自动查找
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
        }
        
        // 记录最大宽度
        if (fillImage != null)
        {
            maxWidth = fillImage.rectTransform.sizeDelta.x;
        }
        
        // 初始化UI状态
        SetVisible(false);
    }
    
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    public void UpdateCharge(float normalizedValue)
    {
        // 限制更新频率，避免卡顿
        if (Time.time - lastUpdateTime < updateInterval)
            return;
            
        lastUpdateTime = Time.time;
        
        if (fillImage != null)
        {
            // 通过修改Fill Image的宽度来显示蓄力值
            float currentWidth = maxWidth * Mathf.Clamp01(normalizedValue);
            fillImage.rectTransform.sizeDelta = new Vector2(currentWidth, fillImage.rectTransform.sizeDelta.y);
        }
    }
}
