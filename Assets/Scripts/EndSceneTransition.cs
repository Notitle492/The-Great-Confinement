using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class EndSceneTransition : MonoBehaviour
{
    [Header("場景切換")]
    [SerializeField] private string sceneToLoad = "EndCredits";

    [Header("淡出設定")]
    [SerializeField] private Animator fadeAnim;
    [SerializeField] private float fadeTime = 1f;

    [Header("監聽的 Timeline")]
    [SerializeField] private PlayableDirector targetDirector; // 拖入最後那段 Timeline

    void Start()
    {
        if (targetDirector != null)
            targetDirector.stopped += OnTimelineFinished;
    }

    void OnTimelineFinished(PlayableDirector director)
    {
        fadeAnim.Play("FadeToBlack");
        StartCoroutine(DelayFade());
    }

    IEnumerator DelayFade()
    {
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(sceneToLoad);
    }

    void OnDestroy()
    {
        if (targetDirector != null)
            targetDirector.stopped -= OnTimelineFinished;
    }
}