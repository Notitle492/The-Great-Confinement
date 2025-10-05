using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    public AudioMixer audioMixer;

    public CanvasGroup SavePanel;
    public CanvasGroup SettingsPanel;

    // 紀錄目前是哪個子面板被開啟
    private CanvasGroup currentActivePanel;

    private Controls controls;

    private void Start()
    {
        LoadVolume();
        MusicManager.Instance.PlayMusic("MainMenu");
    }

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.Cancel.performed += OnCancelPressed;
    }

    private void OnDisable()
    {
        controls.UI.Cancel.performed -= OnCancelPressed;
        controls.UI.Disable();
    }

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        if (currentActivePanel != null)
        {
            CloseCurrentPanel();
        }
    }

    
    public void PlayGame()
    {

        /* LevelManager.Instance.LoadScene("2D", "MainMenu");
        MusicManager.Instance.PlayMusic("MainMenu"); */

        // 開新遊戲前清除舊資料
        if (IconManager.Instance != null)
        {
            IconManager.Instance.ClearAllIcons();
        }

        // 播放 2D 場景的音樂（請替換成你在 MusicLibrary 中設定的實際音樂名稱）
        MusicManager.Instance.PlayMusic("2D");  // ← 改成你的 2D 場景音樂名稱

        // 載入 2D 場景
        SceneManager.LoadScene("2D");
    }

    public void Save()
    {
        OpenPanel(SavePanel);        
    }

    public void Settings()
    {
        OpenPanel(SettingsPanel);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
    

    private void OpenPanel(CanvasGroup panel)
    {
        // 關閉目前開的面板（如果有）
        if (currentActivePanel != null)
        {
            currentActivePanel.alpha = 0;
            currentActivePanel.blocksRaycasts = false;
        }

        // 開啟新的面板
        panel.alpha = 1;
        panel.blocksRaycasts = true;
        currentActivePanel = panel;
    }

    private void CloseCurrentPanel()
    {
        currentActivePanel.alpha = 0;
        currentActivePanel.blocksRaycasts = false;
        currentActivePanel = null;
    }

    
    
}
