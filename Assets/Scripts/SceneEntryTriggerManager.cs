using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 統一管理所有 OnSceneChange 模式的 SceneEntryTrigger。
/// 這個 Manager 自己 DontDestroyOnLoad，集中維持一個正確的 lastSceneName，
/// 避免多個 Trigger 各自記錄導致互相干扰的問題。
/// 
/// 使用方式：
/// 1. 將此腳本掛在一個 GameObject 上，勾選不會被刪除。
/// 2. 將場景裡原本用 OnSceneChange 模式的 SceneEntryTrigger，
///    改成用 RegisterTrigger() 注冊到這個 Manager 裡（見下方說明）。
/// </summary>
public class SceneEntryTriggerManager : MonoBehaviour
{
    public static SceneEntryTriggerManager Instance { get; private set; }

    // 儲存所有注冊過的觸發資訊
    private List<TriggerRegistration> registeredTriggers = new List<TriggerRegistration>();

    // 唯一、正確的前一個場景記錄
    private string lastSceneName = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        lastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"[SceneEntryTriggerManager] 已初始化，起始場景：{lastSceneName}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Additive 加載的場景（例如 PuzzleUI）不應該影響 lastSceneName，直接跳過
        if (mode == LoadSceneMode.Additive)
        {
            Debug.Log($"[SceneEntryTriggerManager] Additive 場景載入：{scene.name}，跳過（不影響場景切換記錄）");
            return;
        }

        string currentScene = scene.name;

        Debug.Log($"[SceneEntryTriggerManager] 場景切換：{lastSceneName}  {currentScene}");

        // 遍歷所有注冊的觸發，檢查哪個該觸發
        foreach (var reg in registeredTriggers)
        {
            // 已經觸發過且是一次觸發模式，跳過
            if (reg.hasTriggered && reg.onlyFirstTime)
                continue;

            // 目標場景不符，跳過
            if (currentScene != reg.targetSceneName)
                continue;

            // 有設定 previousSceneName 但不符，跳過
            if (!string.IsNullOrEmpty(reg.previousSceneName) && lastSceneName != reg.previousSceneName)
            {
                Debug.Log($"[SceneEntryTriggerManager] [{reg.id}] 前一個場景不符（需要：{reg.previousSceneName}，實際：{lastSceneName}），跳過觸發");
                continue;
            }

            // 符合條件，啟動觸發
            Debug.Log($"[SceneEntryTriggerManager] [{reg.id}] 符合觸發條件，啟動延遲觸發");
            StartCoroutine(DelayedTrigger(reg));
        }

        // 更新前一個場景（只在這裡更新一次，不會被多個物件覆蓋）
        lastSceneName = currentScene;
    }

    private IEnumerator DelayedTrigger(TriggerRegistration reg)
    {
        yield return new WaitForSeconds(reg.delayTime);

        if (reg.hasTriggered && reg.onlyFirstTime)
            yield break;

        // 檢查 InteractionStateManager 是否已經記錄過
        if (reg.onlyFirstTime && InteractionStateManager.Instance != null)
        {
            if (InteractionStateManager.Instance.HasInteracted(reg.id))
            {
                reg.hasTriggered = true;
                Debug.Log($"[SceneEntryTriggerManager] [{reg.id}] 已經觸發過（由 InteractionStateManager 確認），跳過");
                yield break;
            }
        }

        // 執行觸發回調前，先檢查來源物件是否還在
        if (reg.sourceObject == null)
        {
            Debug.LogWarning($"[SceneEntryTriggerManager] [{reg.id}] 來源物件已經被銷毀，無法觸發。請確認該 SceneEntryTrigger 是否應該在此場景裡存在。");
            yield break;
        }

        // 執行觸發回調
        reg.onTrigger?.Invoke();
        reg.hasTriggered = true;

        // 記錄到 InteractionStateManager
        if (reg.onlyFirstTime && InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.MarkAsInteracted(reg.id);
            Debug.Log($"[SceneEntryTriggerManager] [{reg.id}] 已記錄為觸發過");
        }

        Debug.Log($"[SceneEntryTriggerManager] [{reg.id}] 觸發完成！");
    }

    /// <summary>
    /// 注冊一個觸發項目。
    /// 從原本的 SceneEntryTrigger（OnSceneChange 模式）裡呼叫此方法，
    /// 將觸發資訊交給 Manager 統一管理。
    /// </summary>
    public void RegisterTrigger(TriggerRegistration registration)
    {
        if (registration == null)
        {
            Debug.LogWarning("[SceneEntryTriggerManager] 注冊的觸發資訊是 null");
            return;
        }

        // 防止重複注冊
        foreach (var existing in registeredTriggers)
        {
            if (existing.id == registration.id)
            {
                Debug.LogWarning($"[SceneEntryTriggerManager] [{registration.id}] 已經注冊過，跳過");
                return;
            }
        }

        // 檢查是否已經觸發過（從 InteractionStateManager 確認）
        if (registration.onlyFirstTime && InteractionStateManager.Instance != null)
        {
            if (InteractionStateManager.Instance.HasInteracted(registration.id))
            {
                registration.hasTriggered = true;
            }
        }

        registeredTriggers.Add(registration);
        Debug.Log($"[SceneEntryTriggerManager] 注冊觸發：[{registration.id}] 目標場景：{registration.targetSceneName}, 前一場景：{(string.IsNullOrEmpty(registration.previousSceneName) ? "不限" : registration.previousSceneName)}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

/// <summary>
/// 觸發注冊資訊（數據類別）
/// </summary>
public class TriggerRegistration
{
    public string id;                       // 唯一ID（對應 sceneEntryID）
    public string targetSceneName;          // 要偵測的目標場景
    public string previousSceneName;        // 前一個場景（留空則不檢查）
    public bool onlyFirstTime = true;       // 是否只觸發一次
    public float delayTime = 1f;            // 延遲觸發時間
    public bool hasTriggered = false;       // 是否已觸發過

    public GameObject sourceObject;         // 注冊此觸發的 SceneEntryTrigger 所在的 GameObject
    public System.Action onTrigger;         // 觸發時要執行的回調
}