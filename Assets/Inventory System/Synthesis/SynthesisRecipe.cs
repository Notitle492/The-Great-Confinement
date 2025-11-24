using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 合成配方：定義哪些圖示組合可以合成新圖示
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Synthesis/Recipe")]
public class SynthesisRecipe : ScriptableObject
{
    [Header("合成材料")]
    [Tooltip("需要的圖示ID列表（順序不重要）")]
    public List<string> requiredIconIDs = new List<string>();
    
    [Header("合成結果")]
    [Tooltip("合成出的新圖示")]
    public IconData resultIcon;
    
    [Header("顯示設定")]
    [Tooltip("配方名稱（用於 Debug）")]
    public string recipeName;
    
    /// <summary>
    /// 檢查給定的圖示ID列表是否符合這個配方
    /// </summary>
    public bool Matches(List<string> iconIDs)
    {
        if (iconIDs == null || iconIDs.Count != requiredIconIDs.Count)
            return false;
        
        // 複製列表以避免修改原始資料
        List<string> tempRequired = new List<string>(requiredIconIDs);
        List<string> tempProvided = new List<string>(iconIDs);
        
        // 排序後比較（因為順序不重要）
        tempRequired.Sort();
        tempProvided.Sort();
        
        for (int i = 0; i < tempRequired.Count; i++)
        {
            if (tempRequired[i] != tempProvided[i])
                return false;
        }
        
        return true;
    }
}