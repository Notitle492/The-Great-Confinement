using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public TextMeshProUGUI text;

    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color pressedColor = Color.red;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>();

        text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        text.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        text.color = hoverColor;
    }
}
