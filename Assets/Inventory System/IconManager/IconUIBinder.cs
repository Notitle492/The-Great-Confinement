using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconUIBinder : MonoBehaviour
{
    [Header("PuzzleUI 場景內的容器與 Slot Prefab")]
    public Transform slotContainer;
    public GameObject slotPrefab;

    // ✅ 新增：保留原始參考，避免被 UnbindUI 清除後無法恢復
    private Transform cachedSlotContainer;
    private GameObject cachedSlotPrefab;


    private void Start()
    {
        Debug.Log("[IconUIBinder] Start 開始");

        // ✅ 快取參考
        cachedSlotContainer = slotContainer;
        cachedSlotPrefab = slotPrefab;


        // 確認容器和 prefab 都有設定
        if (slotContainer == null)
        {
            Debug.LogError("IconUIBinder: slotContainer 未設定！請在 Inspector 中拖入容器。");
            return;
        }
        
        if (slotPrefab == null)
        {
            Debug.LogError("IconUIBinder: slotPrefab 未設定！請在 Inspector 中拖入 Prefab。");
            return;
        }

        // 若 IconManager 存在，綁定 UI
        if (IconManager.Instance != null)
        {
            Debug.Log($"[IconUIBinder] 呼叫 BindUI - Container: {slotContainer.name}, Prefab: {slotPrefab.name}");
            IconManager.Instance.BindUI(slotContainer, slotPrefab);
        }
        else
        {
            Debug.LogWarning("IconUIBinder: 找不到 IconManager。請手動在起始場景放置 IconManager。");
        }
    }

    // ✅ 修改：OnDestroy 改為只在真正銷毀時才解綁
    private void OnDestroy()
    {
        Debug.Log("[IconUIBinder] OnDestroy - 檢查是否需要解除綁定");

        // 只有在場景真正被銷毀時才解綁（不是單純 SetActive(false)）
        if (IconManager.Instance != null && gameObject.scene.isLoaded)
        {
            Debug.Log("[IconUIBinder] 場景銷毀，解除 UI 綁定");
            IconManager.Instance.UnbindUI();
        }
    }

    // ✅ 修改：OnDisable 時不解綁，只記錄狀態
    private void OnDisable()
    {
        Debug.Log("[IconUIBinder] OnDisable - UI 被停用");
    }

    // ✅ 修改：OnEnable 時使用快取的參考重新綁定
    private void OnEnable()
    {
        Debug.Log("[IconUIBinder] OnEnable");
        
        // 如果不是第一次啟用（Start 已經執行過），重新綁定
        if (IconManager.Instance != null && cachedSlotContainer != null && cachedSlotPrefab != null)
        {
            // 等待一幀，確保 UI 已經完全啟用
            StartCoroutine(RebindAfterFrame());
        }
    }

    private IEnumerator RebindAfterFrame()
    {
        yield return null; // 等待一幀
        
        if (IconManager.Instance != null && cachedSlotContainer != null && cachedSlotPrefab != null)
        {
            Debug.Log($"[IconUIBinder] OnEnable 後重新綁定 UI - Container: {cachedSlotContainer.name}, Prefab: {cachedSlotPrefab.name}");
            IconManager.Instance.BindUI(cachedSlotContainer, cachedSlotPrefab);
        }
    }
}
