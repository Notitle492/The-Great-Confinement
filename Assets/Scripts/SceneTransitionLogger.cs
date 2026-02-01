using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 場景切換偵測 Logger（Debug 用）
/// 掛上後會自動跟蹤每次場景切換，並在 Console 印出詳細資訊。
/// 使用方式：建立一個空的 GameObject，取名 "SceneTransitionLogger"，掛上此腳本，
///           然後勾選 "DontDestroy"（預設開啟），讓它跨場景存活。
/// </summary>
public class SceneTransitionLogger : MonoBehaviour
{
    [Header("=== 設定 ===")]
    [Tooltip("是否跨場景保留此物件（建議開啟）")]
    public bool dontDestroy = true;

    
    private static SceneTransitionLogger _instance;
    private string _previousScene = "";
    private string _currentScene = "";
    private int _transitionCount = 0;

    private void Awake()
    {
        // 防止重複實例
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (dontDestroy)
        {
            DontDestroyOnLoad(gameObject);
        }

        // 記錄啟動時的場景
        _currentScene = SceneManager.GetActiveScene().name;
        _previousScene = "（起始）";

        Debug.Log($"[SceneLogger] ===== Logger 啟動 =====");
        Debug.Log($"[SceneLogger] 起始場景：{_currentScene}");

        // 注冊場景載入事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _previousScene = _currentScene;
        _currentScene = scene.name;
        _transitionCount++;

        // 組裝切換路徑字串
        string arrow = " 轉換下一個場景 ";
        string transitionPath = $"{_previousScene}{arrow}{_currentScene}";

        // 判斷是否是「可疑的」轉移（用於高亮警告）
        // 這裡先定義幾組你目前的預期路徑，如果不符就標記為警告
        bool isSuspicious = CheckSuspicious(_previousScene, _currentScene);

        if (isSuspicious)
        {
            Debug.LogWarning(
                $"[SceneLogger] 切換 #{_transitionCount} ：{transitionPath}" +
                $"\n  前一場景：{_previousScene}" +
                $"\n  目前場景：{_currentScene}" +
                $"\n  此切換可能不符合預期！請確認 SceneChanger 的 sceneToLoad 設定。"
            );
        }
        else
        {
            Debug.Log(
                $"[SceneLogger] 切換 #{_transitionCount} ：{transitionPath}" +
                $"\n  前一場景：{_previousScene}" +
                $"\n  目前場景：{_currentScene}"
            );
        }
    }

    /// <summary>
    /// 檢查場景切換組合是否可疑。
    /// 你可以在這裡手動維護「不應該發生的切換」組合，
    /// 便於快速在 Console 裡看到哪次切換出了問題。
    /// </summary>
    private bool CheckSuspicious(string from, string to)
    {
        // === 例如：從 RestroomOutside 2 出來不應該進 TheFirstCorridor（沒有 2） ===
        // === 從 MensRoom 2 出來不應該進 RestroomOutside（沒有 2） ===
        // 請根據你的實際場景名稱調整以下內容：

        if (from == "RestroomOutside 2" && to != "TheFirstCorridor 2")
            return true;

        if (from == "MensRoom 2" && to != "RestroomOutside 2")
            return true;

        // 如果從任何「情況2」的場景切換出來，但目標不含「2」，標記為可疑
        if (from.Contains("2") && !to.Contains("2") && to != "（起始）")
            return true;

        return false;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log($"[SceneLogger] Logger 被銷毀。");
    }
}