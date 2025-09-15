using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [Header("攻击范围设置")]
    public float rangeWidth = 2f;      // 范围宽度
    public float rangeLength = 4f;     // 范围长度
    
    [Header("颜色设置")]
    public Color previewColor = new Color(1f, 0f, 0f, 0.3f); // 预览颜色
    public Color attackColor = new Color(1f, 0f, 0f, 0.8f);  // 攻击颜色
    
    private SpriteRenderer spriteRenderer;
    private Player targetPlayer;
    private Vector2 attackDirection; // 攻击方向（在预览阶段确定）
    private bool isPreviewActive = false; // 是否正在预览状态
    
    void Start()
    {
        // 获取或添加SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 设置默认Sprite
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        }
        
        // 添加BoxCollider2D用于射线检测
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true; // 设为触发器，不参与物理碰撞
        
        // 查找玩家
        targetPlayer = FindAnyObjectByType<Player>();
        
        // 设置初始状态
        SetVisible(false);
        UpdateRangeSize();
    }
    
    
    public void SetAttackDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 设置攻击范围的旋转
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 攻击范围起点在敌人中心，终点朝向白球方向
        // 所以AttackRange的中心应该在敌人中心向攻击方向偏移rangeLength/2
        transform.localPosition = new Vector3(direction.x * rangeLength/2f, direction.y * rangeLength/2f, 0);
        
        Debug.Log($"AttackRange对齐: direction={direction}, angle={angle:F1}°, localPosition={transform.localPosition}");
    }
    
    void UpdateRangeSize()
    {
        // 设置范围大小
        transform.localScale = new Vector3(rangeLength, rangeWidth, 1f);
        
        // 同步更新Collider大小
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(1f, 1f); // 本地大小为1x1，通过scale放大
        }
    }
    
    public void ShowPreview()
    {
        Debug.Log($"AttackRange {name} 开始显示预览，spriteRenderer={spriteRenderer}, isPreviewActive={isPreviewActive}");
        
        SetVisible(true);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = previewColor;
            Debug.Log($"AttackRange {name} 设置颜色: {previewColor}, enabled={spriteRenderer.enabled}");
        }
        
        // 只在第一次进入预览状态时更新攻击方向
        if (!isPreviewActive && targetPlayer != null)
        {
            Vector2 currentDirection = (targetPlayer.transform.position - transform.parent.position).normalized;
            attackDirection = currentDirection; // 保存新的攻击方向
            SetAttackDirection(currentDirection);
            Debug.Log($"AttackRange {name} 更新攻击方向: {currentDirection}, 玩家位置: {targetPlayer.transform.position}, 敌人位置: {transform.parent.position}");
        }
        
        isPreviewActive = true;
        Debug.Log($"AttackRange {name} 显示攻击范围预览完成");
    }
    
    public void ShowAttack()
    {
        SetVisible(true);
        spriteRenderer.color = attackColor;
        
        // 攻击阶段：使用预览阶段确定的固定方向
        SetAttackDirection(attackDirection);
        
        Debug.Log("显示攻击范围攻击状态");
        
        // 闪烁一次红色
        StartCoroutine(FlashRed());
    }
    
    public void Hide()
    {
        SetVisible(false);
        isPreviewActive = false; // 重置预览状态
        Debug.Log("隐藏攻击范围");
    }
    
    void SetVisible(bool visible)
    {
        if (spriteRenderer == null)
        {
            // 如果spriteRenderer未初始化，先初始化
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        spriteRenderer.enabled = visible;
    }
    
    System.Collections.IEnumerator FlashRed()
    {
        // 闪烁效果
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    public bool IsPlayerInRange()
    {
        if (targetPlayer == null) return false;
        
        // 从玩家位置垂直发射射线，检测与攻击范围的重叠
        Vector2 playerPos = targetPlayer.transform.position;
        
        // 获取攻击范围的边界
        Bounds attackBounds = GetAttackRangeBounds();
        
        // 检查玩家是否在攻击范围内
        bool inRange = attackBounds.Contains(playerPos);
        
        if (inRange)
        {
            Debug.Log($"玩家在攻击范围内: 玩家位置={playerPos}, 攻击范围边界={attackBounds}");
        }
        
        return inRange;
    }
    
    Bounds GetAttackRangeBounds()
    {
        // 计算攻击范围的世界坐标边界
        Vector3 center = transform.position;
        Vector3 size = new Vector3(rangeLength, rangeWidth, 0);
        return new Bounds(center, size);
    }
    
}
