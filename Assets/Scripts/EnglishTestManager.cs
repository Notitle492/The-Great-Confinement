using System.Collections;
using UnityEngine;

public class EnglishTestManager : MonoBehaviour
{
    [Header("問題面板")]
    [SerializeField] private QuestionPanel[] questionPanels; // 5個問題面板

    [Header("攝影機控制")]
    [SerializeField] private EnglishTestCameraController cameraController;

    private int currentQuestionIndex = 0;
    private string[] playerAnswers = new string[5]; // 儲存玩家的答案

    void Start()
    {
        // 初始化
        for (int i = 0; i < playerAnswers.Length; i++)
        {
            playerAnswers[i] = "";
        }
    }

    // 在攝影機移動前記錄答案（可由CameraController呼叫）
    public void RecordCurrentAnswer()
    {
        if (currentQuestionIndex < questionPanels.Length)
        {
            playerAnswers[currentQuestionIndex] = questionPanels[currentQuestionIndex].GetSelectedAnswer();
            Debug.Log($"第{currentQuestionIndex + 1}題答案: {playerAnswers[currentQuestionIndex]}");
        }
        currentQuestionIndex++;
    }

    // 測驗結束時檢查答案
    public void CheckAllAnswers()
    {
        string[] correctAnswers = { "B", "A", "A", "?", "?" }; // 正確答案（第4、5題可能無標準答案）

        for (int i = 0; i < playerAnswers.Length; i++)
        {
            Debug.Log($"第{i + 1}題 - 玩家答案: {playerAnswers[i]}, 正確答案: {correctAnswers[i]}");
        }
    }
}