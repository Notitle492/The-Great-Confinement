using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnTo2D : MonoBehaviour
{
    private Controls controls;

    private void Awake()
    {
        controls = new Controls();
        controls.UI.ExitTo2D.performed += OnExit; // 在這裡就綁事件
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        
    }

    private void OnDisable()
    {
        controls.UI.ExitTo2D.performed -= OnExit;
        controls.UI.Disable();
    }

    private void OnExit(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("2D"); // 或 buildIndex = 0，看你主選單是第幾個
    }
}
