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
}
