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
        // 只卸載 PuzzleUI，不重新載入 2D
        if (SceneManager.GetSceneByName("PuzzleUI").isLoaded)
        {
            SceneManager.UnloadSceneAsync("PuzzleUI");
            Debug.Log("已卸載 PuzzleUI，回到 2D 場景");
        }
    }
}
