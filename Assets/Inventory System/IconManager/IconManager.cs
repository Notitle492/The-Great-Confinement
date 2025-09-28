using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    [Header("合成區")]
    public Transform synthesisContainer; // 放合成區圖示的父物件,合成區的 UI 容器（拖進 Canvas 中的底部區域）
    public GameObject synthesisPrefab;   // 合成區圖示用的預製,複製到合成區的圖示 Prefab
    public Sprite defaultSlotSprite;     //（同樣用於合成區 placeholder 清空時的還原）
    
    // synthesisSlots：合成區狀態表，用圖示id當key，值是對應的合成區圖示GameObject
    private Dictionary<string, GameObject> synthesisSlots = new Dictionary<string, GameObject>();
    
    // 如果是動態 Instantiate 的合成 slot，需要記錄以便刪除
    private readonly List<GameObject> dynamicSynthesisSlots = new List<GameObject>();

    public enum IconType { Dialogue, Object }

    public static IconManager Instance { get; private set; }

    private readonly List<IconData> unlockedIcons = new List<IconData>();

    [Header("UI 插槽容器 (在 PuzzleUI 場景綁定)")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    private readonly List<GameObject> dynamicSpawnedSlots = new List<GameObject>();

    [Header("PuzzleUI 主物件")]
    [SerializeField] private GameObject puzzleUI;



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

    // 嘗試找到 synthesisContainer 底下第一個尚未被使用、且帶有 IconSlot 的 placeholder
    private GameObject FindEmptySynthesisPlaceholder()
    {
        if (synthesisContainer == null) return null;

        for (int i = 0; i < synthesisContainer.childCount; i++)
        {
            var child = synthesisContainer.GetChild(i).gameObject;
            var slot = child.GetComponent<IconSlot>();
            if (slot != null && !slot.HasIcon())
            return child;
        }
        return null;
    }

    // 點擊顯示區圖示 → 切換合成區
    public void ToggleSynthesis(IconData data)
    {
        if (data == null)
        {
            Debug.LogWarning("ToggleSynthesis 收到 null data");
            return;
        }

        Debug.Log($"[ToggleSynthesis] 嘗試處理 {data.id}");

        // 已在合成區 → 移除
        if (synthesisSlots.ContainsKey(data.id))
        {
            Debug.Log($"[ToggleSynthesis] {data.id} 已存在於合成區，準備移除");
            RemoveFromSynthesis(data);
            return;
        }

        // 嘗試找空的 placeholder
        IconSlot emptySlot = null;
        if (synthesisContainer != null)
        {
            foreach (Transform child in synthesisContainer)
            {
                IconSlot slot = child.GetComponent<IconSlot>();
                if (slot != null && !slot.HasIcon())
                {
                    emptySlot = slot;
                    break;
                }
            }
        }

        if (emptySlot != null)
        {
            emptySlot.Setup(data);
            emptySlot.isSynthesisSlot = true;
            synthesisSlots[data.id] = emptySlot.gameObject;
            return;
        }

        // 如果沒有空 Slot，就 Instantiate
        if (synthesisPrefab != null && synthesisContainer != null)
        {
            GameObject go = Instantiate(synthesisPrefab, synthesisContainer);
            IconSlot slot = go.GetComponent<IconSlot>();
            if (slot != null)
            {
                slot.Setup(data);
                slot.isSynthesisSlot = true;
            }
            synthesisSlots[data.id] = go;
            dynamicSynthesisSlots.Add(go);
        }
        else
        {
            Debug.LogWarning("IconManager.ToggleSynthesis: synthesisContainer or synthesisPrefab not assigned.");
        }
    }

    // 移除合成區圖示（會根據是否為動態產生選擇 Destroy 或 Clear placeholder）
    public void RemoveFromSynthesis(IconData data)
    {
        if (data == null) return;
        if (!synthesisSlots.ContainsKey(data.id)) return;

        GameObject go = synthesisSlots[data.id];
        var slot = go.GetComponent<IconSlot>();

        if (dynamicSynthesisSlots.Contains(go))
        {
            dynamicSynthesisSlots.Remove(go);
            Destroy(go);
            Debug.Log($"Destroyed dynamic synthesis slot for {data.id}");
        }
        else
        {
            // placeholder: 清空內容而不是 Destroy
            if (slot != null)
            {
                slot.Clear(defaultSlotSprite); // 用預設圖還原或清空
            }
            Debug.Log($"Cleared placeholder synthesis slot for {data.id}");
        }

        synthesisSlots.Remove(data.id);

        /* if (synthesisSlots.ContainsKey(data.id))
        {
            Destroy(synthesisSlots[data.id]);
            synthesisSlots.Remove(data.id);
        } */
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
        ClearDynamicSlots(); // 只清動態 slot

        if (slotContainer == null) return;

        foreach (var icon in unlockedIcons)
        {
            // 如果 slot 已經存在（HasIcon=true），就跳過
            bool alreadyExists = false;
            for (int i = 0; i < slotContainer.childCount; i++)
            {
                var slot = slotContainer.GetChild(i).GetComponent<IconSlot>();
                if (slot != null && slot.HasIcon() && slot.IconData.id == icon.id)
                {
                    alreadyExists = true;
                    break;
                }
            }
            if (!alreadyExists)
                SpawnSlot(icon);
        }
    }

    private void ClearDynamicSlots()
    {
        // 刪除動態產生的顯示區 slot
        foreach (var go in dynamicSpawnedSlots)
            if (go) Destroy(go);
        dynamicSpawnedSlots.Clear();

        // 刪除動態產生的合成區 slot
        foreach (var go in dynamicSynthesisSlots)
            if (go) Destroy(go);
        dynamicSynthesisSlots.Clear();
    }


    private void ClearSynthesisPlaceholders()
    {
        foreach (Transform child in synthesisContainer)
        {
            var slot = child.GetComponent<IconSlot>();
            if (slot != null && !dynamicSynthesisSlots.Contains(slot.gameObject))
            {
                // 只有 placeholder 才清空，如果玩家已放入圖示的 slot 保留
                if (!slot.HasIcon()) 
                    slot.Clear(null); // 空白
            }
        }
    }

    public void TogglePuzzleUI()
    {
        puzzleUI.SetActive(!puzzleUI.activeSelf);
        
        if (puzzleUI.activeSelf)
            RebuildUI(); // 只新增尚未生成的 slot
        // 不要再呼叫 ClearUI() → 保留玩家圖示
    }



    private void SpawnSlot(IconData data)
    {
        int assignedIndex = Mathf.Clamp(unlockedIcons.Count - 1, 0, TooltipManager.Instance.tooltipObjects.Count - 1);

        // 先嘗試把圖示放到尚未被佔用的預設 child placeholder
        
        if (slotContainer != null)
        {
            for (int i = 0; i < slotContainer.childCount; i++)
            {
                var child = slotContainer.GetChild(i).gameObject;
                var slotScript = child.GetComponent<IconSlot>();
                if (slotScript != null && slotScript.HasIcon())
                    continue; // 已有圖示，跳過

                // 找到空的 slot
                if (slotScript != null)
                {
                    slotScript.isSynthesisSlot = false;  // 顯示區
                    slotScript.Setup(data);
                    Debug.Log($"SpawnSlot: {data.id} 已呼叫 Setup，TooltipIndex={assignedIndex}");
                }
                else
                {
                    // 只是 Image 的 placeholder
                    Image img = child.GetComponent<Image>();
                    if (img == null) img = child.GetComponentInChildren<Image>();
                    if (img != null) img.sprite = data.iconSprite;
                }

                return;
            }
        }


        // 若沒有可用的 placeholder，就 Instantiate 一個預製
        if (slotPrefab != null && slotContainer != null)
        {
            var go = Instantiate(slotPrefab, slotContainer);
            Image img = go.GetComponent<Image>();
            if (img == null) img = go.GetComponentInChildren<Image>();
            if (img != null) img.sprite = data.iconSprite;

            var slotScript = go.GetComponent<IconSlot>();
            if (slotScript != null)
            {
                slotScript.isSynthesisSlot = false; // ✅ 顯示區
                slotScript.Setup(data); // ✅ 只傳一個 IconData

                // ✅ 如果 slot 有 Button，綁定點擊事件
                Button btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => slotScript.OnPointerClick(null));
                }

            }
            
            dynamicSpawnedSlots.Add(go);
        }
        else
        {
            Debug.LogWarning("IconManager: 無 slotPrefab 或 slotContainer，無法在 UI 上顯示新圖示。");
        }
    }


    // ClearUI 也要清空合成區（保留 placeholder 結構，但清除內容）
    private void ClearUI()
    {
        // 刪除動態產生的物件
        foreach (var go in dynamicSpawnedSlots)
            if (go) Destroy(go);
        dynamicSpawnedSlots.Clear();

        // 刪除動態產生的合成區 slot
        foreach (var go in dynamicSynthesisSlots)
            if (go) Destroy(go);
        dynamicSynthesisSlots.Clear();
       
    }

    public IReadOnlyList<IconData> GetUnlockedIcons() => unlockedIcons;

    public void ClearAllIcons()
    {
        unlockedIcons.Clear(); // 把已解鎖的圖示清空
        ClearUI();             // 把 UI 上的圖示清空（用你已經寫好的 ClearUI 方法）
    }


}
