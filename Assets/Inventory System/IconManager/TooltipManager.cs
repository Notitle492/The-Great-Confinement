using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("設定")]
    public GameObject tooltipPrefab;       // Tooltip 預置物（內含 TextMeshPro）
    public Canvas targetCanvas;                  // 所屬 Canvas
    public Vector2 Offset = new Vector2(50f, 0f); // Tooltip 與 Slot 的間距
    
    private Dictionary<GameObject, GameObject> activeTooltips = new(); // Slot → Tooltip 實例

    /* private RectTransform currentTooltip;
    private TextMeshProUGUI tooltipText; */    

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;

        /* if (tooltipPrefab != null)
        {
            GameObject obj = Instantiate(tooltipPrefab, transform);
            currentTooltip = obj.GetComponent<RectTransform>();
            tooltipText = obj.GetComponentInChildren<TextMeshProUGUI>();
            currentTooltip.gameObject.SetActive(false);
        } */
    }

    /// 顯示 Tooltip 並自動放在 slot 旁邊

    public void ShowTooltip(GameObject slotObj, string text)
    {
        if (tooltipPrefab == null || targetCanvas == null) return;

        // 已存在的 Tooltip 不重複生成
        if (activeTooltips.ContainsKey(slotObj)) return;

        GameObject tooltip = Instantiate(tooltipPrefab, targetCanvas.transform);
        tooltip.GetComponentInChildren<TextMeshProUGUI>().text = text;
        tooltip.SetActive(true);

        // 設定位置（根據 Slot 的螢幕座標）
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.transform as RectTransform,
            screenPos + Offset,
            targetCanvas.worldCamera,
            out Vector2 localPos
        );
        tooltipRect.localPosition = localPos;

        activeTooltips[slotObj] = tooltip;


        // Tooltip 出現位置計算完後
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 canvasSize = (targetCanvas.transform as RectTransform).sizeDelta;

        // 邊界檢查（簡易）
        if (localPos.x + tooltipSize.x > canvasSize.x / 2)
            localPos.x = (canvasSize.x / 2) - tooltipSize.x;
        if (localPos.y - tooltipSize.y < -canvasSize.y / 2)
            localPos.y = (-canvasSize.y / 2) + tooltipSize.y;

        tooltipRect.localPosition = localPos;


    }

    public void HideTooltip(GameObject slotObj)
    {
        if (activeTooltips.TryGetValue(slotObj, out GameObject tooltip))
        {
            Destroy(tooltip);
            activeTooltips.Remove(slotObj);
        }
    }
}
