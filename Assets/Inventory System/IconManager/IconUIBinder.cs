using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconUIBinder : MonoBehaviour
{
    [Header("PuzzleUI 場景內的容器與 Slot Prefab")]
    public Transform slotContainer;
    public GameObject slotPrefab;

    private void Start()
    {
        Debug.Log("[IconUIBinder] Start 開始");

        // 若 IconManager 尚未存在，先建立一個（方便測試 / 不需手動把 IconManager 放在起始場景）
        if (IconManager.Instance == null)
        {
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
            
            Debug.Log($"[IconUIBinder] 呼叫 BindUI - Container: {slotContainer.name}, Prefab: {slotPrefab.name}");
            IconManager.Instance.BindUI(slotContainer, slotPrefab);
        }
        else
        {
            Debug.LogWarning("IconUIBinder: 找不到 IconManager。請手動在起始場景放置 IconManager。");
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[IconUIBinder] OnDestroy - 解除 UI 綁定");

        if (IconManager.Instance != null)
        {
            IconManager.Instance.UnbindUI();
        }
    }

    // 新增：當物件被啟用時重新綁定（用於處理 SetActive 的情況）
    private void OnEnable()
    {
        Debug.Log("[IconUIBinder] OnEnable");
        
        // 如果不是第一次啟用（Start 已經執行過），重新綁定
        if (IconManager.Instance != null && slotContainer != null && slotPrefab != null)
        {
            // 等待一幀，確保 UI 已經完全啟用
            StartCoroutine(RebindAfterFrame());
        }
    }

    private IEnumerator RebindAfterFrame()
    {
        yield return null; // 等待一幀
        
        if (IconManager.Instance != null && slotContainer != null && slotPrefab != null)
        {
            Debug.Log("[IconUIBinder] OnEnable 後重新綁定 UI");
            IconManager.Instance.BindUI(slotContainer, slotPrefab);
        }
    }
}
