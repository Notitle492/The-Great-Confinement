using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 合成配方：定義哪些圖示組合可以合成新圖示
/// </summary>
[CreateAssetMenu(fileName = "NewSynthesisRecipe", menuName = "Synthesis/Recipe")]
public class SynthesisRecipe : ScriptableObject
{
    [Header("合成材料")]
    [Tooltip("需要的圖示ID列表（順序不重要）")]
    public List<string> requiredIconIDs = new List<string>();
    
    [Header("合成結果")]
    [Tooltip("合成出的新圖示")]
    public IconDataSO resultIconSO;

    [Header("顯示設定")]
    [Tooltip("配方名稱（用於 Debug）")]
    public string recipeName;

    [Header("替代材料移除")]
    [Tooltip("合成成功後，除了使用的材料外，還要移除的其他材料ID（用於多配方情況）")]
    public List<string> alternativeMaterialsToRemove = new List<string>();

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

    /// <summary>
    /// 取得所有需要從顯示區移除的圖示ID（包含使用的材料和替代材料）
    /// </summary>
    public List<string> GetAllMaterialsToRemove()
    {
        List<string> allMaterials = new List<string>(requiredIconIDs);

        // 加入替代材料
        foreach (var altMaterial in alternativeMaterialsToRemove)
        {
            if (!allMaterials.Contains(altMaterial))
            {
                allMaterials.Add(altMaterial);
            }
        }

        return allMaterials;
    }
}