using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class MainMenuController : MonoBehaviour
{

    public CanvasGroup SavePanel;
    public CanvasGroup SettingsPanel;

    // 紀錄目前是哪個子面板被開啟
    private CanvasGroup currentActivePanel;

    private Controls controls;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
