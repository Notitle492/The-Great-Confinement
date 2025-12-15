using System.Collections.Generic;
using UnityEngine;


/// 互動狀態管理器 - 跨場景記錄所有互動過的物件

public class InteractionStateManager : MonoBehaviour
{
    public static InteractionStateManager Instance { get; private set; }

    // 記錄已互動過的物件（使用唯一ID）
    private HashSet<string> interactedObjects = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[InteractionStateManager] 已初始化");
    }

    
    /// 記錄物件已被互動
    
    public void MarkAsInteracted(string objectID)
    {
        if (string.IsNullOrEmpty(objectID))
        {
            Debug.LogWarning("[InteractionStateManager] objectID 是空的");
            return;
        }

        if (!interactedObjects.Contains(objectID))
        {
            interactedObjects.Add(objectID);
            Debug.Log($"[InteractionStateManager] 記錄互動：{objectID}");
        }
    }

    
    /// 檢查物件是否已被互動過
    
    public bool HasInteracted(string objectID)
    {
        if (string.IsNullOrEmpty(objectID))
            return false;

        bool hasInteracted = interactedObjects.Contains(objectID);
        Debug.Log($"[InteractionStateManager] 檢查 {objectID}：{(hasInteracted ? "已互動" : "未互動")}");
        return hasInteracted;
    }

    
    /// 清除所有互動記錄（重置遊戲時使用）
    
    public void ClearAllInteractions()
    {
        interactedObjects.Clear();
        Debug.Log("[InteractionStateManager] 已清除所有互動記錄");
    }

    
    /// 取得所有已互動的物件ID（用於 Debug）
    
    public List<string> GetAllInteractedObjects()
    {
        return new List<string>(interactedObjects);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}