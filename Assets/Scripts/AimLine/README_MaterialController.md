# 瞄准线材质控制器使用指南

## 概述
`AimLineMaterialController` 专门用于控制瞄准线的材质效果，包括短划密度调整和流动效果。

## 集成完成
已成功集成到 `TestAimController` 中，支持以下功能：

### 1. 自动初始化
- 如果未手动设置材质控制器，系统会自动创建
- 在 `TestAimController` 初始化时自动设置

### 2. 动态材质应用
- 每个分段线段会根据长度自动调整 `_UScale` 参数
- 短划密度与线段长度成正比
- 使用 `MaterialPropertyBlock` 避免创建多个材质实例

### 3. 流动效果
- 支持短划的流动动画
- 可通过 `SetMaterialFlowEffect()` 控制开关和速度

## 使用方法

### 在Inspector中设置
1. 将你的瞄准线材质拖拽到 `Material Controller` 的 `Aim Line Material` 字段
2. 调整 `Reference Length`（参考长度，默认1单位）
3. 调整 `Base UScale`（基础UScale值，默认15）
4. 设置 `Auto Scroll Speed`（流动速度）

### 通过代码控制
```csharp
// 获取材质控制器
AimLineMaterialController materialController = testAimController.GetMaterialController();

// 设置材质
materialController.SetAimLineMaterial(yourAimLineMaterial);

// 控制流动效果
materialController.SetFlowEffect(true, 2f); // 启用流动，速度2

// 重置流动时间
materialController.ResetFlowTime();

// 获取状态信息
string stats = materialController.GetMaterialStats();
Debug.Log(stats);
```

## 参数说明

### 材质设置
- **Aim Line Material**: 瞄准线材质（必须设置）
- **Reference Length**: 参考长度，用于计算UScale比例
- **Base UScale**: 基础UScale值
- **Min/Max UScale**: UScale的限制范围

### 流动效果设置
- **Auto Scroll Speed**: 自动滚动速度
- **Enable Flow Effect**: 是否启用流动效果

## 工作原理

1. **UScale计算**: `新UScale = 基础UScale * (线段长度 / 参考长度)`
2. **材质应用**: 使用 `MaterialPropertyBlock` 动态设置 `_UScale` 参数
3. **流动效果**: 通过 `_UVOffset` 参数实现短划流动

## 注意事项

- 确保材质支持 `_UScale`、`_UVOffset`、`_AutoScrollSpeed` 参数
- 参考长度建议设置为1单位，便于调试
- 流动效果在 `Update()` 中更新，确保性能考虑
