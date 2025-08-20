using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IconType
{
    Dialogue,   // 對話圖示
    Object      // 物件圖示
}

[System.Serializable]
public class IconData
{
    public IconType iconType;
    public Sprite iconSprite;
    public string id;           // 唯一識別（避免重複）
    public string displayName;  // 顯示名稱（可選）

    public IconData(IconType type, Sprite sprite, string id, string name = null)
    {
        iconType = type;
        iconSprite = sprite;
        this.id = id;
        displayName = name;
    }
}
