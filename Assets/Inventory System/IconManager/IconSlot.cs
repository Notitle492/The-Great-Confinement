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

    private void Start()
    {
        // 確保 iconImage 被正確賦值
        if (iconImage == null)
        {
            Debug.LogWarning($"IconSlot {gameObject.name} 的 iconImage 尚未指向 Image！請拖入");
            iconImage = GetComponent<Image>();
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();
        }
        
        // 確保可以接收點擊
        if (iconImage != null)
            iconImage.raycastTarget = true;
        Debug.Log($"[IconSlot] Start - {gameObject.name}, HasImage: {iconImage != null}");
    }
    

    
    public void Setup(IconData data)
    {
        iconData = data;
        if (iconImage != null)
        {
            iconImage.sprite = data.iconSprite;
            iconImage.enabled = true;
        }
        
    }

    // ✅ 這裡保留一個就好
    public bool HasIcon() => iconData != null;


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
        Debug.Log($"[TEST] OnPointerClick 觸發，slot={name}, isSynthesis={isSynthesisSlot}");

        
        if (!HasIcon())
        {
            Debug.Log("[OnPointerClick] Slot 沒有圖示，忽略點擊");
            return;
        }

        // 無論是顯示區還是合成區，都使用切換邏輯
        Debug.Log($"[OnPointerClick] {iconData.id} 被點擊 → 執行切換邏輯");
        IconManager.Instance?.ToggleSynthesis(iconData);

        /* if (!isSynthesisSlot)
        {
            Debug.Log($"[OnPointerClick] 顯示區 {iconData.id} 被點擊 → 生成合成區分身");
            IconManager.Instance?.AddToSynthesisDuplicate(iconData);
        }
        else
        {
            Debug.Log($"[OnPointerClick] 合成區 {iconData.id} 被點擊 → 移除");
            // 合成區點擊 → 移除（由 IconManager 管理）
            IconManager.Instance?.RemoveFromSynthesis(iconData);
        } */
    }
}
