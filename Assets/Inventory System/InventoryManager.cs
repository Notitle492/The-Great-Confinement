using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<Item> items = new List<Item>();
    public int maxSlots = 8;
    public InventoryUI inventoryUI;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChangedCallback;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool AddItem(Item item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿");
            return false;
        }

        items.Add(item);
        inventoryUI?.UpdateUI(items); // ✅ 告訴 UI 更新畫面
        /* onInventoryChangedCallback?.Invoke(); */
        return true;
    }

    
    /// ✅ 新增：根據 ItemID 移除背包中的物品
    public bool RemoveItemByID(int itemID)
    {
        // 尋找符合 ItemID 的物品
        Item itemToRemove = items.Find(i => i.ItemID == itemID);

        if (itemToRemove != null)
        {
            items.Remove(itemToRemove);
            inventoryUI?.UpdateUI(items);
            Debug.Log($"[InventoryManager] 已從背包移除物品：{itemToRemove.ItemName} (ID: {itemID})");
            return true;
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] 找不到 ItemID={itemID} 的物品");
            return false;
        }
    }
}
