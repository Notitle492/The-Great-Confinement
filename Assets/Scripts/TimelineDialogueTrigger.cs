using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class TimelineDialogueTrigger : MonoBehaviour
{
    [Header("Timeline 設定")]
    public PlayableDirector playableDirector;

    [Header("播放設定")]
    [Tooltip("是否在場景載入時自動播放 Timeline")]
    public bool autoPlay = true;

    [Tooltip("延遲多少秒後播放（給場景時間初始化）")]
    public float playDelay = 0.5f;

    [Header("對話觸發時機")]
    [Tooltip("對話觸發模式：TimelineEnd=動畫播完才觸發，SpecificTime=指定時間點觸發")]
    public DialogueTriggerMode triggerMode = DialogueTriggerMode.SpecificTime;

    [Tooltip("在 Timeline 的第幾秒觸發對話（僅在 SpecificTime 模式有效）")]
    public float dialogueTriggerTime = 3f;

    public enum DialogueTriggerMode
    {
        TimelineEnd,    // Timeline 播完才觸發對話
        SpecificTime    // 在指定時間點觸發對話
    }

    [Header("對話設定")]
    public TextAsset inkJSON;
    public string dialogueKnot = "Chapter1";

    [Header("圖示設定")]
    public bool giveIconAfterDialogue = false;
    public IconDataSO iconToGive;

    [Header("場景切換設定")]
    public bool switchSceneAfterDialogue = false;
    public string targetSceneName = "";
    public float sceneTransitionDelay = 0.5f;

    private bool hasTriggered = false;
    private bool timelineDialogueTriggered = false; // 新增：避免重複觸發

    private void Start()
    {
        Debug.Log("========== [TimelineDialogueTrigger] Start() ==========");

        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();

            if (playableDirector == null)
            {
                playableDirector = FindObjectOfType<PlayableDirector>();
                Debug.Log($"[TimelineDialogueTrigger] 從場景中找到 PlayableDirector: {playableDirector?.gameObject.name}");
            }
        }

        if (playableDirector == null)
        {
            Debug.LogError("[TimelineDialogueTrigger] 找不到 PlayableDirector！");
            return;
        }

        if (playableDirector.playableAsset == null)
        {
            Debug.LogError("[TimelineDialogueTrigger] PlayableDirector 沒有設定 Timeline！");
            return;
        }

        Debug.Log($"[TimelineDialogueTrigger] Timeline: {playableDirector.playableAsset.name}");
        Debug.Log($"[TimelineDialogueTrigger] Ink JSON: {(inkJSON != null ? inkJSON.name : "NULL")}");
        Debug.Log($"[TimelineDialogueTrigger] Knot: {dialogueKnot}");
        Debug.Log($"[TimelineDialogueTrigger] 觸發模式: {triggerMode}");

        playableDirector.stopped += OnTimelineStopped;
        Debug.Log("[TimelineDialogueTrigger] 已訂閱 Timeline 停止事件");

        if (autoPlay)
        {
            StartCoroutine(PlayTimelineAfterDelay());
        }
    }

    private System.Collections.IEnumerator PlayTimelineAfterDelay()
    {
        Debug.Log($"[TimelineDialogueTrigger] 等待 {playDelay} 秒後播放 Timeline...");
        yield return new WaitForSeconds(playDelay);

        if (playableDirector != null)
        {
            Debug.Log("[TimelineDialogueTrigger] 開始播放 Timeline");
            playableDirector.Play();

            // 如果是指定時間觸發，啟動監聽
            if (triggerMode == DialogueTriggerMode.SpecificTime)
            {
                StartCoroutine(WaitForTimelineTriggerTime());
            }
        }
    }

    /// <summary>
    /// 新增：等待 Timeline 播放到指定時間點
    /// </summary>
    private System.Collections.IEnumerator WaitForTimelineTriggerTime()
    {
        Debug.Log($"[TimelineDialogueTrigger] 等待 Timeline 播放到 {dialogueTriggerTime} 秒...");

        while (playableDirector.time < dialogueTriggerTime && playableDirector.state == PlayState.Playing)
        {
            yield return null;
        }

        if (!timelineDialogueTriggered)
        {
            Debug.Log($"[TimelineDialogueTrigger] 已到達觸發時間點 ({dialogueTriggerTime} 秒)，觸發對話");
            timelineDialogueTriggered = true;
            TriggerDialogue();
        }
    }

    private void Update()
    {
        // 測試用：按 T 鍵手動觸發對話
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("[TimelineDialogueTrigger] ===== 手動測試觸發 (T 鍵) =====");
            TriggerDialogue();
        }
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineStopped;
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        Debug.Log("========== [TimelineDialogueTrigger] OnTimelineStopped ==========");

        // 只有在 TimelineEnd 模式才觸發對話
        if (triggerMode == DialogueTriggerMode.TimelineEnd && !hasTriggered)
        {
            TriggerDialogue();
        }
    }

    /// <summary>
    /// 新增：統一的對話觸發方法
    /// </summary>
    private void TriggerDialogue()
    {
        if (hasTriggered)
        {
            Debug.LogWarning("[TimelineDialogueTrigger] 已經觸發過，跳過");
            return;
        }

        hasTriggered = true;

        if (inkJSON == null)
        {
            Debug.LogError("[TimelineDialogueTrigger] inkJSON 是 null，無法播放對話");

            if (switchSceneAfterDialogue && !string.IsNullOrEmpty(targetSceneName))
            {
                StartCoroutine(TransitionToScene());
            }
            return;
        }

        StartCoroutine(WaitForDialogueManager());
    }

    private System.Collections.IEnumerator WaitForDialogueManager()
    {
        Debug.Log("[TimelineDialogueTrigger] 等待 DialogueManager 初始化...");

        float waitTime = 0f;
        while (DialogueManager.GetInstance() == null && waitTime < 3f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (DialogueManager.GetInstance() == null)
        {
            Debug.LogError("[TimelineDialogueTrigger] DialogueManager 不存在（等待 3 秒後仍未找到）");
            yield break;
        }

        Debug.Log("[TimelineDialogueTrigger] DialogueManager 已找到，開始播放對話");
        DialogueManager.GetInstance().StartDialogue(inkJSON, this.gameObject, dialogueKnot);

        StartCoroutine(WaitForDialogueEnd());
    }

    private System.Collections.IEnumerator WaitForDialogueEnd()
    {
        Debug.Log("[TimelineDialogueTrigger] 等待對話開始...");

        float waitTime = 0f;
        while (!DialogueManager.GetInstance().dialogueIsPlaying && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            Debug.LogWarning("[TimelineDialogueTrigger] 對話未能開始，超時");
            OnDialogueEnded();
            yield break;
        }

        Debug.Log("[TimelineDialogueTrigger] 對話進行中，等待結束...");
        yield return new WaitUntil(() => !DialogueManager.GetInstance().dialogueIsPlaying);

        Debug.Log("[TimelineDialogueTrigger] 對話結束");
        OnDialogueEnded();
    }

    private void OnDialogueEnded()
    {
        Debug.Log("[TimelineDialogueTrigger] OnDialogueEnded 執行");

        if (giveIconAfterDialogue && iconToGive != null)
        {
            GiveIcon();
        }

        if (switchSceneAfterDialogue && !string.IsNullOrEmpty(targetSceneName))
        {
            StartCoroutine(TransitionToScene());
        }
    }

    private void GiveIcon()
    {
        if (IconManager.Instance == null)
        {
            Debug.LogWarning("[TimelineDialogueTrigger] IconManager 不存在");
            return;
        }

        IconData newIcon = iconToGive.ToIconData();
        bool added = IconManager.Instance.AddIcon(newIcon);

        if (added)
        {
            Debug.Log($"[TimelineDialogueTrigger] 給予圖示：{iconToGive.displayName}");
        }
        else
        {
            Debug.Log($"[TimelineDialogueTrigger] 圖示已存在：{iconToGive.displayName}");
        }
    }

    private System.Collections.IEnumerator TransitionToScene()
    {
        Debug.Log($"[TimelineDialogueTrigger] 等待 {sceneTransitionDelay} 秒後切換場景");
        yield return new WaitForSeconds(sceneTransitionDelay);

        Debug.Log($"[TimelineDialogueTrigger] 切換到場景：{targetSceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
    }
}