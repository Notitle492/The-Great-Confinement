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

    public void SetItem(Item item)
    {
        currentItem = item;
        icon.sprite = item.ItemImage;
        icon.enabled = true;

        if (tooltipText != null)
            tooltipText.text = item.ItemName;
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
        if (tooltip != null)
            tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && tooltip != null)
            tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }
}
