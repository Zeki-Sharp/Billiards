using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

/// <summary>
/// 单个伤害数字脚本
/// 只负责设置参数，动画完全由 MMF 控制
/// </summary>
public class DamageText : MonoBehaviour
{
    [Header("组件引用")]
    private TextMeshProUGUI textComponent;
    private RectTransform rectTransform;
    
    [Header("伤害数字设置")]
    private float damageValue;
    private Vector3 targetPosition;
    private DamageTextConfig config;
    
    void Awake()
    {
        // 获取组件引用
        textComponent = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }
    
    /// <summary>
    /// 初始化伤害数字
    /// </summary>
    /// <param name="damage">伤害数值</param>
    /// <param name="position">显示位置</param>
    /// <param name="config">配置数据</param>
    public void Initialize(float damage, Vector3 position, DamageTextConfig config)
    {
        this.damageValue = damage;
        this.targetPosition = position;
        this.config = config;
        
        // 设置文本内容
        SetTextContent();
        
        // 设置位置
        SetPosition(position);
        
        // 设置样式
        SetStyle();
        
        // 启动自动回收协程
        StartCoroutine(AutoReturnToPool());
    }
    
    /// <summary>
    /// 自动回收协程
    /// </summary>
    private System.Collections.IEnumerator AutoReturnToPool()
    {
        // 获取 MMF Player 的总时长
        float animationDuration = GetAnimationDuration();
        
        // 等待动画播放完成
        yield return new WaitForSeconds(animationDuration);
        
        // 回收对象
        ReturnToPool();
    }
    
    /// <summary>
    /// 获取 MMF 动画总时长
    /// </summary>
    /// <returns>动画总时长（秒）</returns>
    private float GetAnimationDuration()
    {
        // 获取 MMF Player 组件
        var mmfPlayer = GetComponent<MMF_Player>();
        if (mmfPlayer != null)
        {
            // 返回 MMF Player 的总时长
            return mmfPlayer.TotalDuration;
        }
        
        // 如果没有 MMF Player，返回默认时长
        return 1.2f;
    }
    
    /// <summary>
    /// 设置文本内容
    /// </summary>
    private void SetTextContent()
    {
        if (textComponent == null) return;
        
        textComponent.text = damageValue.ToString("F0");
    }
    
    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="screenPosition">屏幕坐标位置</param>
    private void SetPosition(Vector3 screenPosition)
    {
        if (rectTransform == null) return;
        
        // 直接设置屏幕坐标位置
        rectTransform.position = screenPosition;
    }
    
    /// <summary>
    /// 设置样式
    /// </summary>
    private void SetStyle()
    {
        if (textComponent == null || config == null) return;
        
        // 设置颜色
        textComponent.color = config.damageColor;
        
        // 设置字体大小
        textComponent.fontSize = config.fontSize;
        
        // 设置描边
        if (config.enableOutline)
        {
            textComponent.outlineColor = config.outlineColor;
            textComponent.outlineWidth = config.outlineWidth;
        }
    }
    
    /// <summary>
    /// 回收到对象池
    /// 由 MMF 动画完成时调用
    /// </summary>
    public void ReturnToPool()
    {
        // 重置状态
        rectTransform.localScale = Vector3.one;
        
        // 通知管理器回收
        DamageTextManager.Instance?.ReturnDamageText(this);
    }
    
    /// <summary>
    /// 强制回收到对象池
    /// </summary>
    public void ForceReturnToPool()
    {
        ReturnToPool();
    }
}