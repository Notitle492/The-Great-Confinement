using UnityEngine;
using System.Collections.Generic;


/// 專門處理圖示互動系統（無對話）
/// 支援：直接給圖示、條件給圖示、自動消除圖示
/// 支援同步到 InventoryUI 和 PuzzleUI


public class IconTrigger : MonoBehaviour
{
    [Header("=== 基本設定 ===")]
    [Tooltip("互動後要給予的圖示")]
    public IconDataSO iconToGive;

    [Tooltip("對應的背包物品（可選，如果要同步到背包的話）")]
    public Item itemToGive;

    [Tooltip("是否只能互動一次")]
    public bool oneTimeInteraction = true;

    [Tooltip("互動後物件是否消失")]
    public bool disappearAfterInteraction = false;

    [Header("=== 條件互動 ===")]
    [Tooltip("是否需要擁有特定圖示才能互動")]
    public bool requiresSpecificIcon = false;

    [Tooltip("需要擁有的圖示ID（例如：7）")]
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

    /// <summary>
    /// 供 ObjectTrigger 呼叫的互動方法
    /// </summary>
    public void Interact()
    {
        Debug.Log($"[IconTrigger] {gameObject.name} Interact() 被呼叫");

        // 檢查是否已經互動過
        if (oneTimeInteraction && hasInteracted)
        {
            Debug.Log($"[IconTrigger] {gameObject.name} 已經互動過，跳過");
            return;
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

    /// <summary>
    /// 檢查玩家是否擁有所需圖示
    /// </summary>
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
                Debug.Log($"[IconTrigger] 玩家擁有所需圖示：{requiredIconID}");
                return true;
            }
        }

        Debug.Log($"[IconTrigger] 玩家未擁有所需圖示：{requiredIconID}");
        return false;
    }

    /// <summary>
    /// 執行互動邏輯
    /// </summary>
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

        //  1. 給予新圖示到 PuzzleUI（解謎介面）
        IconData newIcon = iconToGive.ToIconData();

        //  如果有關聯背包物品，設定 linkedInventoryItemID
        if (itemToGive != null)
        {
            newIcon.linkedInventoryItemID = itemToGive.ItemID;
            Debug.Log($"[IconTrigger] 圖示 {iconToGive.id} 關聯背包物品 ID: {itemToGive.ItemID}");
        }

        bool iconAdded = IconManager.Instance.AddIcon(newIcon);

        if (iconAdded)
        {
            Debug.Log($"[IconTrigger] 成功給予圖示到解謎介面：{iconToGive.displayName} (ID: {iconToGive.id})");

            //  2. 同步加入到背包 InventoryUI（如果有設定 itemToGive）
            if (itemToGive != null)
            {
                if (InventoryManager.Instance != null)
                {
                    bool itemAdded = InventoryManager.Instance.AddItem(itemToGive);
                    if (itemAdded)
                    {
                        Debug.Log($"[IconTrigger] 成功加入背包：{itemToGive.ItemName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[IconTrigger] 背包已滿，無法加入：{itemToGive.ItemName}");
                    }
                }
                else
                {
                    Debug.LogWarning("[IconTrigger] InventoryManager.Instance 是 null，無法同步到背包");
                }
            }

            //  3. 移除舊圖示（如果有設定）
            RemoveOldIcons();

            //  4. 播放成功音效
            PlaySound(successSound);

            // 額外播放 Pickup 音效（與 ItemTriggerStatic 保持一致）
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D("Pickup");
            }

            //  5. 標記已互動
            hasInteracted = true;

            //  6. 物件消失（如果有設定）
            if (disappearAfterInteraction)
            {
                gameObject.SetActive(false);
                Debug.Log($"[IconTrigger] 物件已隱藏：{gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"[IconTrigger] 圖示已存在：{iconToGive.displayName}");
        }
    }

    /// <summary>
    /// 移除指定的舊圖示
    /// </summary>
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
                Debug.Log($"[IconTrigger] 已移除舊圖示：{iconID}");
            }
            else
            {
                Debug.LogWarning($"[IconTrigger] 無法移除圖示（可能不存在）：{iconID}");
            }
        }
    }

    /// <summary>
    /// 顯示失敗訊息
    /// </summary>
    private void ShowFailureMessage()
    {
        if (string.IsNullOrEmpty(failureMessage))
            return;

        Debug.Log($"[IconTrigger] 提示：{failureMessage}");

        // TODO: 如果有 UI 訊息系統，可以這樣呼叫：
        // MessageUI.Instance?.ShowMessage(failureMessage);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
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
            // 備用方案：直接播放
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    /// <summary>
    /// 在 Inspector 中顯示警告
    /// </summary>
    private void OnValidate()
    {
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
