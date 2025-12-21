using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SkipToMainMenu : MonoBehaviour
{
    [Header("場景設定")]
    [Tooltip("要跳轉到的場景名稱")]
    public string targetSceneName = "MainMenu";

    [Header("影片設定")]
    [Tooltip("場景中的 VideoPlayer 組件")]
    public VideoPlayer videoPlayer;

    [Tooltip("影片播放完後是否自動切換場景")]
    public bool autoSwitchAfterVideo = true;

    private bool hasSkipped = false; // 防止重複觸發

    private void Start()
    {
        // 如果沒有手動指定 VideoPlayer，自動尋找
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
        }

        // 如果找到 VideoPlayer 且啟用自動切換
        if (videoPlayer != null && autoSwitchAfterVideo)
        {
            // 訂閱影片播放結束事件
            videoPlayer.loopPointReached += OnVideoFinished;
            Debug.Log("[SkipToMainMenu] 已訂閱影片結束事件");
        }
        else if (videoPlayer == null)
        {
            Debug.LogWarning("[SkipToMainMenu] 找不到 VideoPlayer，無法自動切換場景");
        }
    }

    /// <summary>
    /// 影片播放結束時的回調
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[SkipToMainMenu] 影片播放完畢，自動切換場景");
        ChangeToScene();
    }

    /// <summary>
    /// 跳過按鈕：直接進入主選單
    /// </summary>
    public void ChangeToScene()
    {
        if (hasSkipped)
        {
            Debug.LogWarning("[SkipToMainMenu] 已經跳過了，不重複執行");
            return;
        }

        hasSkipped = true;
        Debug.Log($"[SkipToMainMenu] 切換到場景：{targetSceneName}");

        // 停止影片播放（如果還在播放）
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // 載入場景
        SceneManager.LoadScene(targetSceneName);
    }

    
    private void OnDestroy()
    {
        // 清理事件訂閱
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
