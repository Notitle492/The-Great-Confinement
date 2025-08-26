using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    public enum IconType { Dialogue, Object }

    public static IconManager Instance { get; private set; }

    private readonly List<IconData> unlockedIcons = new List<IconData>();

    [Header("UI 插槽容器 (在 PuzzleUI 場景綁定)")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("如果你有預設的空圖示 (placeholder)，可以指定，Clear UI 時會還原)")]
    [SerializeField] private Sprite defaultSlotSprite;

    // 兩個列表分開管理：動態產生的與佔用的(預先存在的 placeholder)
    private readonly List<GameObject> dynamicSpawnedSlots = new List<GameObject>();
    private readonly List<GameObject> assignedPlaceholders = new List<GameObject>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 跨場景保存
    }

    /// <summary>在 PuzzleUI 場景呼叫，指定要把圖示生成到哪個容器、用什麼預製</summary>
    public void BindUI(Transform container, GameObject prefab)
    {
        slotContainer = container;
        slotPrefab = prefab;
        RebuildUI();
    }

    /// <summary>離開 PuzzleUI 時可呼叫（可選）</summary>
    public void UnbindUI()
    {
        ClearUI();
        slotContainer = null;
        slotPrefab = null;
    }

    /// <summary>解鎖一個新圖示（若已存在則忽略）</summary>
    public bool AddIcon(IconData newIcon)
    {
        if (newIcon == null || newIcon.iconSprite == null || string.IsNullOrEmpty(newIcon.id))
        {
            Debug.LogWarning("IconManager.AddIcon: 傳入資料不完整");
            return false;
        }

        if (unlockedIcons.Exists(i => i.id == newIcon.id))
            return false; // 已經有了

        unlockedIcons.Add(newIcon);
        Debug.Log($"IconManager: 解鎖新圖示 {newIcon.id}");

        // 若 UI 已綁定，立即生成一格
        if (slotContainer != null && slotPrefab != null)
            SpawnSlot(newIcon);

        return true;
    }

    /// <summary>當進到 PuzzleUI 場景時，把已解鎖的圖示全部重建到 UI</summary>
    public void RebuildUI()
    {
        ClearUI();
        if (slotContainer == null) return;

        foreach (var icon in unlockedIcons)
            SpawnSlot(icon);
    }

    private void SpawnSlot(IconData data)
    {
        // 先嘗試把圖示放到尚未被佔用的預設 child placeholder
        if (slotContainer != null)
        {
            for (int i = 0; i < slotContainer.childCount; i++)
            {
                var child = slotContainer.GetChild(i).gameObject;
                if (assignedPlaceholders.Contains(child)) continue; // 已被用過

                // 只要這 child 有 Image，就當作可用槽位
                Image img = child.GetComponent<Image>();
                if (img == null) img = child.GetComponentInChildren<Image>();

                if (img != null)
                {
                    img.sprite = data.iconSprite;
                    assignedPlaceholders.Add(child);
                    return;
                }
            }
        }

        // 若沒有可用的 placeholder，就 Instantiate 一個預製
        if (slotPrefab != null && slotContainer != null)
        {
            var go = Instantiate(slotPrefab, slotContainer);
            Image img = go.GetComponent<Image>();
            if (img == null) img = go.GetComponentInChildren<Image>();
            if (img != null) img.sprite = data.iconSprite;
            dynamicSpawnedSlots.Add(go);
        }
        else
        {
            Debug.LogWarning("IconManager: 無 slotPrefab 或 slotContainer，無法在 UI 上顯示新圖示。");
        }
    }

    private void ClearUI()
    {
        // 刪除動態產生的物件
        foreach (var go in dynamicSpawnedSlots)
            if (go) Destroy(go);
        dynamicSpawnedSlots.Clear();

        // 重置已被佔用的 placeholder（如果你提供 defaultSlotSprite 就還原，否則留著）
        foreach (var child in assignedPlaceholders)
        {
            if (child == null) continue;
            Image img = child.GetComponent<Image>();
            if (img == null) img = child.GetComponentInChildren<Image>();
            if (img != null)
            {
                if (defaultSlotSprite != null) img.sprite = defaultSlotSprite;
                else img.sprite = null;
            }
        }
        assignedPlaceholders.Clear();
    }

    public IReadOnlyList<IconData> GetUnlockedIcons() => unlockedIcons;
}
