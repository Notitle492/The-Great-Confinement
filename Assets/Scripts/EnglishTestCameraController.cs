using System.Collections;
using UnityEngine;

public class EnglishTestCameraController : MonoBehaviour
{
    [Header("攝影機移動設定")]
    [SerializeField] private Transform cameraTarget; // CameraTarget的Transform
    [SerializeField] private float[] questionDurations; // 每題的持續時間（秒）- 在Inspector中設定
    [SerializeField] private float moveDistance = 5f; // 每次下移距離
    [SerializeField] private float moveDuration = 1f; // 移動動畫時間

    [Header("音檔控制")]
    [SerializeField] private AudioSource audioSource; // 播放整合音檔的AudioSource
    [SerializeField] private AudioClip fullTestAudio; // 完整的英聽音檔


    private int currentQuestion = 0;
    private bool isMoving = false;
    private int totalQuestions;

    void Start()
    {
        totalQuestions = questionDurations.Length;

        // 播放完整音檔
        if (audioSource != null && fullTestAudio != null)
        {
            audioSource.clip = fullTestAudio;
            audioSource.Play();
        }

        // 開始測驗流程
        StartCoroutine(QuestionSequence());
    }

    IEnumerator QuestionSequence()
    {
        for (int i = 0; i < totalQuestions; i++)
        {
            currentQuestion = i;
            Debug.Log($"第 {i + 1} 題開始，等待時間: {questionDurations[i]} 秒");

            // 等待當前題目的持續時間
            yield return new WaitForSeconds(questionDurations[i]);

            

            // 如果不是最後一題，移動攝影機到下一題
            if (i < totalQuestions - 1)
            {
                yield return StartCoroutine(MoveCameraDown());
            }
        }

        // 所有題目完成
        OnTestComplete();
    }

    IEnumerator MoveCameraDown()
    {
        isMoving = true;
        Vector3 startPos = cameraTarget.position;
        Vector3 endPos = startPos + new Vector3(0, -moveDistance, 0);

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            // 使用SmoothStep讓移動更平滑
            t = t * t * (3f - 2f * t);
            cameraTarget.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        cameraTarget.position = endPos;
        isMoving = false;

        Debug.Log($"攝影機已移動到第 {currentQuestion + 2} 題");
    }

    void OnTestComplete()
    {
        Debug.Log("英聽測驗完成！");

        

        // 這裡可以觸發測驗結束的事件，例如：
        // - 顯示結束畫面
        // - 切換到下一個場景
        // - 顯示成績
    }

    

    // 公開方法：暫停/繼續音檔（測試用）
    public void PauseAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeAudio()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
}