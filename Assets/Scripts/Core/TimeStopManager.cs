using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 时停管理器 - 管理游戏对象的时停效果
/// </summary>
public class TimeStopManager : MonoBehaviour
{
    [Header("时停设置")]
    [SerializeField] private float timeStopScale = 0f; // 完全时停
    [SerializeField] private float partialTimeStopScale = 0.1f; // 部分时停
    [SerializeField] private float normalTimeScale = 1f; // 正常时间
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 事件
    public System.Action OnTimeStopStart; // 时停开始
    public System.Action OnTimeStopEnd; // 时停结束
    public System.Action OnPartialTimeStopStart; // 部分时停开始
    
    public void ApplyTimeStop()
    {
        Time.timeScale = timeStopScale;
        OnTimeStopStart?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("TimeStopManager: 应用完全时停");
        }
    }
    
    public void ApplyPartialTimeStop()
    {
        Time.timeScale = partialTimeStopScale;
        OnPartialTimeStopStart?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("TimeStopManager: 应用部分时停");
        }
    }
    
    public void ReleaseTimeStop()
    {
        Time.timeScale = normalTimeScale;
        OnTimeStopEnd?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("TimeStopManager: 释放时停");
        }
    }
    
    public bool IsTimeStopped()
    {
        return Time.timeScale <= timeStopScale;
    }
    
    public bool IsPartiallyTimeStopped()
    {
        return Time.timeScale <= partialTimeStopScale && Time.timeScale > timeStopScale;
    }
}
