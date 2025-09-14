# 伤害数字系统开发计划

## 目标
创建一个可复用的伤害数字显示系统，支持自动触发、随机位移、边缘防撞和对象池管理。

## 系统架构

### 核心组件
1. **DamageTextManager** - 全局单例管理器（对象池管理）
2. **DamageText** - 单个伤害数字脚本（极简化，只负责参数设置）
3. **DamageTextConfig** - 配置数据类（简化版）

### 职责分离
- **DamageText 脚本**: 只负责设置参数（数值、位置、样式）
- **MMF Player**: 完全控制所有动画效果
- **DamageTextManager**: 管理对象池和实例生命周期

### 事件系统集成
- 直接使用 MMGameEvent 系统，无需额外工具类
- 通过 "DamageText" 事件触发伤害数字显示
- 完全符合项目现有架构

## 详细设计

### 1. DamageTextManager (单例管理器)
**职责：**
- 管理全局伤害数字的对象池
- 处理边缘检测和位置计算
- 提供伤害数字实例给 MMF Player
- 处理随机位移和自动回收

**核心功能：**
- 对象池管理（预创建、获取、回收）
- 屏幕边缘检测和位置调整
- 随机位移计算
- 实例生命周期管理

### 2. DamageText (单个伤害数字脚本)
**职责：**
- 设置伤害数值和样式
- 提供回收接口给 MMF

**核心功能：**
- 设置文本内容
- 设置显示位置
- 设置样式（颜色、字体、描边）
- 提供 ReturnToPool() 接口

**注意：** 不处理任何动画逻辑，完全由 MMF 控制

### 3. DamageTextConfig (配置类)
**配置参数：**
- 伤害数字颜色
- 字体设置（大小、描边）
- 随机位移范围
- 边缘检测边距
- 对象池大小

**注意：** 动画相关参数由 MMF 控制，不在此配置中

## Prefab 配置要求

### DamageTextPrefab 结构
```
DamageTextPrefab
├── Text (TextMeshPro) - 显示伤害数值
├── DamageText (脚本) - 控制逻辑
└── MMF_Player (组件) - 控制动画效果
```

**注意：** 
- 预制体不包含 Canvas，作为纯 UI 元素
- Canvas 由 DamageTextManager 自动创建和管理
- 每个实例都有独立的 MMF_Player 控制动画

## MMF 配置要求

### Be Hit Effect MMF Player 配置
**需要添加的反馈：**
- `MMF_Player` - 指向伤害数字实例的 MMF Player
- 配置参数传递（Position, Damage）

### DamageText 预制体 MMF Player 配置
**必需动画效果：**
1. **淡入效果** - MMF_CanvasGroup 控制透明度
2. **向上移动** - MMF_Position 控制位置移动
3. **缩放动画** - MMF_Scale 控制缩放
4. **淡出效果** - MMF_CanvasGroup 控制透明度
5. **回收触发** - 最后一个反馈调用 DamageText.ReturnToPool()

**MMF 反馈配置：**
- `MMF_CanvasGroup` - 控制透明度（淡入淡出）
- `MMF_Position` - 控制位置移动（向上飘移）
- `MMF_Scale` - 控制缩放（出现时的缩放效果）
- `MMF_SetActive` - 控制激活状态
- 自定义反馈 - 调用 DamageTextManager 获取实例

## 事件系统集成

### 调用流程
```
受伤事件 → Be Hit Effect MMF Player → MMF_Player Chain → 伤害数字 MMF Player
                                                                    ↓
                                                            DamageTextManager (对象池)
                                                                    ↓
                                                            DamageText 实例
```

### 事件触发方式
```csharp
// 直接使用 MMGameEvent 触发伤害数字
MMGameEvent.Trigger(
    eventName: "DamageText",
    vector3Parameter: position,
    intParameter: Mathf.RoundToInt(damage * 100),
    stringParameter: ""  // 不需要类型参数
);
```

### MMF 配置示例
```csharp
// Be Hit Effect MMF Player 中的配置
// 1. 3D特效反馈 (粒子、动画等)
// 2. 音效反馈
// 3. MMF_Player 反馈
//    - Target: 伤害数字 MMF Player
//    - Position: 传递伤害位置
//    - 其他参数: 传递伤害数值等
```

## 技术实现要点

### 1. 对象池设计
- 预创建 20-50 个 DamageText 实例
- 使用 Queue<DamageText> 管理可用对象
- MMF Player 通过 DamageTextManager 获取实例
- 避免频繁的 Instantiate/Destroy 调用

### 2. 随机位移算法
- 在目标位置周围生成圆形随机偏移
- 偏移范围可配置（默认 0.5-1.5 单位）
- 避免伤害数字重叠显示

### 3. 边缘防撞检测
- 使用 `Camera.WorldToScreenPoint` 检测屏幕边界
- 动态调整位置确保完全可见
- 考虑伤害数字的尺寸和边距

### 4. 性能优化
- 对象池预创建，减少运行时分配
- 批量处理同时出现的伤害数字
- 动画由 MMF 管理，无需脚本协程

## 开发步骤

### ✅ 阶段1：基础架构（已完成）
1. ✅ 创建 DamageTextConfig 配置类
2. ✅ 创建 DamageTextManager 单例管理器
3. ✅ 创建 DamageText 脚本（极简化版）
4. ✅ 集成 MMGameEvent 事件系统

### ✅ 阶段2：对象池系统（已完成）
1. ✅ 实现对象池的预创建和回收机制
2. ✅ 实现随机位移算法
3. ✅ 实现边缘防撞检测
4. ✅ 测试对象池性能

### 🔄 阶段3：MMF 配置（进行中）
1. **需要用户创建 DamageTextPrefab**
   - 包含 Text (TextMeshPro) 组件
   - 包含 DamageText 脚本
   - 不需要 MMF Player 组件

2. **需要用户创建伤害数字 MMF Player**
   - 配置 MMF_CanvasGroup（淡入淡出）
   - 配置 MMF_Position（向上移动）
   - 配置 MMF_Scale（缩放动画）
   - 配置自定义反馈（调用 DamageTextManager）
   - 配置回收触发（调用 DamageText.ReturnToPool()）

3. **需要用户配置 Be Hit Effect MMF Player**
   - 添加 MMF_Player 反馈
   - 指向伤害数字 MMF Player
   - 配置参数传递

### 阶段4：测试和优化
1. 测试 MMF 动画效果
2. 测试参数传递和实例获取
3. 性能测试（大量伤害数字同时显示）
4. 边界情况测试（屏幕边缘、快速连续伤害）
5. 最终优化和文档

## 验收标准
- 伤害数字能正确显示在目标位置
- 随机位移和边缘防撞正常工作
- 对象池有效减少 GC 压力
- MMF 动画效果流畅
- 参数传递准确无误
- 性能满足要求（同时显示 50+ 伤害数字）

## 下一步需要用户完成的配置

### 1. 创建 DamageTextPrefab
- 创建预制体，包含 Text (TextMeshPro) 组件
- 添加 DamageText 脚本
- 设置 DamageTextManager 的预制体引用

### 2. 创建伤害数字 MMF Player
- 创建新的 MMF Player 对象
- 配置以下 MMF 反馈：
  - MMF_CanvasGroup（淡入淡出效果）
  - MMF_Position（向上移动）
  - MMF_Scale（缩放动画）
  - 自定义反馈（调用 DamageTextManager.GetDamageTextInstance()）
  - 回收触发（调用 DamageText.ReturnToPool()）

### 3. 配置 Be Hit Effect MMF Player
- 在现有的 Be Hit Effect MMF Player 中添加 MMF_Player 反馈
- 指向伤害数字 MMF Player
- 配置参数传递（Position, Damage）

## 注意事项
- 遵循项目现有的 MMF 架构约定
- 动画完全由 MMF 控制，脚本不处理动画逻辑
- 保持代码的简洁性，避免过度设计
- 提供适当的调试日志
