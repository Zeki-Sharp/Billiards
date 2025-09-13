using UnityEngine;

/// <summary>
/// 瞄准线材质控制器 - 负责控制瞄准线的材质效果
/// 包括短划密度调整、流动效果、淡出效果等
/// </summary>
public class AimLineMaterialController : MonoBehaviour
{
    [Header("材质设置")]
    [SerializeField] private Material aimLineMaterial;  // 瞄准线材质
    [SerializeField] private float referenceLength = 1f;  // 参考长度（用于计算UScale）
    [SerializeField] private float baseUScale = 15f;  // 基础UScale值
    [SerializeField] private float minUScale = 1f;  // 最小UScale值
    [SerializeField] private float maxUScale = 100f;  // 最大UScale值
    
    [Header("流动效果设置")]
    [SerializeField] private float autoScrollSpeed = 1f;  // 自动滚动速度
    [SerializeField] private bool enableFlowEffect = true;  // 是否启用流动效果
    
    [Header("淡出效果设置")]
    [SerializeField] private float fadeLength = 0.5f;  // 淡出长度
    [SerializeField] private float fadeStrength = 1f;  // 淡出强度
    [SerializeField] private float fadeCurve = 1f;  // 淡出曲线
    [SerializeField] private bool enableFadeEffect = true;  // 是否启用淡出效果
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = false;  // 是否启用调试日志
    
    // 材质属性名称常量
    private const string USCALE_PROPERTY = "_UScale";
    private const string UVOFFSET_PROPERTY = "_UVOffset";
    private const string AUTOSCROLL_SPEED_PROPERTY = "_AutoScrollSpeed";
    private const string FADE_LEN_PROPERTY = "_FadeLen";
    private const string FADE_STRENGTH_PROPERTY = "_FadeStrength";
    private const string FADE_CURVE_PROPERTY = "_FadeCurve";
    
    // 时间累积，用于流动效果
    private float timeAccumulator = 0f;
    
    void Start()
    {
        InitializeMaterial();
    }
    
    void Update()
    {
        if (enableFlowEffect)
        {
            UpdateFlowEffect();
        }
    }
    
    /// <summary>
    /// 初始化材质设置
    /// </summary>
    void InitializeMaterial()
    {
        if (aimLineMaterial == null)
        {
            Debug.LogWarning("AimLineMaterialController: 瞄准线材质未设置");
            return;
        }
        
        // 设置基础材质参数
        aimLineMaterial.SetFloat(AUTOSCROLL_SPEED_PROPERTY, autoScrollSpeed);
        aimLineMaterial.SetFloat(FADE_LEN_PROPERTY, fadeLength);
        aimLineMaterial.SetFloat(FADE_STRENGTH_PROPERTY, fadeStrength);
        aimLineMaterial.SetFloat(FADE_CURVE_PROPERTY, fadeCurve);
        
        if (enableDebugLog)
        {
            Debug.Log($"AimLineMaterialController: 材质初始化完成 - 基础UScale: {baseUScale}, 参考长度: {referenceLength}");
        }
    }
    
    /// <summary>
    /// 更新流动效果
    /// </summary>
    void UpdateFlowEffect()
    {
        if (aimLineMaterial == null) return;
        
        timeAccumulator += Time.deltaTime * autoScrollSpeed;
        aimLineMaterial.SetFloat(UVOFFSET_PROPERTY, timeAccumulator);
    }
    
    /// <summary>
    /// 为指定LineRenderer设置材质参数
    /// </summary>
    /// <param name="lineRenderer">目标LineRenderer</param>
    /// <param name="segmentLength">线段长度</param>
    public void UpdateSegmentMaterial(LineRenderer lineRenderer, float segmentLength)
    {
        if (lineRenderer == null || aimLineMaterial == null)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning("AimLineMaterialController: LineRenderer或材质为空");
            }
            return;
        }
        
        // 计算UScale
        float calculatedUScale = CalculateUScale(segmentLength);
        
        // 创建MaterialPropertyBlock
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        propBlock.SetFloat(USCALE_PROPERTY, calculatedUScale);
        
        // 应用材质属性
        lineRenderer.SetPropertyBlock(propBlock);
        
        if (enableDebugLog)
        {
            Debug.Log($"AimLineMaterialController: 更新分段材质 - 长度: {segmentLength:F2}, UScale: {calculatedUScale:F2}");
        }
    }
    
    /// <summary>
    /// 为指定LineRenderer设置材质参数（带淡出效果）
    /// </summary>
    /// <param name="lineRenderer">目标LineRenderer</param>
    /// <param name="segmentLength">线段长度</param>
    /// <param name="enableFade">是否启用淡出效果</param>
    public void UpdateSegmentMaterial(LineRenderer lineRenderer, float segmentLength, bool enableFade)
    {
        if (lineRenderer == null || aimLineMaterial == null)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning("AimLineMaterialController: LineRenderer或材质为空");
            }
            return;
        }
        
        // 计算UScale
        float calculatedUScale = CalculateUScale(segmentLength);
        
        // 创建MaterialPropertyBlock
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        propBlock.SetFloat(USCALE_PROPERTY, calculatedUScale);
        
        // 设置淡出效果
        if (enableFade && enableFadeEffect)
        {
            propBlock.SetFloat(FADE_LEN_PROPERTY, fadeLength);
            propBlock.SetFloat(FADE_STRENGTH_PROPERTY, fadeStrength);
            propBlock.SetFloat(FADE_CURVE_PROPERTY, fadeCurve);
            
            if (enableDebugLog)
            {
                Debug.Log($"AimLineMaterialController: 应用淡出效果 - 长度: {fadeLength}, 强度: {fadeStrength}, 曲线: {fadeCurve}");
            }
        }
        else
        {
            // 禁用淡出效果
            propBlock.SetFloat(FADE_LEN_PROPERTY, 0f);
            propBlock.SetFloat(FADE_STRENGTH_PROPERTY, 0f);
            propBlock.SetFloat(FADE_CURVE_PROPERTY, 1f);
            
            if (enableDebugLog)
            {
                Debug.Log($"AimLineMaterialController: 禁用淡出效果 - enableFade: {enableFade}, enableFadeEffect: {enableFadeEffect}");
            }
        }
        
        // 应用材质属性
        lineRenderer.SetPropertyBlock(propBlock);
        
        if (enableDebugLog)
        {
            Debug.Log($"AimLineMaterialController: 更新分段材质 - 长度: {segmentLength:F2}, UScale: {calculatedUScale:F2}, 淡出: {enableFade}");
        }
    }
    
    /// <summary>
    /// 计算UScale值
    /// </summary>
    /// <param name="segmentLength">线段长度</param>
    /// <returns>计算后的UScale值</returns>
    float CalculateUScale(float segmentLength)
    {
        if (segmentLength <= 0f)
        {
            return baseUScale;
        }
        
        // 根据线段长度计算UScale
        float calculatedUScale = baseUScale * (segmentLength / referenceLength);
        
        // 限制在最小值和最大值之间
        calculatedUScale = Mathf.Clamp(calculatedUScale, minUScale, maxUScale);
        
        return calculatedUScale;
    }
    
    /// <summary>
    /// 设置瞄准线材质
    /// </summary>
    /// <param name="material">新的材质</param>
    public void SetAimLineMaterial(Material material)
    {
        aimLineMaterial = material;
        InitializeMaterial();
    }
    
    /// <summary>
    /// 设置参考长度
    /// </summary>
    /// <param name="length">参考长度</param>
    public void SetReferenceLength(float length)
    {
        referenceLength = Mathf.Max(0.1f, length);
        
        if (enableDebugLog)
        {
            Debug.Log($"AimLineMaterialController: 参考长度设置为 {referenceLength}");
        }
    }
    
    /// <summary>
    /// 设置基础UScale
    /// </summary>
    /// <param name="uScale">基础UScale值</param>
    public void SetBaseUScale(float uScale)
    {
        baseUScale = Mathf.Max(0.1f, uScale);
        
        if (enableDebugLog)
        {
            Debug.Log($"AimLineMaterialController: 基础UScale设置为 {baseUScale}");
        }
    }
    
    /// <summary>
    /// 设置流动效果
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <param name="speed">滚动速度</param>
    public void SetFlowEffect(bool enabled, float speed = 1f)
    {
        enableFlowEffect = enabled;
        autoScrollSpeed = Mathf.Max(0f, speed);
        
        if (aimLineMaterial != null)
        {
            aimLineMaterial.SetFloat(AUTOSCROLL_SPEED_PROPERTY, autoScrollSpeed);
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"AimLineMaterialController: 流动效果 {(enabled ? "启用" : "禁用")}, 速度: {autoScrollSpeed}");
        }
    }
    
    /// <summary>
    /// 设置淡出效果
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <param name="length">淡出长度</param>
    /// <param name="strength">淡出强度</param>
    /// <param name="curve">淡出曲线</param>
    public void SetFadeEffect(bool enabled, float length = 0.5f, float strength = 1f, float curve = 1f)
    {
        enableFadeEffect = enabled;
        fadeLength = Mathf.Max(0f, length);
        fadeStrength = Mathf.Clamp01(strength);
        fadeCurve = Mathf.Max(0.1f, curve);
        
        if (aimLineMaterial != null)
        {
            aimLineMaterial.SetFloat(FADE_LEN_PROPERTY, fadeLength);
            aimLineMaterial.SetFloat(FADE_STRENGTH_PROPERTY, fadeStrength);
            aimLineMaterial.SetFloat(FADE_CURVE_PROPERTY, fadeCurve);
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"AimLineMaterialController: 淡出效果 {(enabled ? "启用" : "禁用")}, 长度: {fadeLength}, 强度: {fadeStrength}");
        }
    }
    
    /// <summary>
    /// 重置时间累积器（用于重新开始流动效果）
    /// </summary>
    public void ResetFlowTime()
    {
        timeAccumulator = 0f;
        
        if (enableDebugLog)
        {
            Debug.Log("AimLineMaterialController: 流动时间已重置");
        }
    }
    
    /// <summary>
    /// 获取当前材质
    /// </summary>
    /// <returns>当前使用的材质</returns>
    public Material GetAimLineMaterial()
    {
        return aimLineMaterial;
    }
    
    /// <summary>
    /// 获取材质统计信息
    /// </summary>
    /// <returns>材质统计信息字符串</returns>
    public string GetMaterialStats()
    {
        return $"材质控制器状态:\n" +
               $"- 材质: {(aimLineMaterial != null ? aimLineMaterial.name : "未设置")}\n" +
               $"- 参考长度: {referenceLength}\n" +
               $"- 基础UScale: {baseUScale}\n" +
               $"- UScale范围: {minUScale} - {maxUScale}\n" +
               $"- 流动效果: {(enableFlowEffect ? "启用" : "禁用")}\n" +
               $"- 滚动速度: {autoScrollSpeed}\n" +
               $"- 淡出效果: {(enableFadeEffect ? "启用" : "禁用")}\n" +
               $"- 淡出长度: {fadeLength}\n" +
               $"- 淡出强度: {fadeStrength}\n" +
               $"- 淡出曲线: {fadeCurve}";
    }
}