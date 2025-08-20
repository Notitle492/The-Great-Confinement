using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    public static IconManager Instance { get; private set; }

    private readonly List<IconData> unlockedIcons = new List<IconData>();

    [Header("UI 插槽容器 (在 PuzzleUI 場景綁定)")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    private readonly List<GameObject> spawnedSlots = new List<GameObject>();

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
        Debug.Log($"解鎖新圖示: {newIcon.id}");

        // 若 UI 已綁定，立即生成一格
        if (slotContainer != null && slotPrefab != null)
            SpawnSlot(newIcon);

        return true;
    }

    /// <summary>當進到 PuzzleUI 場景時，把已解鎖的圖示全部重建到 UI</summary>
    public void RebuildUI()
    {
        ClearUI();
        if (slotContainer == null || slotPrefab == null) return;

        foreach (var icon in unlockedIcons)
            SpawnSlot(icon);
    }

    private void SpawnSlot(IconData data)
    {
        var go = Instantiate(slotPrefab, slotContainer);
        var img = go.GetComponentInChildren<Image>();
        if (img != null) img.sprite = data.iconSprite;
        spawnedSlots.Add(go);
    }

    private void ClearUI()
    {
        foreach (var go in spawnedSlots)
            if (go) Destroy(go);
        spawnedSlots.Clear();
    }

    public IReadOnlyList<IconData> GetUnlockedIcons() => unlockedIcons;
}
