using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapeToMenu : MonoBehaviour
{
    private Controls controls;

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.ExitToMenu.performed += OnExit;
    }

    private void OnDisable()
    {
        controls.UI.ExitToMenu.performed -= OnExit;
        controls.UI.Disable();
    }

    private void OnExit(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("MainMenu"); // 或 buildIndex = 0，看你主選單是第幾個
    }
}
