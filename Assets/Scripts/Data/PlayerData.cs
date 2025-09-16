using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("玩家基本信息")]
    public string playerName;
    public GameObject playerPrefab;
    public Sprite playerIcon;
    
    [Header("物理数据")]
    public BallData ballData;
    
    [Header("战斗配置")]
    public float maxHealth = 100f;
    public float damage = 10f;
    
    [Header("移动配置")]
    public float microMoveSpeed = 5f;
    
    [Header("玩家特有配置")]
    public bool canLevelUp = true;
    public int startingLevel = 1;
    public int maxLevel = 100;
    public float experienceMultiplier = 1f;
}