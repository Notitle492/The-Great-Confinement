using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    private void Update()
    {
        // 測試用：按 T 鍵手動觸發第一個圖示的點擊
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (unlockedIcons.Count > 0)
            {
                Debug.Log($"[測試] 手動觸發 ToggleSynthesis: {unlockedIcons[0].id}");
                ToggleSynthesis(unlockedIcons[0]);
            }
        }
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

    // ToggleSynthesis 方法
    public void ToggleSynthesis(IconData data)
    {
        if (data == null)
        {
            Debug.LogWarning("ToggleSynthesis 收到 null data");
            return;
        }

        Debug.Log($"[ToggleSynthesis] 嘗試處理 {data.id}");

        // 改為：總是新增到合成區（不檢查是否已存在）
        AddToSynthesisDuplicate(data);
    }

    // 將方法獨立出來
    public void AddToSynthesisDuplicate(IconData data)
    {
        if (data == null) return;

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

        GameObject go;
        if (emptySlot != null)
        {
            go = emptySlot.gameObject;
            emptySlot.Setup(data);
            emptySlot.isSynthesisSlot = true;
            synthesisSlots[data.id] = go;
            Debug.Log($"[ToggleSynthesis] {data.id} 已加入合成區: {emptySlot.name}");
        }
        else
        {
            // 如果沒有空 Slot，就 Instantiate
            if (synthesisPrefab != null && synthesisContainer != null)
            {
                go = Instantiate(synthesisPrefab, synthesisContainer);
                IconSlot slot = go.GetComponent<IconSlot>();
                if (slot != null)
                {
                    slot.Setup(data);
                    slot.isSynthesisSlot = true;
                }
                synthesisSlots[data.id] = go;
                dynamicSynthesisSlots.Add(go);
                Debug.Log($"[ToggleSynthesis] {data.id} 已 Instantiate 到合成區");
            }
            else
            {
                Debug.LogWarning("IconManager.ToggleSynthesis: synthesisContainer or synthesisPrefab not assigned.");
                return;
            }
        }

        EnsureClickableSlot(go, go.GetComponent<IconSlot>());
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
    }

    /// <summary>在 PuzzleUI 場景呼叫，指定要把圖示生成到哪個容器、用什麼預製</summary>
    public void BindUI(Transform container, GameObject prefab)
    {
        if (container == null || prefab == null)
        {
            Debug.LogError("IconManager.BindUI: container 或 prefab 為 null！");
            return;
        }
        
        // ✅ 即使已經有參考，也強制更新（以防 missing）
        slotContainer = container;
        slotPrefab = prefab;
        Debug.Log($"[IconManager] BindUI 成功 - Container: {container.name}, Prefab: {prefab.name}");
        RebuildUI();
    }
        
    /// <summary>離開 PuzzleUI 時可呼叫（可選）</summary>
    public void UnbindUI()
    {
        Debug.Log("[IconManager] UnbindUI - 只清除動態 UI，不清空容器參考");
        ClearDynamicSlots(); // ✅ 改用這個，只清除動態生成的物件
        // ❌ 不要設 null！保留 slotContainer 和 slotPrefab 的參考
        // slotContainer = null;  // 絕對不要這樣做
        // slotPrefab = null;     // 絕對不要這樣做
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
        if (synthesisContainer == null) return;
        
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
        if (puzzleUI == null) return;
        
        puzzleUI.SetActive(!puzzleUI.activeSelf);
        
        if (puzzleUI.activeSelf)
            RebuildUI(); // 只新增尚未生成的 slot
        // 不要再呼叫 ClearUI() → 保留玩家圖示
    }

    private void SpawnSlot(IconData data)
    {
        // 先嘗試把圖示放到尚未被佔用的預設 child placeholder
        if (slotContainer != null)
        {
            for (int i = 0; i < slotContainer.childCount; i++)
            {
                var child = slotContainer.GetChild(i).gameObject;
                var slotScript = child.GetComponent<IconSlot>();
                if (slotScript != null && !slotScript.HasIcon())
                {
                    // 找到空的 slot
                    slotScript.isSynthesisSlot = false;  // 顯示區
                    slotScript.Setup(data);

                    // 確保可以被點擊
                    EnsureClickableSlot(child, slotScript);

                    Debug.Log($"SpawnSlot: {data.id} 已設置到 placeholder");
                    return;
                }
            }
        }

        // 若沒有可用的 placeholder，就 Instantiate 一個預製
        if (slotPrefab != null && slotContainer != null)
        {
            var go = Instantiate(slotPrefab, slotContainer);

            var slotScript = go.GetComponent<IconSlot>();
            if (slotScript != null)
            {
                slotScript.isSynthesisSlot = false; // 顯示區
                slotScript.Setup(data); // 只傳一個 IconData

                // 確保可以被點擊
                EnsureClickableSlot(go, slotScript);
                
                Debug.Log($"SpawnSlot: {data.id} 已 Instantiate 新的 slot");
            }
            
            dynamicSpawnedSlots.Add(go);
        }
        else
        {
            Debug.LogWarning("IconManager: 無 slotPrefab 或 slotContainer，無法在 UI 上顯示新圖示。");
        }
    }

    // 在 SpawnSlot 方法後面新增這個方法
    private void EnsureClickableSlot(GameObject slotObject, IconSlot slotScript)
    {
        if (slotObject == null || slotScript == null) return;

        // 方法 1: 確保 Image 可以接收射線
        Image img = slotObject.GetComponent<Image>();
        if (img == null) img = slotObject.GetComponentInChildren<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
            Debug.Log($"設置 {slotScript.IconData?.id} 的 Image.raycastTarget = true");
        }
        
        // 方法 2: 如果有 Button 組件，設置點擊事件
        Button btn = slotObject.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => 
            {
                Debug.Log($"[Button] 點擊 {slotScript.IconData?.id}");
                if (slotScript.IconData != null)
                {
                    ToggleSynthesis(slotScript.IconData);
                }
            });
        }
        
        // 方法 3: 添加 EventTrigger 作為備用方案
        var eventTrigger = slotObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = slotObject.AddComponent<EventTrigger>();
        }
        
        // 清除舊的事件
        eventTrigger.triggers.Clear();
        
        // 添加點擊事件
        var clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => 
        {
            Debug.Log($"[EventTrigger] 點擊 {slotScript.IconData?.id}");
            if (slotScript.IconData != null)
            {
                ToggleSynthesis(slotScript.IconData);
            }
        });
        eventTrigger.triggers.Add(clickEntry);
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
       
        // 清空合成區字典
        synthesisSlots.Clear();
    }

    public IReadOnlyList<IconData> GetUnlockedIcons() => unlockedIcons;

    public void ClearAllIcons()
    {
        unlockedIcons.Clear(); // 把已解鎖的圖示清空
        ClearUI();             // 把 UI 上的圖示清空（用你已經寫好的 ClearUI 方法）
    }
}