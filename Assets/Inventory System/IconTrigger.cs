using UnityEngine;
using System.Collections.Generic;


/// 專門處理圖示互動系統（無對話）
/// 支援：直接給圖示、條件給圖示、自動消除圖示
/// 支援同步到 InventoryUI 和 PuzzleUI


public class IconTrigger : MonoBehaviour
{

    [Header("=== 物件識別 ===")]
    [Tooltip("唯一識別ID(用於跨場景記錄互動狀態)")]
    public string uniqueObjectID;

    [Header("=== 基本設定 ===")]
    [Tooltip("互動後要給予的圖示")]
    public IconDataSO iconToGive;

    [Tooltip("對應的背包物品(可選，如果要同步到背包的話)")]
    public Item itemToGive;

    [Tooltip("是否只能互動一次")]
    public bool oneTimeInteraction = true;

    [Tooltip("互動後物件是否消失")]
    public bool disappearAfterInteraction = false;

    [Header("=== 條件互動 ===")]
    [Tooltip("是否需要擁有特定圖示才能互動")]
    public bool requiresSpecificIcon = false;

    [Tooltip("需要擁有的圖示ID(例如:7)")]
    public string requiredIconID;

    [Tooltip("未滿足條件時的提示訊息")]
    public string failureMessage = "需要特定道具才能互動";

    [Header("=== 自動消除圖示 ===")]
    [Tooltip("獲得新圖示時要自動移除的舊圖示ID列表")]
    public List<string> iconsToRemoveOnSuccess = new List<string>();

    [Header("=== 音效 ===")]
    [Tooltip("成功互動的音效")]
    public AudioClip successSound;

    [Tooltip("失敗互動的音效")]
    public AudioClip failureSound;

    private bool hasInteracted = false;

    private void Start()
    {
        // 遊戲開始時檢查是否已經互動過
        if (InteractionStateManager.Instance != null && oneTimeInteraction)
        {
            if (InteractionStateManager.Instance.HasInteracted(uniqueObjectID))
            {
                hasInteracted = true;
                Debug.Log($"[IconTrigger] {uniqueObjectID} 已記錄為互動過，設定 hasInteracted=true");

                // 如果設定為互動後消失，直接隱藏物件
                if (disappearAfterInteraction)
                {
                    gameObject.SetActive(false);
                    Debug.Log($"[IconTrigger] {uniqueObjectID} 已互動過且設定為消失，物件已隱藏");
                }
            }
        }
    }

    /// 供 ObjectTrigger 呼叫的互動方法
    
    public void Interact()
    {
        Debug.Log($"[IconTrigger] {gameObject.name} Interact() 被呼叫");

        // 檢查是否已經互動過(跨場景記錄)
        if (oneTimeInteraction)
        {
            if (InteractionStateManager.Instance != null)
            {
                if (InteractionStateManager.Instance.HasInteracted(uniqueObjectID))
                {
                    Debug.Log($"[IconTrigger] {uniqueObjectID} 已經互動過(跨場景記錄)，跳過");
                    return;
                }
            }
            else if (hasInteracted)
            {
                Debug.Log($"[IconTrigger] {gameObject.name} 已經互動過(本地記錄)，跳過");
                return;
            }
        }

        // 檢查是否需要特定圖示
        if (requiresSpecificIcon)
        {
            if (!HasRequiredIcon())
            {
                Debug.Log($"[IconTrigger] 玩家未擁有圖示 {requiredIconID}，無法互動");
                ShowFailureMessage();
                PlaySound(failureSound);
                return;
            }
        }

        // 執行互動
        PerformInteraction();
    }

    
    /// 檢查玩家是否擁有所需圖示
    
    private bool HasRequiredIcon()
    {
        if (string.IsNullOrEmpty(requiredIconID))
            return true;

        if (IconManager.Instance == null)
        {
            Debug.LogWarning("[IconTrigger] IconManager.Instance 是 null");
            return false;
        }

        var unlockedIcons = IconManager.Instance.GetUnlockedIcons();
        foreach (var icon in unlockedIcons)
        {
            if (icon.id == requiredIconID)
            {
                Debug.Log($"[IconTrigger] 玩家擁有所需圖示:{requiredIconID}");
                return true;
            }
        }

        Debug.Log($"[IconTrigger] 玩家未擁有所需圖示:{requiredIconID}");
        return false;
    }

    
    /// 執行互動邏輯
    
    private void PerformInteraction()
    {
        if (iconToGive == null)
        {
            Debug.LogWarning($"[IconTrigger] {gameObject.name} 的 iconToGive 是 null");
            return;
        }

        if (IconManager.Instance == null)
        {
            Debug.LogError("[IconTrigger] IconManager.Instance 是 null");
            return;
        }

        // 1. 給予新圖示到 PuzzleUI(解謎介面)
        IconData newIcon = iconToGive.ToIconData();

        // 如果有關聯背包物品，設定 linkedInventoryItemID
        if (itemToGive != null)
        {
            newIcon.linkedInventoryItemID = itemToGive.ItemID;
            Debug.Log($"[IconTrigger] 圖示 {iconToGive.id} 關聯背包物品 ID: {itemToGive.ItemID}");
        }

        bool iconAdded = IconManager.Instance.AddIcon(newIcon);

        if (iconAdded)
        {
            Debug.Log($"[IconTrigger] 成功給予圖示到解謎介面:{iconToGive.displayName} (ID: {iconToGive.id})");

            // 2. 同步加入到背包 InventoryUI(如果有設定 itemToGive)
            if (itemToGive != null)
            {
                if (InventoryManager.Instance != null)
                {
                    bool itemAdded = InventoryManager.Instance.AddItem(itemToGive);
                    if (itemAdded)
                    {
                        Debug.Log($"[IconTrigger] 成功加入背包:{itemToGive.ItemName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[IconTrigger] 背包已滿，無法加入:{itemToGive.ItemName}");
                    }
                }
                else
                {
                    Debug.LogWarning("[IconTrigger] InventoryManager.Instance 是 null，無法同步到背包");
                }
            }

            // 3. 移除舊圖示(如果有設定)
            RemoveOldIcons();

            // 4. 播放成功音效
            PlaySound(successSound);

            // 額外播放 Pickup 音效(與 ItemTriggerStatic 保持一致)
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D("Pickup");
            }

            // 5. 記錄互動狀態到跨場景管理器
            if (InteractionStateManager.Instance != null && oneTimeInteraction)
            {
                InteractionStateManager.Instance.MarkAsInteracted(uniqueObjectID);
                Debug.Log($"[IconTrigger] 已記錄互動:{uniqueObjectID}");
            }

            // 6. 標記已互動(本地記錄)
            hasInteracted = true;

            // 7. 物件消失(如果有設定)
            if (disappearAfterInteraction)
            {
                gameObject.SetActive(false);
                Debug.Log($"[IconTrigger] 物件已隱藏:{gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"[IconTrigger] 圖示已存在:{iconToGive.displayName}");
        }
    }

    
    /// 移除指定的舊圖示
    
    private void RemoveOldIcons()
    {
        if (iconsToRemoveOnSuccess == null || iconsToRemoveOnSuccess.Count == 0)
            return;

        foreach (var iconID in iconsToRemoveOnSuccess)
        {
            if (string.IsNullOrEmpty(iconID))
                continue;

            bool removed = IconManager.Instance.RemoveIconByID(iconID);
            if (removed)
            {
                Debug.Log($"[IconTrigger] 已移除舊圖示:{iconID}");
            }
            else
            {
                Debug.LogWarning($"[IconTrigger] 無法移除圖示(可能不存在):{iconID}");
            }
        }
    }

    
    /// 顯示失敗訊息
    
    private void ShowFailureMessage()
    {
        if (string.IsNullOrEmpty(failureMessage))
            return;

        Debug.Log($"[IconTrigger] 提示:{failureMessage}");

        // TODO: 如果有 UI 訊息系統，可以這樣呼叫:
        // MessageUI.Instance?.ShowMessage(failureMessage);
    }

    
    /// 播放音效
    
    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(clip.name);
        }
        else
        {
            // 備用方案:直接播放
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    
    /// 在 Inspector 中顯示警告
    
    private void OnValidate()
    {
        // 如果沒有設定 ID，自動生成
        if (string.IsNullOrEmpty(uniqueObjectID))
        {
            string sceneName = gameObject.scene.name;

            // 如果場景名稱為空(例如在 Prefab 模式)，使用預設值
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = "Prefab";
            }

            uniqueObjectID = $"{sceneName}_{gameObject.name}";
            Debug.Log($"[IconTrigger] 自動生成ID:{uniqueObjectID}");

#if UNITY_EDITOR
            // 標記物件為「已修改」，讓 Unity 知道要儲存變更
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // 檢查 ID 是否有效
        if (string.IsNullOrEmpty(uniqueObjectID))
        {
            Debug.LogError($"[IconTrigger] {gameObject.name}: uniqueObjectID 不能為空！");
        }

        // 檢查基本設定
        if (iconToGive == null)
        {
            Debug.LogWarning($"[IconTrigger] {gameObject.name}: 請設定 iconToGive！");
        }

        if (requiresSpecificIcon && string.IsNullOrEmpty(requiredIconID))
        {
            Debug.LogWarning($"[IconTrigger] {gameObject.name}: 已啟用條件互動，但 requiredIconID 是空的！");
        }
    }
}
