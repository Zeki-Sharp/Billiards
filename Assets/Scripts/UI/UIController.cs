using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("UI面板")]
    public GameUIPanel gameUIPanel;
    
    
    void Start()
    {
        // 查找GameUIPanel
        if (gameUIPanel == null)
        {
            gameUIPanel = FindFirstObjectByType<GameUIPanel>();
        }
        
        Debug.Log("UIController: 初始化完成");
    }
}
