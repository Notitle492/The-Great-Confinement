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
        // 之後合成功能可以放這裡
    }
}

