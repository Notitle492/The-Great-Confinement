using UnityEngine;

/// <summary>
/// IconData 的 ScriptableObject 版本
/// 可以在 Unity Inspector 中創建和編輯
/// </summary>
[CreateAssetMenu(fileName = "NewIconData", menuName = "IconSystem/IconData")]
public class IconDataSO : ScriptableObject
{
    [Header("圖示類型")]
    public IconType iconType;
    
    [Header("圖示資料")]
    public Sprite iconSprite;
    
    [Tooltip("唯一識別ID")]
    public string id;
    
    [Tooltip("顯示名稱")]
    public string displayName;
    
    [Tooltip("圖示描述")]
    [TextArea(2, 4)]
    public string itemDescription;
    
    /// <summary>
    /// 轉換成 IconData（用於系統內部）
    /// </summary>
    public IconData ToIconData()
    {
        return new IconData(iconType, iconSprite, id, displayName, itemDescription);
    }
}

// ✅ 保留原本的 IconData class（系統內部使用）
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
    public string id;
    public string displayName;
    public string itemDescription;

    public IconData(IconType type, Sprite sprite, string id, string name = null, string description = "")
    {
        iconType = type;
        iconSprite = sprite;
        this.id = id;
        displayName = name;
        itemDescription = description;
    }
}