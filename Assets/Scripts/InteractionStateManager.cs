using System.Collections.Generic;
using UnityEngine;


/// 互動狀態管理器 - 跨場景記錄所有互動過的物件
/// 擴充版：支援儲存更多細節資料（互動次數、獎勵狀態等）

public class InteractionStateManager : MonoBehaviour
{
    public static InteractionStateManager Instance { get; private set; }

    // 記錄已互動過的物件（使用唯一ID）
    private HashSet<string> interactedObjects = new HashSet<string>();

    // 新增：記錄互動的詳細資料（Key: 物件ID, Value: 資料字串）
    private Dictionary<string, string> interactionData = new Dictionary<string, string>();

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

        return interactedObjects.Contains(objectID);
    }

    /// 新增：設定互動的詳細資料（例如：互動次數、獎勵狀態）
    public void SetInteractionData(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[InteractionStateManager] key 是空的");
            return;
        }

        if (interactionData.ContainsKey(key))
        {
            interactionData[key] = value;
        }
        else
        {
            interactionData.Add(key, value);
        }

        Debug.Log($"[InteractionStateManager] 設定資料：{key} = {value}");
    }

    /// 新增：取得互動的詳細資料
    
    public string GetInteractionData(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        if (interactionData.TryGetValue(key, out string value))
        {
            return value;
        }

        return null;
    }


    /// 清除所有互動記錄（重置遊戲時使用）

    public void ClearAllInteractions()
    {
        interactedObjects.Clear();
        interactionData.Clear(); // 同時清除詳細資料
        Debug.Log("[InteractionStateManager] 已清除所有互動記錄");
    }

    
    /// 取得所有已互動的物件ID（用於 Debug）
    
    public List<string> GetAllInteractedObjects()
    {
        return new List<string>(interactedObjects);
    }

    /// 新增：取得所有互動資料（用於 Debug）
    
    public Dictionary<string, string> GetAllInteractionData()
    {
        return new Dictionary<string, string>(interactionData);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}