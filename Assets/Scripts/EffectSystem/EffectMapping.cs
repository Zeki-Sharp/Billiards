using System.Collections.Generic;

/// <summary>
/// 特效事件映射管理
/// 维护事件类型到MMF对象名称的映射关系
/// </summary>
public static class EffectMapping
{
    /// <summary>
    /// 事件类型到MMF对象名称的映射字典
    /// 新增特效时，只需在此处添加映射关系
    /// </summary>
    private static readonly Dictionary<string, string> eventToMMFName = new Dictionary<string, string>
    {
        // 基础特效
        {"Hit", "Hit Effect"},
        {"Launch Effect", "Launch Effect"},
        {"Hole Enter Effect", "Hole Enter Effect"},
        {"Dead Effect", "Dead Effect"},
        {"Charge Effect", "Charge Effect"},
        
        // 新的攻击特效映射
        {"Hit Attack Effect", "Hit Attack Effect"},
        {"Be Hit Effect", "Be Hit Effect"},
        {"Skill Attack Effect", "Skill Attack Effect"},
        {"Shoot Attack Effect", "Shoot Attack Effect"},
        {"Magic Attack Effect", "Magic Attack Effect"},
        
        // 可以继续添加更多特效映射
        // {"NewEffect", "New Effect MMF Object Name"}
    };
    
    /// <summary>
    /// 获取事件类型对应的MMF对象名称
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>MMF对象名称，如果未找到返回null</returns>
    public static string GetMMFObjectName(string eventType)
    {
        if (string.IsNullOrEmpty(eventType))
            return null;
            
        return eventToMMFName.TryGetValue(eventType, out string mmfName) ? mmfName : null;
    }
    
    /// <summary>
    /// 检查事件类型是否已映射
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>是否已映射</returns>
    public static bool HasMapping(string eventType)
    {
        return !string.IsNullOrEmpty(eventType) && eventToMMFName.ContainsKey(eventType);
    }
    
    /// <summary>
    /// 获取所有已映射的事件类型
    /// </summary>
    /// <returns>事件类型列表</returns>
    public static IEnumerable<string> GetAllEventTypes()
    {
        return eventToMMFName.Keys;
    }
    
    /// <summary>
    /// 添加新的特效映射（运行时添加，用于动态扩展）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="mmfObjectName">MMF对象名称</param>
    public static void AddMapping(string eventType, string mmfObjectName)
    {
        if (!string.IsNullOrEmpty(eventType) && !string.IsNullOrEmpty(mmfObjectName))
        {
            eventToMMFName[eventType] = mmfObjectName;
        }
    }
}
