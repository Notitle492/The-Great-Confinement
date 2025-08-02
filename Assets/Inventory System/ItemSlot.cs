using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public GameObject tooltip;
    public TextMeshProUGUI tooltipText;

    private Item currentItem;

    public Sprite defaultSlotImage; // 拖入你希望的預設槽位圖片


    public void SetItem(Item item)
    {
        currentItem = item;
        icon.sprite = item.ItemImage;
        icon.enabled = true;

        /* if (tooltipText != null)
            tooltipText.text = item.ItemName; */
        
        // 不在這裡設置 tooltip，等滑鼠進入時再設置
        if (tooltip != null)
            tooltip.SetActive(false); // 一開始不顯示
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (icon != null)
        {
            icon.sprite = defaultSlotImage; // 換成預設空圖
            icon.color = Color.white;
        }
        /* icon.sprite = null;
        icon.enabled = false; */
        if (tooltip != null)
            tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && tooltip != null)
        {
            tooltip.SetActive(true); // 顯示 chatbox
            if (tooltipText != null)
                tooltipText.text = currentItem.ItemName; // 只在滑鼠進來時顯示名稱
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false); // 滑鼠離開就關閉
        }
    }
}
