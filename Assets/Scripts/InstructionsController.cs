using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InstructionsController : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup InstructionsPanel;
    public Image instructionImage; // 顯示說明圖片的 Image 元件
    public Button previousButton;  // 左邊的按鈕
    public Button nextButton;      // 右邊的按鈕
    public Button closeButton;     // 右上角的X按鈕

    [Header("Instruction Images")]
    public Sprite[] instructionSprites; // 5張說明圖片

    private int currentImageIndex = 0;
    // 紀錄目前是哪個子面板被開啟
    private CanvasGroup currentActivePanel;
    private Controls controls;

    private bool isChangingPage = false; // 新增：防止快速重複點擊


    private void Awake()
    {
        controls = new Controls();
    }

    private void Start()
    {

        // 確保一開始面板是隱藏的
        if (InstructionsPanel != null)
        {
            InstructionsPanel.alpha = 0;
            InstructionsPanel.blocksRaycasts = false;
        }

        // 設定按鈕事件
        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousImage);

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextImage);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseInstructions);
    }


    private void OnEnable()
    {
        if (controls != null) // 加入 null 檢查
        {
            controls.UI.Enable();
            controls.UI.Cancel.performed += OnCancelPressed;
        }
    }

    private void OnDisable()
    {
        if (controls != null) // 加入 null 檢查
        {
            controls.UI.Cancel.performed -= OnCancelPressed;
            controls.UI.Disable();
        }
    }

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        if (currentActivePanel != null)
        {
            CloseCurrentPanel();
        }
    }

    // 這個方法給 InstructionsButton 的 OnClick() 呼叫
    public void OpenInstructions()
    {
        if (InstructionsPanel == null)
        {
            Debug.LogWarning("InstructionsPanel 未設定！");
            return;
        }

        if (instructionSprites == null || instructionSprites.Length == 0)
        {
            Debug.LogWarning("instructionSprites 陣列是空的！");
            return;
        }

        currentImageIndex = 0; // 重置為第一張圖
        UpdateInstructionImage();
        OpenPanel(InstructionsPanel);
    }

    private void ShowPreviousImage()
    {
        // 如果正在換頁中，直接忽略這次點擊
        if (isChangingPage) return;

        if (currentImageIndex > 0)
        {
            isChangingPage = true;
            currentImageIndex--;
            UpdateInstructionImage();

            // 短暫延遲後才允許再次換頁（防止連點）
            StartCoroutine(ResetPageChangeCooldown());
        }
    }

    private void ShowNextImage()
    {
        // 如果正在換頁中，直接忽略這次點擊
        if (isChangingPage) return;

        if (currentImageIndex < instructionSprites.Length - 1)
        {
            isChangingPage = true;
            currentImageIndex++;
            UpdateInstructionImage();

            // 短暫延遲後才允許再次換頁（防止連點）
            StartCoroutine(ResetPageChangeCooldown());
        }
    }

    // 新增協程：冷卻時間結束後重置旗標
    private System.Collections.IEnumerator ResetPageChangeCooldown()
    {
        yield return new WaitForSeconds(0.3f); // 0.3秒內不允許再次換頁
        isChangingPage = false;
    }

    private void UpdateInstructionImage()
    {
        if (instructionImage != null && instructionSprites != null &&
        instructionSprites.Length > 0 &&
        currentImageIndex >= 0 && currentImageIndex < instructionSprites.Length)
        {
            instructionImage.sprite = instructionSprites[currentImageIndex];
        }

        // 更新按鈕的啟用狀態
        if (previousButton != null)
            previousButton.interactable = (currentImageIndex > 0);

        if (nextButton != null)
            nextButton.interactable = (instructionSprites != null && currentImageIndex < instructionSprites.Length - 1);
    }

    // 給關閉按鈕(X)呼叫的公開方法
    public void CloseInstructions()
    {
        CloseCurrentPanel();
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
        if (currentActivePanel != null)
        {
            currentActivePanel.alpha = 0;
            currentActivePanel.blocksRaycasts = false;
            currentActivePanel = null;
        }
    }
}
