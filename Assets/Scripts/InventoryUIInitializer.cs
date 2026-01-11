using UnityEngine;

/// 放在每個場景的 InventoryUI 物件上
/// 負責在場景載入時重新綁定 InventoryManager

public class InventoryUIInitializer : MonoBehaviour
{
    public InventoryUI inventoryUI;

    private void Start()
    {
        // 延遲一幀確保所有 Manager 都已初始化
        StartCoroutine(RebindInventoryUI());
    }

    private System.Collections.IEnumerator RebindInventoryUI()
    {
        yield return null; // 等待一幀

        if (inventoryUI == null)
        {
            inventoryUI = GetComponent<InventoryUI>();
        }

        if (InventoryManager.Instance != null && inventoryUI != null)
        {
            InventoryManager.Instance.RebindUI(inventoryUI);
            Debug.Log($"[InventoryUIInitializer] 已重新綁定 InventoryUI");
        }
        else
        {
            Debug.LogError($"[InventoryUIInitializer] 綁定失敗！InventoryManager={InventoryManager.Instance}, UI={inventoryUI}");
        }
    }
}
