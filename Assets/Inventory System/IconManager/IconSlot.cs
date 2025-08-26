using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image iconImage;      
    private IconData iconData;

    private int tooltipIndex = -1; // ❌ 不再在 Inspector 設定，而是由 IconManager 動態指定

    
    public void Setup(IconData data, int assignedIndex)
    {
        iconData = data;
        tooltipIndex = assignedIndex;

        if (iconImage != null && data != null)
            iconImage.sprite = data.iconSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (iconData == null) return;
        if (TooltipManager.Instance != null && tooltipIndex >= 0)
        {
            string nameToShow = string.IsNullOrEmpty(iconData.displayName) ? iconData.id : iconData.displayName;
            TooltipManager.Instance.Show(tooltipIndex, nameToShow, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipIndex >= 0)
            TooltipManager.Instance?.Hide(tooltipIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 之後合成功能可以放這裡
    }
}

