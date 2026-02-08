using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 
using TMPro;

public class ButtonTextHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("TextMeshPro 支持")]
    public TextMeshProUGUI text;

    [Header("Text Legacy 支持")]
    public Text legacyText; 

    [Header("顏色設定")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color pressedColor = Color.red;

    private void Awake()
    {
        
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>();

       
        if (legacyText == null)
            legacyText = GetComponentInChildren<Text>();

        
        SetColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetColor(normalColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetColor(pressedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetColor(hoverColor);
    }

    
    private void SetColor(Color color)
    {
        
        if (text != null)
        {
            text.color = color;
        }

        
        if (legacyText != null)
        {
            legacyText.color = color;
        }
    }
}