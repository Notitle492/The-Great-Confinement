using UnityEngine;
using UnityEngine.InputSystem;  // 引用新輸入系統
using UnityEngine.UI; // 確保引入

public class ObjectTrigger : MonoBehaviour
{
    [Header("Visual Cue (e-letter icon)")]
    [SerializeField] private GameObject visualCue;

    private bool playerInRange = false;
    private bool hasInteracted = false;

    private MonoBehaviour triggerScript; // 可能是 ItemTrigger 或 ItemTriggerStatic
    private PlayerInput playerInput;  // 用 PlayerInput 監聽
    

    private void Awake()
    {
        if (visualCue != null)
            visualCue.SetActive(false);

        // 🔍 自動抓取各種 Trigger 腳本（按優先順序）
        triggerScript = GetComponent<IconTrigger>();      // ✅ 新增：優先檢查 IconTrigger

        if (triggerScript == null)
            triggerScript = GetComponent<ItemTrigger>();

        if (triggerScript == null)
            triggerScript = GetComponent<ItemTriggerStatic>();

        if (triggerScript == null)
            Debug.LogWarning("ObjectTrigger: 找不到任何 Trigger 腳本！", this);

        playerInput = FindObjectOfType<PlayerInput>();


    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Interact"].performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Interact"].performed -= OnInteract;
        }
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (visualCue != null && !visualCue.activeSelf)
                visualCue.SetActive(true);
        }
        else
        {
            if (visualCue != null && visualCue.activeSelf)
                visualCue.SetActive(false);
        }

        
    }

    private void OnInteract(InputAction.CallbackContext context)
    {

        Debug.Log($"ObjectTrigger.OnInteract 觸發 - playerInRange={playerInRange}, hasInteracted={hasInteracted}");

        if (!playerInRange)
            return;

        if (triggerScript == null)
        {
            Debug.LogWarning("ObjectTrigger: 沒有可觸發的 triggerScript");
            return;
        }

        // ✅ IconTrigger 支援多次互動，所以不檢查 hasInteracted
        bool isIconTrigger = triggerScript is IconTrigger;

        if (!hasInteracted || isIconTrigger)
        {
            try
            {
                // ✨ 呼叫 Interact()
                triggerScript.Invoke("Interact", 0f);

                // 只有非 IconTrigger 才設定 hasInteracted
                if (!isIconTrigger)
                {
                    hasInteracted = true;
                }

                if (visualCue != null && !isIconTrigger)
                    visualCue.SetActive(false);

                Debug.Log("ObjectTrigger: 互動完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ObjectTrigger: 互動錯誤 - {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (visualCue != null)
                visualCue.SetActive(false);
        }
    }
}
