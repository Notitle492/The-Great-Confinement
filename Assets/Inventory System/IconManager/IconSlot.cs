using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image iconImage;      // 指向顯示圖示的 Image（在 prefab 裡 assign）
    private IconData iconData;

    // 呼叫此函式初始化格子
    public void Setup(IconData data)
    {
        iconData = data;
        if (iconImage != null && data != null)
            iconImage.sprite = data.iconSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (iconData == null) return;
        if (TooltipManager.Instance != null)
        {
            string nameToShow = string.IsNullOrEmpty(iconData.displayName) ? iconData.id : iconData.displayName;
            TooltipManager.Instance.Show(nameToShow, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance?.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果將來需要點擊把圖示送到合成區，可在這裡處理（目前先保留）
    }
}
