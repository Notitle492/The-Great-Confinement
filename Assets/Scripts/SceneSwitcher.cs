using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    private Controls controls;
    private bool puzzleUILoaded = false;

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.SwitchScene.performed += OnSwitchScenePerformed; // Tab
        controls.UI.ExitTo2D.performed += OnExitTo2DPerformed;       // 右鍵
    }

    private void OnDisable()
    {
        controls.UI.SwitchScene.performed -= OnSwitchScenePerformed;
        controls.UI.ExitTo2D.performed -= OnExitTo2DPerformed;
        controls.UI.Disable();
    }

    // 按 Tab：載入 PuzzleUI (Additive)
    private void OnSwitchScenePerformed(InputAction.CallbackContext context)
    {
        if (!puzzleUILoaded)
        {
            SceneManager.LoadScene("PuzzleUI", LoadSceneMode.Additive);
            puzzleUILoaded = true;
            Debug.Log("載入 PuzzleUI (Additive)");
        }
    }

    // 按右鍵：卸載 PuzzleUI
    private void OnExitTo2DPerformed(InputAction.CallbackContext context)
    {
        if (puzzleUILoaded)
        {
            SceneManager.UnloadSceneAsync("PuzzleUI");
            puzzleUILoaded = false;
            Debug.Log("卸載 PuzzleUI，回到 2D");
        }
    }
}
