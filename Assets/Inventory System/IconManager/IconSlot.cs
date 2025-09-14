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

    public bool isSynthesisSlot = false; // 標記這個 Slot 是合成區還是顯示區


    [Header("對應的 Tooltip")]
    public GameObject tooltipObject;        // 直接拖入 chatbox(1/2/3...)
    public TextMeshProUGUI tooltipText;

    

    
    public void Setup(IconData data)
    {
        iconData = data;
        

        if (iconImage != null && data != null)
            iconImage.sprite = data.iconSprite;

        if (tooltipText != null)
            tooltipText.text = string.IsNullOrEmpty(data.displayName) ? data.id : data.displayName;

        if (tooltipObject != null)
            tooltipObject.SetActive(false); // 一開始隱藏
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
        // 只有在 slot 已經有圖示時才顯示 tooltip
        if (iconData == null || iconImage == null || iconImage.sprite == null)
            return;
                
        if (tooltipObject != null)
            tooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (iconData == null) return;

        if (!isSynthesisSlot)
        {
            // 顯示區圖示點擊 → 切換合成區
            IconManager.Instance.ToggleSynthesis(iconData);
        }
        else
        {
            // 合成區圖示點擊 → 移除自己
            IconManager.Instance.RemoveFromSynthesis(iconData);
        }
    }
}

