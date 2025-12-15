using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PuzzleUIController : MonoBehaviour
{
    public static PuzzleUIController Instance { get; private set; }

    private Controls controls;
    private bool isPuzzleUILoaded = false;

    [Header("允許開啟解謎介面的場景")]
    [Tooltip("留空則所有場景都允許")]
    public string[] allowedScenes = new string[] { };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Found duplicate PuzzleUIController. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.SwitchScene.performed += OnTabPressed;
        controls.UI.ExitTo2D.performed += OnEscPressed;
    }

    private void OnDisable()
    {
        controls.UI.SwitchScene.performed -= OnTabPressed;
        controls.UI.ExitTo2D.performed -= OnEscPressed;
        controls.UI.Disable();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 按 Tab 鍵：開啟/關閉 PuzzleUI
    /// </summary>
    private void OnTabPressed(InputAction.CallbackContext context)
    {
        // 檢查當前場景是否允許開啟
        if (!CanOpenPuzzleUI())
        {
            Debug.Log($"當前場景 '{SceneManager.GetActiveScene().name}' 不允許開啟解謎介面");
            return;
        }

        if (!isPuzzleUILoaded)
        {
            OpenPuzzleUI();
        }
        else
        {
            ClosePuzzleUI();
        }
    }

    /// <summary>
    /// 按 Esc 鍵：關閉 PuzzleUI
    /// </summary>
    private void OnEscPressed(InputAction.CallbackContext context)
    {
        if (isPuzzleUILoaded)
        {
            ClosePuzzleUI();
        }
    }

    /// <summary>
    /// 檢查當前場景是否允許開啟解謎介面
    /// </summary>
    private bool CanOpenPuzzleUI()
    {
        // 如果 allowedScenes 是空的，則所有場景都允許
        if (allowedScenes == null || allowedScenes.Length == 0)
        {
            return true;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // 檢查當前場景是否在允許列表中
        foreach (string sceneName in allowedScenes)
        {
            if (currentScene == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 開啟 PuzzleUI
    /// </summary>
    public void OpenPuzzleUI()
    {
        if (isPuzzleUILoaded)
        {
            Debug.LogWarning("PuzzleUI 已經開啟了");
            return;
        }

        SceneManager.LoadScene("PuzzleUI", LoadSceneMode.Additive);
        isPuzzleUILoaded = true;
        Debug.Log("已開啟 PuzzleUI");
    }

    /// <summary>
    /// 關閉 PuzzleUI
    /// </summary>
    public void ClosePuzzleUI()
    {
        if (!isPuzzleUILoaded)
        {
            Debug.LogWarning("PuzzleUI 尚未開啟");
            return;
        }

        Scene puzzleScene = SceneManager.GetSceneByName("PuzzleUI");
        if (puzzleScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync("PuzzleUI");
            isPuzzleUILoaded = false;
            Debug.Log("已關閉 PuzzleUI");
        }
    }

    /// <summary>
    /// 供外部查詢 PuzzleUI 是否開啟
    /// </summary>
    public bool IsPuzzleUIOpen()
    {
        return isPuzzleUILoaded;
    }
}
