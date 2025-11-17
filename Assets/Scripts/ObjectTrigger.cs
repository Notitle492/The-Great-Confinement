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

        // 🔍 自動抓取 ItemTrigger 或 ItemTriggerStatic
        triggerScript = GetComponent<ItemTrigger>();

        if (triggerScript == null)
            triggerScript = GetComponent<ItemTriggerStatic>();

        if (triggerScript == null)
            Debug.LogWarning("ObjectTrigger: 找不到 ItemTrigger 或 ItemTriggerStatic！", this);

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

        if (!hasInteracted)
        {
            try
            {
                // ✨ 呼叫 Interact()（無論是 ItemTrigger 或 ItemTriggerStatic）
                triggerScript.Invoke("Interact", 0f);

                hasInteracted = true;

                if (visualCue != null)
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
