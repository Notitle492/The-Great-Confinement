using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class IconSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image iconImage;      
    private IconData iconData;
    public IconData IconData => iconData;


    [Header("元件標記")]
    public bool isSynthesisSlot = false; // 標記這個 Slot 是合成區還是顯示區


    [Header("對應的 Tooltip")]
    public GameObject tooltipObject;        // 直接拖入 chatbox(1/2/3...)
    public TextMeshProUGUI tooltipText;

    

    
    public void Setup(IconData data)
    {
        iconData = data;
        

        if (iconImage != null && data != null)
            iconImage.sprite = data.iconSprite;
        else if (iconImage != null && data == null)
            iconImage.sprite = null;

        if (tooltipText != null)
            tooltipText.text = data != null ? (string.IsNullOrEmpty(data.displayName) ? data.id : data.displayName) : "";

        if (tooltipObject != null)
            tooltipObject.SetActive(false); // 一開始隱藏
    }

    // 判斷這個 slot 是否已經有圖示（由 IconSlot 自己管理）
    public bool HasIcon()
    {
        return iconData != null;
    }

    // 清空 slot（placeholder 用），可傳入預設圖（null 表示清空）
    public void Clear(Sprite defaultSprite = null)
    {
        iconData = null;
        if (iconImage != null)
            iconImage.sprite = defaultSprite; 

        if (tooltipText != null)
            tooltipText.text = "";

        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
        // 只有在 slot 已經有圖示時才顯示 tooltip
        if (!HasIcon()) return;

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
        
        if (!HasIcon())
        {
            Debug.Log("[OnPointerClick] Slot 沒有圖示，忽略點擊");
            return;
        }

        if (!isSynthesisSlot)
        {
            Debug.Log($"[OnPointerClick] 顯示區 {iconData.id} 被點擊 → 嘗試加入合成區");
            // 顯示區點擊 → 切換合成區（由 IconManager 管理）
            IconManager.Instance?.ToggleSynthesis(iconData);
        }
        else
        {
            Debug.Log($"[OnPointerClick] 合成區 {iconData.id} 被點擊 → 移除");
            // 合成區點擊 → 移除（由 IconManager 管理）
            IconManager.Instance?.RemoveFromSynthesis(iconData);
        }
    }
}

