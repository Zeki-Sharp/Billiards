using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// 过渡状态管理器 - 管理从蓄力状态到正常状态的过渡
/// </summary>
public class TransitionManager : MonoBehaviour
{
    [Header("过渡设置")]
    [SerializeField] private float transitionDuration = 3f; // 过渡持续时间
    [SerializeField] private bool enableAutoTransition = true; // 自动过渡
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    
    private float transitionTimer = 0f;
    private bool isTransitioning = false;
    
    // 事件
    public System.Action OnTransitionStart; // 过渡开始
    public System.Action OnTransitionEnd; // 过渡结束
    
    void Update()
    {
        if (isTransitioning)
        {
            transitionTimer -= Time.deltaTime;
            
            if (transitionTimer <= 0f)
            {
                EndTransition();
            }
        }
    }
    
    public void StartTransition()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        transitionTimer = transitionDuration;
        OnTransitionStart?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log($"TransitionManager: 开始过渡，持续时间: {transitionDuration}秒");
        }
    }
    
    public void EndTransition()
    {
        if (!isTransitioning) return;
        
        isTransitioning = false;
        transitionTimer = 0f;
        OnTransitionEnd?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("TransitionManager: 过渡结束");
        }
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    public float GetTransitionProgress()
    {
        if (!isTransitioning) return 1f;
        return 1f - (transitionTimer / transitionDuration);
    }
    
    public float GetRemainingTime()
    {
        return transitionTimer;
    }
}
