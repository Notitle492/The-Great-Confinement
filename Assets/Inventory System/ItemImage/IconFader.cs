using UnityEngine;
using System.Collections; // 一定要加，使用 Coroutine 需要

public class IconFader : MonoBehaviour
{
    public CanvasGroup canvasGroup; // 把你的圖示物件的CanvasGroup拖到這裡

    public float fadeDuration = 1f; // 淡出時間1秒

    private void Awake()
    {
        // 初始透明
        canvasGroup.alpha = 0f;
    }


    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    
    // 開始淡出呼叫這個函式
    public void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    public IEnumerator FadeOutCoroutine()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;  // 等待下一幀繼續執行
        }

        // 確保完全透明
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false; // 不可互動
        canvasGroup.blocksRaycasts = false; // 不擋點擊
    }
}
