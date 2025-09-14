using UnityEngine;

/// <summary>
/// 伤害数字配置数据类
/// 包含伤害数字系统的各种配置参数
/// </summary>
[CreateAssetMenu(fileName = "DamageTextConfig", menuName = "DamageText/DamageText Config")]
public class DamageTextConfig : ScriptableObject
{
    [Header("颜色设置")]
    [Tooltip("伤害数字颜色")]
    public Color damageColor = Color.white;
    
    [Header("Anchor 偏移设置")]
    [Tooltip("相对于目标物体的偏移位置")]
    public Vector3 anchorOffset = new Vector3(0, 1f, 0);
    [Tooltip("是否使用目标物体的包围盒高度")]
    public bool useTargetBounds = true;
    [Tooltip("包围盒高度倍数")]
    public float boundsHeightMultiplier = 1.2f;
    
    [Header("随机位移设置")]
    [Tooltip("随机位移范围")]
    public float randomOffsetRange = 1f;
    [Tooltip("最小随机偏移")]
    public float minRandomOffset = 0.5f;
    [Tooltip("最大随机偏移")]
    public float maxRandomOffset = 1.5f;
    
    [Header("边缘检测设置")]
    [Tooltip("屏幕边距")]
    public float screenMargin = 50f;
    [Tooltip("是否启用边缘检测")]
    public bool enableEdgeDetection = true;
    
    [Header("对象池设置")]
    [Tooltip("对象池大小")]
    public int poolSize = 30;
    [Tooltip("是否自动扩展对象池")]
    public bool autoExpandPool = true;
    [Tooltip("最大对象池大小")]
    public int maxPoolSize = 100;
    
    [Header("字体设置")]
    [Tooltip("伤害数字字体大小")]
    public float fontSize = 24f;
    [Tooltip("是否启用字体描边")]
    public bool enableOutline = true;
    [Tooltip("字体描边颜色")]
    public Color outlineColor = Color.black;
    [Tooltip("字体描边宽度")]
    public float outlineWidth = 2f;

}