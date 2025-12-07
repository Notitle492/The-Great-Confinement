using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 管理圖示合成系統
/// </summary>
public class SynthesisManager : MonoBehaviour
{
    public static SynthesisManager Instance { get; private set; }

    [Header("合成配方")]
    [Tooltip("所有可用的合成配方")]
    public List<SynthesisRecipe> recipes = new List<SynthesisRecipe>();

    [Header("UI 引用")]
    [Tooltip("合成按鈕（箭頭按鈕）")]
    public Button synthesisButton;

    [Tooltip("合成結果槽位（最右邊的槽位）")]
    public IconSlot resultSlot;

    [Tooltip("錯誤訊息文字（顯示「不存在此配對」）")]
    public TextMeshProUGUI errorMessageText;

    [Header("音效設定（可選）")]
    public AudioClip successSound;
    public AudioClip failSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 綁定合成按鈕點擊事件
        if (synthesisButton != null)
        {
            synthesisButton.onClick.AddListener(OnSynthesisButtonClicked);
            Debug.Log("[SynthesisManager] 合成按鈕已綁定");
        }
        else
        {
            Debug.LogWarning("[SynthesisManager] synthesisButton 未指定！");
        }

        // 初始隱藏錯誤訊息
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 當玩家點擊合成按鈕時
    /// </summary>
    public void OnSynthesisButtonClicked()
    {
        Debug.Log("[SynthesisManager] 合成按鈕被點擊");

        // 隱藏舊的錯誤訊息
        HideErrorMessage();

        // 取得合成區的圖示
        List<IconData> synthesisIcons = new List<IconData>(IconManager.Instance.GetSynthesisHistory());

        // 檢查是否有足夠的圖示
        if (synthesisIcons.Count < 2)
        {
            Debug.LogWarning("[SynthesisManager] 合成區圖示不足（需要2個）");
            ShowErrorMessage("請放入兩個圖示");
            return;
        }

        if (synthesisIcons.Count > 2)
        {
            Debug.LogWarning("[SynthesisManager] 合成區圖示過多");
            ShowErrorMessage("只能放入兩個圖示");
            return;
        }

        // 轉換成 ID 列表
        List<string> iconIDs = new List<string>();
        foreach (var icon in synthesisIcons)
        {
            iconIDs.Add(icon.id);
        }

        Debug.Log($"[SynthesisManager] 嘗試合成：{string.Join(" + ", iconIDs)}");

        // 尋找匹配的配方
        SynthesisRecipe matchedRecipe = FindMatchingRecipe(iconIDs);

        if (matchedRecipe != null)
        {
            // 合成成功
            Debug.Log($"[SynthesisManager] 合成成功！配方：{matchedRecipe.recipeName}");
            PerformSynthesis(matchedRecipe, synthesisIcons);
        }
        else
        {
            // 合成失敗
            Debug.Log("[SynthesisManager] 找不到匹配的配方");
            ShowErrorMessage("不存在此配對");
            PlaySound(failSound);
        }
    }

    /// <summary>
    /// 尋找匹配的配方
    /// </summary>
    private SynthesisRecipe FindMatchingRecipe(List<string> iconIDs)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.Matches(iconIDs))
            {
                return recipe;
            }
        }
        return null;
    }

    /// <summary>
    /// 執行合成
    /// </summary>
    private void PerformSynthesis(SynthesisRecipe recipe, List<IconData> usedIcons)
    {
        // 檢查配方是否有結果
        if (recipe.resultIconSO == null)
        {
            Debug.LogError($"配方 {recipe.recipeName} 沒有指定結果圖示！");
            return;
        }
        if (string.IsNullOrEmpty(recipe.resultIconSO.id) || recipe.resultIconSO.iconSprite == null)
        {
            Debug.LogError($"配方 {recipe.recipeName} 的結果圖示資料不完整");
            return;
        }

        // 轉換 ScriptableObject 為 IconData
        IconData resultIcon = recipe.resultIconSO.ToIconData();

        // 1. 移除合成區的材料圖示
        foreach (var icon in usedIcons)
        {
            IconManager.Instance.RemoveFromSynthesis(icon);
        }

        // 2. ✅ 從顯示區移除所有相關材料（包含替代材料）
        List<string> allMaterialsToRemove = recipe.GetAllMaterialsToRemove();
        foreach (var materialID in allMaterialsToRemove)
        {
            bool removed = IconManager.Instance.RemoveIconByID(materialID);
            if (removed)
            {
                Debug.Log($"[SynthesisManager] 已從顯示區移除材料：{materialID}");
            }
        }

        // 3. 在結果槽顯示新圖示
        if (resultSlot != null)
        {
            resultSlot.Setup(resultIcon);
            Debug.Log($"[SynthesisManager] 結果槽顯示：{resultIcon.displayName}");
        }

        // 4. 將新圖示解鎖到顯示區
        bool added = IconManager.Instance.AddIcon(resultIcon);
        if (added)
        {
            Debug.Log($"[SynthesisManager] 新圖示已解鎖：{resultIcon.displayName}");
        }
        else
        {
            Debug.Log($"[SynthesisManager] 圖示已存在：{resultIcon.displayName}");
        }

        // 5. 播放成功音效
        PlaySound(successSound);
    }

    /// <summary>
    /// 顯示錯誤訊息
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);

            // 3秒後自動隱藏
            CancelInvoke(nameof(HideErrorMessage));
            Invoke(nameof(HideErrorMessage), 3f);
        }
    }

    /// <summary>
    /// 隱藏錯誤訊息
    /// </summary>
    private void HideErrorMessage()
    {
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(clip.name);
        }
    }

    /// <summary>
    /// 清空結果槽（給外部呼叫，例如玩家點擊結果圖示後）
    /// </summary>
    public void ClearResultSlot()
    {
        if (resultSlot != null)
        {
            resultSlot.Clear(null);
            Debug.Log("[SynthesisManager] 結果槽已清空");
        }
    }
}