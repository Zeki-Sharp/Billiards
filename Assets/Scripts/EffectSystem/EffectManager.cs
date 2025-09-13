using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 特效管理器 - 纯事件监听器
/// 监听MMEventManager的特效事件，分发全局特效和对象特效
/// </summary>
public class EffectManager : MonoBehaviour, MMEventListener<EffectEvent>
{
    public static EffectManager Instance { get; private set; }
    
    [Header("特效系统说明")]
    [TextArea(4, 6)]
    public string systemInfo = "此系统使用 MMEventManager + EffectPlayer + 全局特效 架构\n" +
                              "全局特效：镜头摇晃、全局音效等（在EffectManager上）\n" +
                              "对象特效：粒子特效、对象动画等（在目标对象上）\n" +
                              "新增特效：只需在EffectMapping中添加映射即可";
    
    [Header("特效设置")]
    public bool enableDebugLog = true;          // 是否启用调试日志
    
    [Header("全局特效")]
    [Tooltip("全局特效播放器，自动查找 EffectManager 上的 EffectPlayer 组件")]
    private EffectPlayer globalEffectPlayer;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 自动查找全局特效播放器
        globalEffectPlayer = GetComponent<EffectPlayer>();
        if (globalEffectPlayer == null)
        {
            // 在子对象中查找 EffectPlayer
            globalEffectPlayer = GetComponentInChildren<EffectPlayer>();
            if (globalEffectPlayer == null)
            {
                Debug.LogWarning("EffectManager 及其子对象上没有找到 EffectPlayer 组件，全局特效将无法播放");
            }
            else
            {
                Debug.Log($"EffectManager 在子对象中找到全局特效播放器: {globalEffectPlayer.name}");
            }
        }
        else
        {
            Debug.Log($"EffectManager 找到全局特效播放器: {globalEffectPlayer.name}");
        }
    }
    
    void OnEnable()
    {
        // 订阅MMEventManager的特效事件
        this.MMEventStartListening<EffectEvent>();
        if (enableDebugLog)
            Debug.Log("EffectManager 已订阅 MMEventManager 特效事件");
    }
    
    void Start()
    {
        // 在Start中测试MMEventManager是否工作
        if (enableDebugLog)
        {
            Debug.Log("测试 MMEventManager 是否工作...");
            EffectEvent.Trigger("Test", Vector3.zero);
        }
    }
    
    void OnDisable()
    {
        // 取消订阅MMEventManager的特效事件
        this.MMEventStopListening<EffectEvent>();
    }
    
    /// <summary>
    /// 处理特效事件（MMEventListener接口实现）
    /// </summary>
    public void OnMMEvent(EffectEvent effectEvent)
    {
        if (enableDebugLog)
            Debug.Log($"EffectManager接收到特效事件: {effectEvent.EffectType} at {effectEvent.Position}");
        
        // 播放全局特效
        if (globalEffectPlayer != null)
        {
            globalEffectPlayer.PlayEffect(effectEvent.EffectType, effectEvent.Position, effectEvent.Direction, effectEvent.HitNormal, effectEvent.HitSpeed, effectEvent.WallHitRotationAngle, effectEvent.WallHitPositionOffset);
            if (enableDebugLog)
                Debug.Log($"播放全局{effectEvent.EffectType}特效 at {effectEvent.Position}, 速度: {effectEvent.HitSpeed:F2}");
        }
        else
        {
            if (enableDebugLog)
                Debug.LogWarning($"全局特效播放器为null，无法播放全局{effectEvent.EffectType}特效");
        }
        
        // 播放目标对象特效
        if (effectEvent.TargetObject != null)
        {
            var objectEffectPlayer = FindEffectPlayerInTarget(effectEvent.TargetObject);
            if (objectEffectPlayer != null)
            {
                objectEffectPlayer.PlayEffect(effectEvent.EffectType, effectEvent.Position, effectEvent.Direction, effectEvent.HitNormal, effectEvent.HitSpeed, effectEvent.WallHitRotationAngle, effectEvent.WallHitPositionOffset);
                if (enableDebugLog)
                    Debug.Log($"播放对象{effectEvent.EffectType}特效 - {effectEvent.TargetObject.name} at {effectEvent.Position}, 速度: {effectEvent.HitSpeed:F2}");
            }
            else
            {
                Debug.LogWarning($"目标对象 {effectEvent.TargetObject.name} 及其子对象上没有EffectPlayer组件");
            }
        }
    }
    
    
    /// <summary>
    /// 在目标对象及其子对象中查找 EffectPlayer 组件
    /// </summary>
    private EffectPlayer FindEffectPlayerInTarget(GameObject targetObject)
    {
        // 首先在目标对象本身查找
        var effectPlayer = targetObject.GetComponent<EffectPlayer>();
        if (effectPlayer != null)
        {
            return effectPlayer;
        }
        
        // 在子对象中查找名为 "Effect Player" 的对象
        Transform effectPlayerTransform = targetObject.transform.Find("Effect Player");
        if (effectPlayerTransform != null)
        {
            effectPlayer = effectPlayerTransform.GetComponent<EffectPlayer>();
            if (effectPlayer != null)
            {
                return effectPlayer;
            }
        }
        
        // 如果没找到，尝试在所有子对象中查找
        effectPlayer = targetObject.GetComponentInChildren<EffectPlayer>();
        return effectPlayer;
    }
    
}
