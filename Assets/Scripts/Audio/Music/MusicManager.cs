using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "EnglishListeningTest")
        {
            StopMusic(1.0f); // 在聽力測試場景自動淡出並停止
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        
        AudioClip clip = musicLibrary.GetClipFromName(trackName);
        if (clip != null)
        {
            StartCoroutine(AnimateMusicCrossfade(clip, fadeDuration));
        }
    }

    public void StopMusic(float fadeDuration = 0.5f)
    {
        StartCoroutine(AnimateMusicFadeOut(fadeDuration));
    }

    IEnumerator AnimateMusicFadeOut(float fadeDuration)
    {
        float startVolume = musicSource.volume;
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            yield return null;
        }
        musicSource.Stop();
    }


    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1f, 0, percent);
            yield return null;
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
    }    
}
