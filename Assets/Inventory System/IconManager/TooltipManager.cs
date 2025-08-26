using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("UI")]
    public List<GameObject> tooltipObjects;       // 拖進 chatbox(1)~chatbox(5)
    public List<TextMeshProUGUI> tooltipTexts;    // 對應每個 chatbox 的文字
    public Canvas targetCanvas;                  // 放 PuzzleUI 的 Canvas（用來把螢幕座標轉 local）

    public Vector2 screenOffset = new Vector2(12f, -12f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 一開始把所有 tooltip 關閉
        foreach (var obj in tooltipObjects)
            if (obj != null) obj.SetActive(false);
    }

    public void Show(int index, string text, Vector2 screenPosition)
    {
        Debug.Log($"Tooltip.Show index={index}, text={text}, pos={screenPosition}");

        if (index < 0 || index >= tooltipObjects.Count) return;
        if (tooltipObjects[index] == null || tooltipTexts[index] == null) return;

        tooltipObjects[index].SetActive(true);
        tooltipTexts[index].text = text;

        // 把螢幕座標轉成 Canvas 的 localPosition
        RectTransform canvasRect = targetCanvas.transform as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
            out localPoint);

        tooltipObjects[index].transform.localPosition = localPoint + screenOffset;

        // 🔹 確保 tooltip 顯示在最上層
        tooltipObjects[index].transform.SetAsLastSibling();
        
    }

    /// <summary>隱藏指定 index 的 tooltip</summary>
    public void Hide(int index)
    {
        if (index < 0 || index >= tooltipObjects.Count) return;
        if (tooltipObjects[index] != null)
            tooltipObjects[index].SetActive(false);
    }
}
