# 状态机通信架构文档

## 概述

本文档描述了台球项目中两个核心状态机之间的基于MM事件系统的通信机制，实现了状态机间的解耦和独立运行。

## 核心原则

### 1. 状态机独立性
- **PlayerStateMachine**：只管理玩家状态（Idle/Charging/Moving）
- **GameFlowController**：只管理游戏流程状态（Normal/Charging/Transition）
- 两个状态机不直接访问对方的内部状态

### 2. 事件驱动通信
- 所有状态机间通信通过MM事件系统进行
- 状态变化时自动触发相应事件
- 其他状态机监听相关事件并做出响应

### 3. 单向依赖
- 状态机只负责自己的状态逻辑
- 通过事件通知其他系统状态变化
- 不直接调用其他状态机的方法

## 事件类型定义

### 1. PlayerStateChangeEvent
```csharp
public struct PlayerStateChangeEvent
{
    public string FromState;        // 原状态
    public string ToState;          // 目标状态
    public string StateType;        // 状态类型：Idle, Charging, Moving
    public bool CanMove;            // 是否可以移动
    public bool CanCharge;          // 是否可以蓄力
    public bool IsPhysicsMoving;    // 是否在物理移动
}
```

### 2. GameFlowStateChangeEvent
```csharp
public struct GameFlowStateChangeEvent
{
    public string FromState;        // 原状态
    public string ToState;          // 目标状态
    public string FlowType;         // 流程类型：Normal, Charging, Transition
    public bool IsTimeStopped;      // 是否时停
    public bool IsPartialTimeStop;  // 是否部分时停
    public bool CanPlayerMove;      // 玩家是否可以移动
}
```

### 3. GameStateEvent（请求事件）
```csharp
// 用于状态机间的请求通信
"RequestCharging"    // 请求进入蓄力状态
"RequestTransition"  // 请求进入过渡状态
"RequestNormal"      // 请求回到正常状态
"ForceIdle"          // 强制切换到空闲状态
```

## 通信流程

### 1. 蓄力流程
```
PlayerInputHandler → EventTrigger.RequestChargingState() 
    → GameStateEvent("RequestCharging") 
    → GameFlowController: Normal → Charging
    → PlayerStateMachine: Idle → Charging
```

### 2. 发射流程
```
PlayerStateMachine.LaunchCharged() 
    → SwitchToState(Moving) 
    → GameFlowController: 保持不变（仍然是Charging）
```

### 3. 停止流程
```
PlayerCore.OnBallStopped() 
    → PlayerStateMachine: Moving → Idle
    → EventTrigger.RequestTransitionState()
    → GameFlowController: Charging → Transition
```

### 4. 过渡完成流程（暂时空着）
```
GameFlowController: Transition → Normal
    → 触发 ForceIdle 事件
    → PlayerStateMachine: 保持 Idle
```

## 状态机职责

### PlayerStateMachine
- **状态管理**：Idle ↔ Charging ↔ Moving
- **事件触发**：状态变化时触发PlayerStateChangeEvent
- **事件监听**：监听RequestCharging、ForceIdle等请求事件
- **自动请求**：Moving时请求Transition，Idle时请求Normal

### GameFlowController
- **状态管理**：Normal ↔ Charging ↔ Transition
- **事件触发**：状态变化时触发GameFlowStateChangeEvent
- **事件监听**：监听RequestCharging、RequestTransition、RequestNormal等请求
- **系统协调**：管理时停、过渡、敌人等系统

## 关键优势

### 1. 解耦性
- 状态机间无直接依赖
- 易于测试和维护
- 支持独立开发

### 2. 可扩展性
- 新增状态机只需监听相关事件
- 事件系统支持多对多通信
- 易于添加新的状态变化逻辑

### 3. 调试友好
- 所有通信都有事件日志
- 状态变化可追踪
- 问题定位更容易

## 使用示例

### 添加新的状态机
```csharp
public class NewStateMachine : MonoBehaviour, MMEventListener<PlayerStateChangeEvent>
{
    void OnEnable()
    {
        this.MMEventStartListening<PlayerStateChangeEvent>();
    }
    
    public void OnMMEvent(PlayerStateChangeEvent playerEvent)
    {
        // 根据玩家状态变化做出响应
        switch (playerEvent.StateType)
        {
            case "Moving":
                // 玩家开始移动时的处理
                break;
        }
    }
}
```

### 触发状态变化
```csharp
// 请求进入蓄力状态
EventTrigger.RequestChargingState();

// 触发玩家状态变化事件
EventTrigger.PlayerStateChanged("Idle", "Charging", "Charging", false, false, false);
```

## 注意事项

1. **事件顺序**：确保事件触发的顺序正确，避免循环依赖
2. **状态一致性**：确保两个状态机的状态保持逻辑一致
3. **性能考虑**：避免过于频繁的事件触发
4. **错误处理**：添加适当的状态验证和错误处理

## 未来扩展

1. **状态持久化**：支持状态机的状态保存和恢复
2. **状态历史**：记录状态变化历史用于调试
3. **状态验证**：添加状态转换的合法性检查
4. **性能监控**：监控状态机性能指标
