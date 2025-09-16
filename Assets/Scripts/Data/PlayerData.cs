using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("玩家基本信息")]
    public string playerName;
    public GameObject playerPrefab;
    public Sprite playerIcon;
    
    [Header("物理数据")]
    public BallData ballData;                   // 打包的物理数据
    
    [Header("战斗配置")]
    public float maxHealth = 100f;
    public float damage = 10f;
    public float attackCooldown = 1f;
    public float moveSpeed = 2f;
    
    [Header("玩家特有配置")]
    public bool canLevelUp = true;
    public int startingLevel = 1;
    public int maxLevel = 100;
    public float experienceMultiplier = 1f;
    
    [Header("技能配置")]
    // public SkillData[] availableSkills;      // 暂时注释，等技能系统实现
    
    [Header("攻击配置")]
    public float attackRange = 3f;
    public float contactRadius = 1f;
    
    [Header("微操配置")]
    public float microMoveSpeed = 5f;
}
