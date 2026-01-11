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
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Found duplicate InventoryManager. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); //跨場景保留
        Debug.Log("[InventoryManager] 已設定為跨場景保留");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool AddItem(Item item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿");
            return false;
        }

        items.Add(item);
        //更新 UI 前先檢查是否存在
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI(items);
            Debug.Log($"[InventoryManager] 已更新 UI，當前物品數：{items.Count}");
        }
        else
        {
            Debug.LogWarning("[InventoryManager] inventoryUI 是 null，無法更新 UI");
        }

        return true;
    }

    /// 重新綁定 UI（場景切換後呼叫）
    public void RebindUI(InventoryUI ui)
    {
        inventoryUI = ui;
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI(items);
            Debug.Log($"[InventoryManager] UI 已重新綁定，當前物品數：{items.Count}");
        }
    }



    /// 根據 ItemID 移除背包中的物品
    public bool RemoveItemByID(int itemID)
    {
        // 尋找符合 ItemID 的物品
        Item itemToRemove = items.Find(i => i.ItemID == itemID);

        if (itemToRemove != null)
        {
            items.Remove(itemToRemove);

            if (inventoryUI != null)
            {
                inventoryUI.UpdateUI(items);
            }

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
