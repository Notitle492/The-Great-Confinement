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
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("MainMenu");
        }
    }

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        if (controls != null) //加入 null 檢查
        {
            controls.UI.Enable();
            controls.UI.Cancel.performed += OnCancelPressed; // 滑鼠右鍵
            controls.UI.ExitTo2D.performed += OnEscPressed; // Esc 鍵
        }
    }

    private void OnDisable()
    {
        if (controls != null) // 加入 null 檢查
        {
            controls.UI.Cancel.performed -= OnCancelPressed;
            controls.UI.ExitTo2D.performed -= OnEscPressed;
            controls.UI.Disable();
        }
    }

    /// 滑鼠右鍵：關閉當前面板
    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        if (currentActivePanel != null)
        {
            CloseCurrentPanel();
        }
    }


    /// Esc 鍵：從遊戲場景返回主選單（不重置進度）

    private void OnEscPressed(InputAction.CallbackContext context)
    {
        // 只在遊戲場景（非主選單）時才處理
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "MainMenu")
        {
            Debug.Log("[MainMenu] 玩家按 Esc 返回主選單（保留進度）");

            // 播放主選單音樂
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMusic("MainMenu");
            }

            // 只載入主選單，不重置進度
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// 開始按鈕：重置所有進度並開始新遊戲
    public void PlayGame()
    {
        Debug.Log("[MainMenu] 點擊開始按鈕，重置進度並開始新遊戲");

        // 開新遊戲前完整重置所有資料
        ResetGameProgress();

        /* LevelManager.Instance.LoadScene("2D", "MainMenu");
        MusicManager.Instance.PlayMusic("MainMenu"); */

        //// 開新遊戲前清除舊資料
        //if (IconManager.Instance != null)
        //{
        //    IconManager.Instance.ClearAllIcons();
        //}

        // 播放 2D 場景的音樂（請替換成你在 MusicLibrary 中設定的實際音樂名稱）
        // 改成你的 2D 場景音樂名稱
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("2D");
        }

        // 載入 2D 場景
        SceneManager.LoadScene("2D");
    }


    /// 新增：重置所有遊戲進度
    private void ResetGameProgress()
    {
        Debug.Log("[MainMenuController] 開始重置遊戲進度...");

        // 1. 清除所有圖示
        if (IconManager.Instance != null)
        {
            IconManager.Instance.ClearAllIcons();
            Debug.Log("[MainMenuController] 已清除所有圖示");
        }

        // 2. 清除背包
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.items.Clear();
            if (InventoryManager.Instance.inventoryUI != null)
            {
                InventoryManager.Instance.inventoryUI.UpdateUI(InventoryManager.Instance.items);
            }
            Debug.Log("[MainMenuController] 已清空背包");
        }

        // 3. 清除所有互動記錄（跨場景狀態）
        if (InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.ClearAllInteractions();
            Debug.Log("[MainMenuController] 已清除互動記錄");
        }

        // 4. 清除合成管理器的結果槽
        if (SynthesisManager.Instance != null)
        {
            SynthesisManager.Instance.ClearResultSlot();
            Debug.Log("[MainMenuController] 已清空合成結果槽");
        }

        // 5. TODO: 如果有對話系統的進度，也可以在這裡重置
        // 例如：DialogueManager.Instance?.ResetDialogueProgress();

        Debug.Log("[MainMenuController] 遊戲進度重置完成");
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

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
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
