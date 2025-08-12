using UnityEngine;
using UnityEngine.InputSystem;  // 引用新輸入系統

public class ObjectTrigger : MonoBehaviour
{
    [Header("Visual Cue (e-letter icon)")]
    [SerializeField] private GameObject visualCue;

    private bool playerInRange = false;
    private bool hasInteracted = false;

    private ItemTrigger itemTrigger;  // 同物件上掛的ItemTrigger組件

    private PlayerInput playerInput;  // 用 PlayerInput 監聽
    

    private void Awake()
    {
        if (visualCue != null)
            visualCue.SetActive(false);

        // 取得同物件上的 ItemTrigger
        itemTrigger = GetComponent<ItemTrigger>();
        playerInput = FindObjectOfType<PlayerInput>(); // 找到全場的 PlayerInput
        if (itemTrigger == null)
        {
            Debug.LogWarning("ObjectTrigger: 找不到同物件上的 ItemTrigger 組件");
        }

        
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
        if (playerInRange && !hasInteracted)
        {
            if (itemTrigger != null)
            {
                itemTrigger.Interact();
                hasInteracted = true;
                if (visualCue != null)
                    visualCue.SetActive(false);
                Debug.Log("ObjectTrigger: 互動觸發");
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
