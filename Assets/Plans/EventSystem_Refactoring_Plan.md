# 事件系统重构计划

## 目标
统一使用MMEventManager作为整个游戏的事件系统核心，消除当前三套并行事件系统的混乱状态。

## 当前问题
1. **三套并行事件系统**：EventBus、MMEventManager、直接调用
2. **重复的特效类型映射**：EffectManager和EffectPlayer都有映射逻辑
3. **职责混乱**：EffectManager既管理全局特效又提供直接调用接口
4. **配置分散**：特效名称硬编码在多个地方

## 重构思路

### 1. 统一事件入口
- **唯一触发方式**：只通过MMEventManager触发所有事件
- **移除冗余**：删除EventBus和直接调用方式
- **保持简化**：提供简化的触发接口，内部调用MMEventManager

### 2. 事件类型分类
- **EffectEvent**：特效相关事件（现有）
- **GameStateEvent**：游戏状态事件（阶段转换、游戏开始/结束等）
- **UIEvent**：UI相关事件（界面切换、按钮响应等）
- **AudioEvent**：音频相关事件（音效、音乐等）

### 3. 职责分离
- **EffectManager**：纯事件监听器，只处理特效事件
- **EffectPlayer**：纯特效播放器，不处理事件和配置
- **EventTrigger**：统一的事件触发接口
- **EffectMapping**：特效事件映射管理

### 4. 利用MMF现有配置
- **MMF Inspector配置**：直接在Unity编辑器中配置特效序列
- **代码映射**：简单的字典映射事件类型到MMF对象名称
- **自动查找**：EffectPlayer根据映射自动查找MMF对象
- **零配置扩展**：新增特效只需在MMF中创建对象，在映射表中添加一行

## 实施约定

### 事件命名约定
- **特效事件**：`"Hit"`, `"WallHit"`, `"Launch"`, `"HoleEnter"`等
- **游戏状态**：`"PhaseChanged"`, `"GameStart"`, `"GameEnd"`, `"ScoreChanged"`等
- **UI事件**：`"ShowDialog"`, `"HideUI"`, `"ButtonClick"`等
- **音频事件**：`"PlaySound"`, `"StopMusic"`, `"VolumeChanged"`等

### 事件参数约定
- **EffectEvent**：Position, Direction, TargetObject, HitNormal, HitSpeed等
- **GameStateEvent**：StateName, IntValue, FloatValue, StringValue, BoolValue
- **UIEvent**：EventName, IntValue, StringValue, BoolValue
- **AudioEvent**：SoundName, Volume, BoolValue

### 监听器实现约定
- 所有监听器必须实现`MMEventListener<T>`接口
- 在OnEnable/OnDisable中正确订阅/取消订阅
- 事件处理方法命名：`OnMMEvent(EventType event)`
- 使用switch-case处理不同事件类型

### 触发接口约定
- 提供静态方法类：`EventTrigger`, `GameEventTrigger`, `UIEventTrigger`等
- 方法命名：动词+名词，如`Hit()`, `PhaseChanged()`, `ShowDialog()`
- 参数顺序：位置、方向、目标对象、其他参数
- 内部调用MMEventManager，不暴露复杂参数

### 配置约定
- **MMF对象命名**：使用描述性名称，如`Hit Effect`, `Wall Hit Effect`, `Launch Effect`
- **事件映射**：在EffectMapping类中维护事件类型到MMF对象名称的映射
- **MMF配置**：在Unity Inspector中配置特效序列、时间轴、参数等
- **命名规范**：驼峰命名，如`hit`, `wallHit`, `launch`

## 迁移步骤

### 阶段1：准备新架构
1. 创建新的事件结构体
2. 创建EventTrigger静态类
3. 创建EffectMapping映射类
4. 更新EffectEvent，添加静态触发方法

### 阶段2：迁移特效系统
1. 重构EffectManager为纯事件监听器（保留，重新定义职责）
2. 重构EffectPlayer使用EffectMapping
3. 更新所有特效触发调用
4. 测试MMF对象自动查找功能

### 阶段3：迁移游戏状态（暂不执行，相关内容之后完全重写）
1. 将GameManager的Action事件迁移到MMEventManager
2. 创建GameStateEvent和GameEventTrigger
3. 更新UI系统监听游戏状态事件
4. 移除传统事件系统
5. 注意：游戏状态逻辑仍在代码中管理，MMEventSystem只负责事件发布和监听

### 阶段4：清理和优化
1. 删除EventBus.cs
2. 删除EventFactory.cs
3. 清理所有直接调用代码
4. 添加事件系统文档

## 验收标准
- 所有事件通过MMEventManager触发
- 新增特效只需在MMF中创建对象，在映射表中添加一行
- 事件监听器正确实现接口
- 代码中无直接调用EffectManager的情况
- 事件命名符合约定规范
- MMF对象自动查找功能正常工作

## 注意事项
- 保持向后兼容，逐步迁移
- 每个阶段完成后进行测试
- 保留调试日志，便于问题排查
- 更新相关文档和注释
