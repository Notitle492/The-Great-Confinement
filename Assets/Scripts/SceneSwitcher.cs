using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    private Controls controls;

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.UI.SwitchScene.performed += OnSwitchScenePerformed;
        controls.UI.Enable();
    }

    private void OnDisable()
    {
        controls.UI.SwitchScene.performed -= OnSwitchScenePerformed;
        controls.UI.Disable();
    }

    private void OnSwitchScenePerformed(InputAction.CallbackContext context)
    {
        // 按下 Tab 後切換場景
        SceneManager.LoadScene("PuzzleUI");
    }
    
}
