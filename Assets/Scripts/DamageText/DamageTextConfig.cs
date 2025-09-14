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
    
    [Header("文本设置")]
    [Tooltip("伤害数字前缀（如：-、+、暴击等）")]
    public string damagePrefix = "-";
    [Tooltip("伤害数字后缀（如：伤害、治疗等）")]
    public string damageSuffix = "";
    
    
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