using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 進入場景自動觸發系統（進階版）
/// 支援：偵測特定場景切換、第一次進入場景自動給圖示、播放對話
/// </summary>
public class SceneEntryTrigger : MonoBehaviour
{
    [Header("=== 場景識別 ===")]
    [Tooltip("場景唯一識別ID（自動生成）")]
    public string sceneEntryID;

    [Header("=== 觸發模式 ===")]
    [Tooltip("觸發模式：場景載入時自動觸發 或 偵測場景切換")]
    public TriggerMode triggerMode = TriggerMode.OnSceneLoad;

    [Header("=== 場景切換偵測設定 ===")]
    [Tooltip("要偵測的目標場景名稱（只在切換到這個場景時觸發）")]
    public string targetSceneName = "MensRoom";

    [Tooltip("前一個場景名稱（從哪個場景來才觸發，留空則不檢查）")]
    public string previousSceneName = "";

    [Header("=== 觸發設定 ===")]
    [Tooltip("是否只在第一次進入場景時觸發")]
    public bool onlyFirstTime = true;

    [Tooltip("進入場景後延遲多少秒觸發（給場景時間載入）")]
    [Range(0f, 5f)]
    public float delayTime = 1f;

    [Header("=== 圖示設定 ===")]
    [Tooltip("是否給予圖示")]
    public bool giveIcon = true;

    [Tooltip("要給予的圖示")]
    public IconDataSO iconToGive;

    [Tooltip("對應的背包物品（可選）")]
    public Item itemToGive;

    [Header("=== 對話設定 ===")]
    [Tooltip("是否播放對話")]
    public bool playDialogue = false;

    [Tooltip("對話的 Ink JSON 檔案")]
    public TextAsset inkJSON;

    [Tooltip("要播放的對話 Knot 名稱")]
    public string dialogueKnot = "Chapter1";

    [Header("=== 音效設定 ===")]
    [Tooltip("觸發時的音效")]
    public AudioClip triggerSound;

    public enum TriggerMode
    {
        OnSceneLoad,      // 場景載入時立即觸發（需要物件在目標場景中）
        OnSceneChange     // 偵測場景切換時觸發（物件需要 DontDestroyOnLoad）
    }

    private bool hasTriggered = false;
    private string lastSceneName = "";

    private void Awake()
    {
        if (triggerMode == TriggerMode.OnSceneChange)
        {
            // 場景切換模式：設定為跨場景保留
            DontDestroyOnLoad(gameObject);
            lastSceneName = SceneManager.GetActiveScene().name;

            // 註冊場景載入事件
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void Start()
    {
        if (triggerMode == TriggerMode.OnSceneLoad)
        {
            // 場景載入模式：立即檢查並觸發
            CheckAndTrigger();
        }
    }

    /// <summary>
    /// 場景載入回調（僅在 OnSceneChange 模式使用）
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        Debug.Log($"[SceneEntryTrigger] 場景載入：{currentScene}（上一個場景：{lastSceneName}）");

        // 檢查是否是目標場景
        if (currentScene == targetSceneName)
        {
            // 檢查前一個場景（如果有設定）
            if (!string.IsNullOrEmpty(previousSceneName))
            {
                if (lastSceneName != previousSceneName)
                {
                    Debug.Log($"[SceneEntryTrigger] 前一個場景不符（需要：{previousSceneName}，實際：{lastSceneName}），跳過觸發");
                    lastSceneName = currentScene;
                    return;
                }
            }

            // 延遲觸發
            StartCoroutine(DelayedTrigger());
        }

        lastSceneName = currentScene;
    }

    /// <summary>
    /// 檢查並觸發（OnSceneLoad 模式）
    /// </summary>
    private void CheckAndTrigger()
    {
        // 檢查是否已經觸發過
        if (onlyFirstTime && InteractionStateManager.Instance != null)
        {
            if (InteractionStateManager.Instance.HasInteracted(sceneEntryID))
            {
                hasTriggered = true;
                Debug.Log($"[SceneEntryTrigger] {sceneEntryID} 已經觸發過，跳過");
                return;
            }
        }

        // 延遲觸發
        StartCoroutine(DelayedTrigger());
    }

    /// <summary>
    /// 延遲觸發
    /// </summary>
    private IEnumerator DelayedTrigger()
    {
        yield return new WaitForSeconds(delayTime);

        if (!hasTriggered)
        {
            TriggerEntry();
        }
    }

    /// <summary>
    /// 執行進入場景觸發
    /// </summary>
    private void TriggerEntry()
    {
        // 再次檢查是否已觸發（防止重複）
        if (onlyFirstTime && InteractionStateManager.Instance != null)
        {
            if (InteractionStateManager.Instance.HasInteracted(sceneEntryID))
            {
                Debug.Log($"[SceneEntryTrigger] {sceneEntryID} 已經觸發過，跳過");
                return;
            }
        }

        Debug.Log($"[SceneEntryTrigger] {sceneEntryID} 觸發！");

        // 1. 給予圖示
        if (giveIcon && iconToGive != null)
        {
            GiveIconToPlayer();
        }

        // 2. 播放對話
        if (playDialogue && inkJSON != null)
        {
            PlayDialogue();
        }

        // 3. 播放音效
        if (triggerSound != null)
        {
            PlaySound(triggerSound);
        }

        // 4. 記錄已觸發（跨場景）
        if (onlyFirstTime && InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.MarkAsInteracted(sceneEntryID);
            Debug.Log($"[SceneEntryTrigger] {sceneEntryID} 已記錄為觸發過");
        }

        hasTriggered = true;
    }

    private void GiveIconToPlayer()
    {
        if (IconManager.Instance == null)
        {
            Debug.LogError("[SceneEntryTrigger] IconManager.Instance 是 null");
            return;
        }

        IconData newIcon = iconToGive.ToIconData();

        if (itemToGive != null)
        {
            newIcon.linkedInventoryItemID = itemToGive.ItemID;
        }

        bool iconAdded = IconManager.Instance.AddIcon(newIcon);

        if (iconAdded)
        {
            Debug.Log($"[SceneEntryTrigger] 成功給予圖示：{iconToGive.displayName} (ID: {iconToGive.id})");

            if (itemToGive != null && InventoryManager.Instance != null)
            {
                bool itemAdded = InventoryManager.Instance.AddItem(itemToGive);
                if (itemAdded)
                {
                    Debug.Log($"[SceneEntryTrigger] 成功加入背包：{itemToGive.ItemName}");
                }
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D("Pickup");
            }
        }
    }

    private void PlayDialogue()
    {
        if (DialogueManager.GetInstance() == null)
        {
            Debug.LogWarning("[SceneEntryTrigger] DialogueManager 不存在");
            return;
        }

        if (DialogueManager.GetInstance().dialogueIsPlaying)
        {
            Debug.LogWarning("[SceneEntryTrigger] 對話已經在播放中");
            return;
        }

        Debug.Log($"[SceneEntryTrigger] 播放對話：{dialogueKnot}");
        DialogueManager.GetInstance().StartDialogue(inkJSON, this.gameObject, dialogueKnot);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SceneEntryTrigger] Trigger Sound 是 null");
            return;
        }

        // 優先使用備用方案直接播放（最可靠）
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 1f);
        Debug.Log($"[SceneEntryTrigger] 播放音效：{clip.name}");

        // 如果有 SoundManager，也可以額外呼叫（可選）
        // if (SoundManager.Instance != null)
        // {
        //     SoundManager.Instance.PlaySound2D(clip.name);
        // }
    }

    private void OnDestroy()
    {
        if (triggerMode == TriggerMode.OnSceneChange)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(sceneEntryID))
        {
            string sceneName = gameObject.scene.name;
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = "Prefab";
            }

            sceneEntryID = $"{sceneName}_Entry";
            Debug.Log($"[SceneEntryTrigger] 自動生成ID：{sceneEntryID}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        if (giveIcon && iconToGive == null)
        {
            Debug.LogWarning($"[SceneEntryTrigger] {gameObject.name}: 已啟用給予圖示，但 iconToGive 是 null！");
        }

        if (playDialogue && inkJSON == null)
        {
            Debug.LogWarning($"[SceneEntryTrigger] {gameObject.name}: 已啟用播放對話，但 inkJSON 是 null！");
        }

        if (triggerMode == TriggerMode.OnSceneChange && string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"[SceneEntryTrigger] {gameObject.name}: 使用場景切換模式，但未設定 targetSceneName！");
        }
    }
}