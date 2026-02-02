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

    // ============================================================
    // 新增：指定要隱藏的 Persistent Canvas
    // 在這裡拖入 GameManager 的 persistentObjects 裡那個叫 Canvas 的物件
    // ============================================================
    [Header("Persistent Canvas 設定")]
    [Tooltip("拖入 persistentObjects 裡的 Canvas，這個會在切場景前被隱藏，避免蓋住後面的動畫場景")]
    public GameObject persistentCanvas;

    private bool hasSkipped = false;

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
        }

        if (videoPlayer != null && autoSwitchAfterVideo)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            Debug.Log("[SkipToMainMenu] 已訂閱影片結束事件");
        }
        else if (videoPlayer == null)
        {
            Debug.LogWarning("[SkipToMainMenu] 找不到 VideoPlayer，無法自動切換場景");
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[SkipToMainMenu] 影片播放完畢，自動切換場景");
        ChangeToScene();
    }

    public void ChangeToScene()
    {
        if (hasSkipped)
        {
            Debug.LogWarning("[SkipToMainMenu] 已經跳過了，不重複執行");
            return;
        }

        hasSkipped = true;
        Debug.Log($"[SkipToMainMenu] 切換到場景：{targetSceneName}");

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // ============================================================
        // 新增：隱藏 Persistent Canvas，避免蓋住後面的動畫場景
        // ============================================================
        if (persistentCanvas != null)
        {
            persistentCanvas.SetActive(false);
            Debug.Log($"[SkipToMainMenu] 已隱藏 Persistent Canvas：{persistentCanvas.name}");
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}