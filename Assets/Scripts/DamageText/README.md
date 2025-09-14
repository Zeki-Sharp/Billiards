# 伤害数字系统使用说明

## 系统架构

### 职责分离
- **DamageText 脚本**: 只负责设置参数（数值、位置、样式）
- **MMF Player**: 完全控制所有动画效果
- **DamageTextManager**: 管理对象池和实例生命周期

### 文件结构
```
Assets/Scripts/DamageText/
├── DamageText.cs          - 伤害数字脚本（简化版）
├── DamageTextConfig.cs    - 配置数据
├── DamageTextManager.cs   - 对象池管理器
└── README.md              - 使用说明
```

## 使用方法

### 1. 触发伤害数字
```csharp
// 直接使用 MMGameEvent 触发
MMGameEvent.Trigger(
    eventName: "DamageText",
    vector3Parameter: position,
    intParameter: Mathf.RoundToInt(damage * 100),
    stringParameter: ""  // 不需要类型参数
);
```

### 2. MMF 配置要求

#### DamageTextPrefab 结构
```
DamageTextPrefab
├── Text (TextMeshPro) - 显示伤害数值
├── DamageText (脚本) - 控制逻辑
└── MMF_Player (组件) - 控制动画效果
```

**注意：** 
- 预制体不包含 Canvas，作为纯 UI 元素
- Canvas 由 DamageTextManager 自动创建和管理
- 所有伤害数字实例都作为同一个 Canvas 的子对象
- 每个实例都有独立的 MMF_Player 控制动画

#### MMF Player 配置
- **淡入效果** - MMF_CanvasGroup
- **向上移动** - MMF_Position
- **缩放动画** - MMF_Scale
- **淡出效果** - MMF_CanvasGroup
- **回收触发** - 最后一个反馈调用 `DamageText.ReturnToPool()`

### 3. 配置说明

#### DamageTextConfig 参数
- `damageColor` - 伤害数字颜色
- `fontSize` - 字体大小
- `enableOutline` - 是否启用描边
- `outlineColor` - 描边颜色
- `outlineWidth` - 描边宽度
- `randomOffsetRange` - 随机位移范围
- `screenMargin` - 屏幕边距

## 优势

1. **职责清晰** - 脚本只管理参数，MMF 控制动画
2. **配置化** - 所有动画效果在 MMF 中配置
3. **性能优化** - 对象池避免频繁创建销毁，共享 Canvas
4. **自动化管理** - Canvas 自动创建和管理，无需手动配置
5. **易于维护** - 动画修改只需调整 MMF 配置
6. **统一架构** - 使用项目现有的 MMGameEvent 系统

## 注意事项

- 动画完全由 MMF 控制，脚本不处理动画逻辑
- 回收通过 MMF 的最后一个反馈触发
- 配置参数主要用于样式设置，动画参数由 MMF 控制
