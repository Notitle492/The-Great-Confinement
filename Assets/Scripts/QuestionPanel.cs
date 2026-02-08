using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionPanel : MonoBehaviour
{
    [Header("UI元件")]
    [SerializeField] private TextMeshProUGUI answerDisplayText; // 顯示答案的Text (如 "(A)")
    [SerializeField] private Button optionA;
    [SerializeField] private Button optionB;
    [SerializeField] private Button optionC;

    [Header("視覺設定")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.5f); // 淺黃色

    private string selectedAnswer = ""; // 儲存選擇的答案 "A", "B", "C"
    private Button currentSelectedButton = null;

    void Start()
    {
        // 初始化答案顯示
        answerDisplayText.text = " ";

        // 綁定按鈕事件
        optionA.onClick.AddListener(() => SelectOption("A", optionA));
        optionB.onClick.AddListener(() => SelectOption("B", optionB));
        optionC.onClick.AddListener(() => SelectOption("C", optionC));
    }

    void SelectOption(string answer, Button button)
    {
        // 重置之前選擇的按鈕顏色
        if (currentSelectedButton != null)
        {
            ResetButtonColor(currentSelectedButton);
        }

        // 記錄新的選擇
        selectedAnswer = answer;
        currentSelectedButton = button;

        // 更新答案顯示
        answerDisplayText.text = $"{answer}";

        // 設置選中按鈕的顏色
        ColorBlock colors = button.colors;
        colors.normalColor = selectedColor;
        colors.highlightedColor = selectedColor;
        button.colors = colors;

        Debug.Log($"玩家選擇: {answer}");
    }

    void ResetButtonColor(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = new Color(0.8f, 0.8f, 1f); // 淺藍色Hover
        button.colors = colors;
    }

    // 公開方法：獲取玩家答案
    public string GetSelectedAnswer()
    {
        return selectedAnswer;
    }

    // 公開方法：重置答題狀態
    public void ResetAnswer()
    {
        selectedAnswer = "";
        answerDisplayText.text = " ";

        if (currentSelectedButton != null)
        {
            ResetButtonColor(currentSelectedButton);
            currentSelectedButton = null;
        }
    }
}