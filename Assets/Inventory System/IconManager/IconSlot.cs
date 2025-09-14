using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class IconSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler/* , IPointerClickHandler */
{
    public Image iconImage;      
    private IconData iconData;


    [Header("對應的 Tooltip")]
    public GameObject tooltipObject;        // 直接拖入 chatbox(1/2/3...)
    public TextMeshProUGUI tooltipText;

    /* private int tooltipIndex = -1; // ❌ 不再在 Inspector 設定，而是由 IconManager 動態指定 */

    
    public void Setup(IconData data/* , int assignedIndex */)
    {
        iconData = data;
        /* tooltipIndex = assignedIndex; */

        if (iconImage != null && data != null)
            iconImage.sprite = data.iconSprite;

        if (tooltipText != null)
            tooltipText.text = string.IsNullOrEmpty(data.displayName) ? data.id : data.displayName;

        if (tooltipObject != null)
            tooltipObject.SetActive(false); // 一開始隱藏
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        /* Debug.Log($"PointerEnter {name}, iconData={iconData}, tooltipIndex={tooltipIndex}");

        if (iconData == null) return;
        if (TooltipManager.Instance != null && tooltipIndex >= 0)
        {
            string nameToShow = string.IsNullOrEmpty(iconData.displayName) ? iconData.id : iconData.displayName;
            TooltipManager.Instance.Show(tooltipIndex, nameToShow);
        } */
        // 只有在 slot 已經有圖示時才顯示 tooltip
        if (iconData == null || iconImage == null || iconImage.sprite == null)
            return;
                
        if (tooltipObject != null)
            tooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        /* if (tooltipIndex >= 0)
            TooltipManager.Instance?.Hide(tooltipIndex); */
        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 之後合成功能可以放這裡
    }
}

