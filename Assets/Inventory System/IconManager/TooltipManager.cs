using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("UI")]
    public GameObject tooltipObject;             // 你的 chatbox(1) GameObject（整個框）
    public TextMeshProUGUI tooltipText;          // chatbox(1) 裡的 TMP 文本
    public Canvas targetCanvas;                  // 放 PuzzleUI 的 Canvas（用來把螢幕座標轉 local）

    public Vector2 screenOffset = new Vector2(12f, -12f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (tooltipObject != null) tooltipObject.SetActive(false);
    }

    public void Show(string text, Vector2 screenPosition)
    {
        if (tooltipObject == null || tooltipText == null) return;

        tooltipObject.SetActive(true);
        tooltipText.text = text;

        // 將螢幕座標轉成 Canvas localPosition
        RectTransform canvasRect = targetCanvas.transform as RectTransform;
        Vector2 localPoint;
        // For Screen Space - Overlay, camera param should be null
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, 
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera, out localPoint);

        tooltipObject.transform.localPosition = localPoint + screenOffset;
    }

    public void Hide()
    {
        if (tooltipObject != null) tooltipObject.SetActive(false);
    }
}
