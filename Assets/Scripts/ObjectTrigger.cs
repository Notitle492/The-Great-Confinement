using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    [Header("Visual Cue (e-letter icon)")]
    [SerializeField] private GameObject visualCue;

    private bool playerInRange = false;
    private bool hasInteracted = false;

    private ItemTrigger itemTrigger;  // 同物件上掛的ItemTrigger組件

    private void Awake()
    {
        if (visualCue != null)
            visualCue.SetActive(false);

        // 取得同物件上的 ItemTrigger
        itemTrigger = GetComponent<ItemTrigger>();
        if (itemTrigger == null)
        {
            Debug.LogWarning("ObjectTrigger: 找不到同物件上的 ItemTrigger 組件");
        }
    }

    private void Update()
    {
        if (playerInRange && !hasInteracted)
        {
            if (visualCue != null)
                visualCue.SetActive(true);

            // 按下 E 鍵，觸發互動
            if (InputManager.GetInstance() != null && InputManager.GetInstance().GetInteractPressed())
            {
                // 呼叫 ItemTrigger 的方法來處理新增物品和圖示
                if (itemTrigger != null)
                {
                    itemTrigger.OnDialogueEnded();  // 這裡名字是OnDialogueEnded，沒錯的話就是呼叫它
                    hasInteracted = true;

                    // 互動後隱藏提示
                    if (visualCue != null)
                        visualCue.SetActive(false);
                }
            }
        }
        else
        {
            if (visualCue != null)
                visualCue.SetActive(false);
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
