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
    

    [Header("合成區設定")]
    [Tooltip("合成區可用的槽位數量（不含結果槽）")]
    public int maxSynthesisSlots = 2; // ✅ 前兩格可用


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

    // 儲存合成區已放入的圖示ID（或 IconData）
    private readonly List<IconData> synthesisHistory = new List<IconData>();

    public IReadOnlyList<IconData> GetSynthesisHistory() => synthesisHistory;

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

    // ToggleSynthesis 方法
    public void ToggleSynthesis(IconData data)
    {
        if (data == null)
        {
            Debug.LogWarning("ToggleSynthesis 收到 null data");
            return;
        }

        Debug.Log($"[ToggleSynthesis] 處理 {data.id}");


        // ✅ 檢查合成區是否已經有該圖示
        if (synthesisSlots.ContainsKey(data.id))
        {
            // 已存在 → 移除（回到顯示區）
            Debug.Log($"[ToggleSynthesis] {data.id} 已在合成區，執行移除");
            RemoveFromSynthesis(data);
        
            // 從歷史紀錄中移除
            synthesisHistory.Remove(data);
        }
        else
        {
            // 不存在 → 檢查是否還有空位
            int currentCount = GetUsableSynthesisSlotCount();
            
            if (currentCount >= maxSynthesisSlots)
            {
                Debug.LogWarning($"[ToggleSynthesis] 合成區已滿 ({currentCount}/{maxSynthesisSlots})，無法新增 {data.id}");
                // 可以在這裡播放音效或顯示提示
                return;
            }
            
            Debug.Log($"[ToggleSynthesis] {data.id} 不在合成區 → 新增 (目前 {currentCount}/{maxSynthesisSlots})");
            AddSingleToSynthesis(data);
        }
        
    }

    // ✅ 新增方法：計算目前已使用的合成槽位數量
    private int GetUsableSynthesisSlotCount()
    {
        int count = 0;
        if (synthesisContainer != null)
        {
            for (int i = 0; i < Mathf.Min(maxSynthesisSlots, synthesisContainer.childCount); i++)
            {
                IconSlot slot = synthesisContainer.GetChild(i).GetComponent<IconSlot>();
                if (slot != null && slot.HasIcon())
                {
                    count++;
                }
            }
        }
        return count;
    }

    // ✅ 優化後的 AddSingleToSynthesis
    private void AddSingleToSynthesis(IconData data)
    {
        if (data == null) return;

        // 尋找空的 placeholder（只在前 maxSynthesisSlots 格中尋找）
        IconSlot emptySlot = null;
        if (synthesisContainer != null)
        {
            int searchLimit = Mathf.Min(maxSynthesisSlots, synthesisContainer.childCount);
            
            for (int i = 0; i < searchLimit; i++)
            {
                IconSlot slot = synthesisContainer.GetChild(i).GetComponent<IconSlot>();
                if (slot != null && !slot.HasIcon())
                {
                    emptySlot = slot;
                    Debug.Log($"[AddSingleToSynthesis] 找到空槽位 #{i}");
                    break;
                }
            }
        }

        GameObject go;
        if (emptySlot != null)
        {
            // 使用現有的 placeholder
            go = emptySlot.gameObject;
            emptySlot.Setup(data);
            emptySlot.isSynthesisSlot = true;
            synthesisSlots[data.id] = go;
            Debug.Log($"[AddSingleToSynthesis] {data.id} 已加入合成區（使用 placeholder）");
        }
        else
        {
            // 檢查是否超過限制
            if (GetUsableSynthesisSlotCount() >= maxSynthesisSlots)
            {
                Debug.LogWarning($"[AddSingleToSynthesis] 無法新增 {data.id}：合成區已滿");
                return;
            }
            
            // 動態生成（在前 maxSynthesisSlots 個位置）
            if (synthesisPrefab != null && synthesisContainer != null)
            {
                go = Instantiate(synthesisPrefab, synthesisContainer);
                
                // 確保新生成的物件在正確位置（不是最後）
                go.transform.SetSiblingIndex(Mathf.Min(maxSynthesisSlots - 1, synthesisContainer.childCount - 2));
                
                IconSlot slot = go.GetComponent<IconSlot>();
                if (slot != null)
                {
                    slot.Setup(data);
                    slot.isSynthesisSlot = true;
                }
                synthesisSlots[data.id] = go;
                dynamicSynthesisSlots.Add(go);
                Debug.Log($"[AddSingleToSynthesis] {data.id} 已動態生成到合成區");
            }
            else
            {
                Debug.LogWarning("[AddSingleToSynthesis] synthesisContainer 或 synthesisPrefab 未指定");
                return;
            }
        }

        if (!synthesisHistory.Contains(data))
        {
            synthesisHistory.Add(data);
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
    public void BindUI(Transform container, GameObject prefab, Transform synthesis, GameObject puzzleUI)
    {
        slotContainer = container;
        slotPrefab = prefab;
        synthesisContainer = synthesis;
        this.puzzleUI = puzzleUI;

        // ✅ 初始化時標記最後一格為結果槽
        MarkResultSlot();

        RebuildUI();
    }

    // ✅ 新增方法：標記結果槽位
    private void MarkResultSlot()
    {
        if (synthesisContainer == null) return;
        
        int lastIndex = synthesisContainer.childCount - 1;
        if (lastIndex >= 0)
        {
            var resultSlot = synthesisContainer.GetChild(lastIndex).GetComponent<IconSlot>();
            if (resultSlot != null)
            {
                resultSlot.isSynthesisSlot = true; // 標記為合成區
                // 可以加個特殊標記
                resultSlot.gameObject.name = "ResultSlot";
                Debug.Log($"[MarkResultSlot] 已標記第 {lastIndex} 格為結果槽");
            }
        }
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

        // 重建顯示區圖示
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

        // 重建合成區 UI：根據歷史紀錄
        foreach (var data in synthesisHistory)
        {
            if (!synthesisSlots.ContainsKey(data.id))
            {
                AddSingleToSynthesis(data);
            }
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

    public void TogglePuzzleUI()
    {
        if (puzzleUI == null) return;
        
        puzzleUI.SetActive(!puzzleUI.activeSelf);
        
        if (puzzleUI.activeSelf)
        {
            RebuildUI();
            // 不要再清空合成區，保留歷史紀錄
        }
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