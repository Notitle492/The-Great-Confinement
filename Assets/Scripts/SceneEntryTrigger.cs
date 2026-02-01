using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 進入場景自動觸發系統
/// 
/// 觸發模式說明：
/// ─────────────────────────────────────────────
/// OnSceneLoad：物件放在「目標場景本身」裡，場景載入時自動觸發。
///   → 適用於：進入 MensRoom / MensRoom 2 時觸發對話、給圖示等。
///   → 這是你 MensRoom 應該用的模式。
/// 
/// OnSceneChange：物件必須放在「永久存活的物件」上（DontDestroyOnLoad），
///   由 SceneEntryTriggerManager 統一管理，偵測「從哪個場景來」才觸發。
///   → 適用於：需要檢查「前一個場景」才觸發的特殊情況。
///   → 如果你不需要檢查前一個場景，不要用這個模式。
/// ─────────────────────────────────────────────
/// </summary>
public class SceneEntryTrigger : MonoBehaviour
{
    [Header("=== 場景識別 ===")]
    [Tooltip("場景唯一識別ID（自動生成，每個物件必須唯一）")]
    public string sceneEntryID;

    [Header("=== 觸發模式 ===")]
    [Tooltip("觸發模式：見上方說明")]
    public TriggerMode triggerMode = TriggerMode.OnSceneLoad;

    [Header("=== 場景切換偵測設定（僅 OnSceneChange 模式使用）===")]
    [Tooltip("要偵測的目標場景名稱")]
    public string targetSceneName = "";

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
        OnSceneLoad,      // 場景載入時觸發（物件放在目標場景裡）
        OnSceneChange     // 偵測場景切換時觸發（物件放在永久存活物件上，由 Manager 管理）
    }

    private bool hasTriggered = false;

    private void Awake()
    {
        if (triggerMode == TriggerMode.OnSceneChange)
        {
            if (SceneEntryTriggerManager.Instance == null)
            {
                Debug.LogError($"[SceneEntryTrigger] [{sceneEntryID}] OnSceneChange 模式需要 SceneEntryTriggerManager！請確認 Manager 已經啟動。");
                return;
            }

            SceneEntryTriggerManager.Instance.RegisterTrigger(new TriggerRegistration
            {
                id = sceneEntryID,
                targetSceneName = targetSceneName,
                previousSceneName = previousSceneName,
                onlyFirstTime = onlyFirstTime,
                delayTime = delayTime,
                sourceObject = gameObject,
                onTrigger = TriggerEntry
            });

            Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] 已注冊到 Manager（OnSceneChange 模式）");
        }
    }

    private void Start()
    {
        if (triggerMode == TriggerMode.OnSceneLoad)
        {
            Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] Start() 呼叫，當前場景：{SceneManager.GetActiveScene().name}");
            CheckAndTrigger();
        }
    }

    private void CheckAndTrigger()
    {
        if (onlyFirstTime && InteractionStateManager.Instance != null)
        {
            if (InteractionStateManager.Instance.HasInteracted(sceneEntryID))
            {
                hasTriggered = true;
                Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] 已經觸發過（InteractionStateManager 確認），跳過");
                return;
            }
        }

        Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] 通過檢查，啟動延遲觸發（{delayTime}s）");
        StartCoroutine(DelayedTrigger());
    }

    private IEnumerator DelayedTrigger()
    {
        yield return new WaitForSeconds(delayTime);

        if (!hasTriggered)
        {
            TriggerEntry();
        }
    }

    /// <summary>
    /// 執行觸發內容（兩種模式都會走這裡）
    /// </summary>
    private void TriggerEntry()
    {
        if (onlyFirstTime && InteractionStateManager.Instance != null)
        {
            if (InteractionStateManager.Instance.HasInteracted(sceneEntryID))
            {
                Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] 觸發前再次確認：已觸發過，跳過");
                return;
            }
        }

        Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] ===== 觸發開始 =====");

        if (giveIcon && iconToGive != null)
        {
            GiveIconToPlayer();
        }

        if (playDialogue && inkJSON != null)
        {
            PlayDialogue();
        }

        if (triggerSound != null)
        {
            PlaySound(triggerSound);
        }

        // 記錄已觸發
        if (onlyFirstTime && InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.MarkAsInteracted(sceneEntryID);
            Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] 已記錄為觸發過");
        }

        hasTriggered = true;
        Debug.Log($"[SceneEntryTrigger] [{sceneEntryID}] ===== 觸發完成 =====");
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
        else
        {
            Debug.LogWarning($"[SceneEntryTrigger] 給予圖示失敗：{iconToGive.displayName}");
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
            Debug.LogWarning("[SceneEntryTrigger] 對話已經在播放中，無法觸發新對話");
            return;
        }

        Debug.Log($"[SceneEntryTrigger] 播放對話：Knot = {dialogueKnot}");
        DialogueManager.GetInstance().StartDialogue(inkJSON, this.gameObject, dialogueKnot);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SceneEntryTrigger] Trigger Sound 是 null");
            return;
        }

        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 1f);
        Debug.Log($"[SceneEntryTrigger] 播放音效：{clip.name}");
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
            Debug.LogWarning($"[SceneEntryTrigger] {gameObject.name}: 使用 OnSceneChange 模式，但未設定 targetSceneName！");
        }
    }
}